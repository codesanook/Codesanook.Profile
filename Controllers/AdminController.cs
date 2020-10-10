using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.Users.Events;
using Orchard.Users.Services;
using Orchard.Users.ViewModels;
using Orchard.Users;
using Orchard.Data;
using NHibernate.Transform;
using NHibernate.Criterion;
using Codesanook.Users.Models;
using Orchard.ContentManagement.Records;
using NHibernate.SqlCommand;
using Orchard.Users.Models;
using UserIndexOptions = Codesanook.Users.ViewModels.UserIndexOptions;
using UserIndexViewModel = Codesanook.Users.ViewModels.UsersIndexViewModel;
using Orchard.ContentManagement.MetaData;
using NHibernate;
using Orchard.ContentManagement;
using Orchard.Utility.Extensions;
using Orchard.Mvc.Extensions;
using Orchard.UI.Notify;
using System;

namespace Codesanook.Users.Controllers {

    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IMembershipService membershipService;
        private readonly IUserService userService;
        private readonly IUserEventHandler userEventHandlers;
        private readonly ISiteService siteService;
        private readonly ITransactionManager transactionManager;
        private readonly IContentDefinitionManager contentDefinitionManager;
        private readonly IOrchardServices service;
        private dynamic Shape { get; set; }

        private const string activationEmailKey = "activationEmail";

        public Localizer T { get; set; }

        public AdminController(
            IOrchardServices service,
            IMembershipService membershipService,
            IUserService userService,
            IShapeFactory shapeFactory,
            IUserEventHandler userEventHandlers,
            ISiteService siteService,
            ITransactionManager transactionManager,
            IContentDefinitionManager contentDefinitionManager
        ) {
            this.service = service;
            this.membershipService = membershipService;
            this.userService = userService;
            this.userEventHandlers = userEventHandlers;
            this.siteService = siteService;
            this.transactionManager = transactionManager;
            this.contentDefinitionManager = contentDefinitionManager;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        public ActionResult Index(UserIndexOptions options, PagerParameters pagerParameters) {
            if (!service.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to list users"))) {
                return new HttpUnauthorizedResult();
            }

            var userType = contentDefinitionManager.GetTypeDefinition("User");
            var hasBasicUserProfilePart = userType.Parts.Any(p => p.PartDefinition.Name == nameof(BasicUserProfilePart));

            // Create criteria
            var session = transactionManager.GetSession();
            var query = session.CreateCriteria<ContentItemVersionRecord>("version")
                .CreateAlias("version.ContentItemRecord", "item", JoinType.InnerJoin)
                // content item is aggregation of part records
                .CreateAlias("item.ContentType", "type", JoinType.InnerJoin)
                .CreateAlias("item.UserPartRecord", "user", JoinType.InnerJoin)
                .Add(Restrictions.Eq("type.Name", "User"))
                .Add(Restrictions.Eq("version.Published", true));

            if (hasBasicUserProfilePart) {
                query.CreateAlias(
                    "item.BasicUserProfilePartRecord",
                    "profile",
                    JoinType.LeftOuterJoin
                ); // Not all user have profiles, e.g Admin
            }
            SetFilter(options, query, hasBasicUserProfilePart);

            var totalItemCount = query.SetProjection(Projections.RowCount()).UniqueResult<int>();
            var pager = new Pager(siteService.GetSiteSettings(), pagerParameters);
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalItemCount);

            SetProjectionList(hasBasicUserProfilePart, query);
            // FirstResult is row index start from 0 
            // pager.GetStartIndex() is calculated row index that we use
            // If we want page number, use pager.Page.
            query.SetFirstResult(pager.GetStartIndex()).SetMaxResults(pager.PageSize);
            SetSortOrder(options, query, hasBasicUserProfilePart);

            var results = query.List<BasicUserProfileDto>();
            var model = new UserIndexViewModel {
                Users = results.Select(u => new ViewModels.UserEntry() { User = u }).ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);
            pagerShape.RouteData(routeData);

            // https://stackoverflow.com/questions/43565738/name-valuetuple-properties-when-creating-with-new/43566072#43566072
            // can't cast with "is (Url: string, Email: string>) activationEmail"
            if (TempData[activationEmailKey] is ValueTuple<string, string> activationEmail) {
                var (url, email) = activationEmail;
                model.ActivationEmailSent = Shape.ActivationEmailSent(Url: url, Email: email);
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult SendChallengeEmail(int id) {
            if (!service.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users"))) {
                return new HttpUnauthorizedResult();
            }

            var user = service.ContentManager.Get<IUser>(id);
            if (user == null) {
                return HttpNotFound();
            }

            var siteUrl = service.WorkContext.CurrentSite.BaseUrl;
            if (string.IsNullOrWhiteSpace(siteUrl)) {
                siteUrl = HttpContext.Request.ToRootUrlString();
            }
            var delayToValidate = new TimeSpan(7, 0, 0, 0); // one week to validate email
            var nonce = userService.CreateNonce(user, delayToValidate);
            var activationEmailUrl = Url.MakeAbsolute(
                Url.Action("ChallengeEmail", "Account", new { Area = "Orchard.Users", nonce }), siteUrl
            );
            userService.SendChallengeEmail(
                user.As<UserPart>(),
                //ignore nonce created by SenderChallengeEmail service
                _ => activationEmailUrl
            );
            service.Notifier.Success(T("Challenge email sent to {0}", user.Email));
            var activationEmail = (Url: activationEmailUrl, user.Email);
            TempData[activationEmailKey] = activationEmail;
            return RedirectToAction(nameof(Index));
        }

        private static void SetProjectionList(bool hasBasicUserProfilePart, ICriteria query) {
            // set a new projection
            var projection = Projections.ProjectionList()
                .Add(Projections.Property("user.Id").As("Id"))
                .Add(Projections.Property("user.EmailStatus").As("EmailStatus"))
                .Add(Projections.Property("user.UserName").As("Username"))
                .Add(Projections.Property("user.Email").As("Email"))
                .Add(Projections.Property("user.RegistrationStatus").As("RegistrationStatus"))
                .Add(Projections.Property("user.CreatedUtc").As("CreatedUtc"))
                .Add(Projections.Property("user.LastLoginUtc").As("LastLoginUtc"));

            if (hasBasicUserProfilePart) {
                projection
                    .Add(Projections.Property("profile.FirstName").As("FirstName"))
                    .Add(Projections.Property("profile.Lastname").As("LastName"));
            }
            else {
                projection
                    .Add(Projections.Constant(string.Empty), "FirstName")
                    .Add(Projections.Constant(string.Empty), "LastName");
            }

            query.SetProjection(projection);
            query.SetResultTransformer(Transformers.AliasToBean<BasicUserProfileDto>());
        }

        private static void SetSortOrder(UserIndexOptions options, ICriteria query, bool hasBasicUserProfilePart) {
            switch (options.Order) {
                case UsersOrder.Name:
                    if (hasBasicUserProfilePart) {
                        query.AddOrder(Order.Asc("profile.FirstName"));
                    }
                    else {
                        query.AddOrder(Order.Asc("user.UserName"));
                    }
                    break;
                case UsersOrder.Email:
                    query.AddOrder(Order.Asc("user.Email"));
                    break;
                case UsersOrder.CreatedUtc:
                    query.AddOrder(Order.Desc("user.CreatedUtc"));
                    break;
                case UsersOrder.LastLoginUtc:
                    query.AddOrder(Order.Desc("user.LastLoginUtc"));
                    break;
            }
        }

        private static void SetFilter(UserIndexOptions options, ICriteria query, bool hasBasicUserProfilePart) {
            // default options
            options ??= new UserIndexOptions();
            switch (options.Filter) {
                case UsersFilter.Approved:
                    query.Add(Restrictions.Eq("user.RegistrationStatus", UserStatus.Approved));
                    break;
                case UsersFilter.Pending:
                    query.Add(Restrictions.Eq("user.RegistrationStatus", UserStatus.Pending));
                    break;
                case UsersFilter.EmailPending:
                    query.Add(Restrictions.Eq("user.EmailStatus", UserStatus.Pending));
                    break;
            }

            // TODO check if it show duplicated value
            if (!string.IsNullOrWhiteSpace(options.Search)) {
                var disjunction = Restrictions.Disjunction()
                    .Add(Restrictions.Like("user.Email", $"{options.Search}%"))
                    .Add(Restrictions.Like("user.UserName", $"{options.Search}%"));//Field for ICriteria is case sensitive

                if (hasBasicUserProfilePart) {
                    disjunction.Add(Restrictions.Like("profile.FirstName", $"{options.Search}%"));
                }

                query.Add(disjunction);
            }
        }
    }
}
