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
using Codesanook.Profile.Models;
using Orchard.ContentManagement.Records;
using NHibernate.SqlCommand;
using Orchard.Users.Models;
using UserIndexOptions = Codesanook.BasicUserProfile.ViewModels.UserIndexOptions;
using UserOrder = Codesanook.BasicUserProfile.ViewModels.UsersOrder;

namespace Codesanook.BasicUserProfile.Controllers {

    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IMembershipService membershipService;
        private readonly IUserService userService;
        private readonly IUserEventHandler userEventHandlers;
        private readonly ISiteService siteService;
        private readonly ITransactionManager transactionManager;
        private readonly IOrchardServices service;
        private dynamic Shape { get; set; }

        public Localizer T { get; set; }

        public AdminController(
            IOrchardServices service,
            IMembershipService membershipService,
            IUserService userService,
            IShapeFactory shapeFactory,
            IUserEventHandler userEventHandlers,
            ISiteService siteService,
            ITransactionManager transactionManager
        ) {
            this.service = service;
            this.membershipService = membershipService;
            this.userService = userService;
            this.userEventHandlers = userEventHandlers;
            this.siteService = siteService;
            this.transactionManager = transactionManager;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        public ActionResult Index(UserIndexOptions options, PagerParameters pagerParameters) {
            if (!service.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to list users"))) {
                return new HttpUnauthorizedResult();
            }

            // Create criteria
            var session = transactionManager.GetSession();
            var query = session.CreateCriteria<ContentItemVersionRecord>("version")
                .CreateAlias("version.ContentItemRecord", "item", JoinType.InnerJoin)
                // content item is aggregation of part records
                .CreateAlias("item.ContentType", "type", JoinType.InnerJoin)
                .CreateAlias("item.UserPartRecord", "user", JoinType.InnerJoin)
                .CreateAlias("item.UserProfilePartRecord", "profile", JoinType.LeftOuterJoin)// Not all user have profiles, e.g admin
                .Add(Restrictions.Eq("type.Name", "User"))
                .Add(Restrictions.Eq("version.Published", true));

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

            if (!string.IsNullOrWhiteSpace(options.Search)) {
                query.Add(Restrictions.Or(
                    Restrictions.Like("profile.FirstName", $"{options.Search}%"),
                    Restrictions.Like("user.Email", $"{options.Search}%")
                ));
            }
            var totalItemCount = query.SetProjection(Projections.RowCount()).UniqueResult<int>();

            var pager = new Pager(siteService.GetSiteSettings(), pagerParameters);
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalItemCount);

            // set a new projection
            query.SetProjection(Projections.ProjectionList()
                .Add(Projections.Property("user.Id").As("Id"))
                .Add(Projections.Property("user.EmailStatus").As("EmailStatus"))
                .Add(Projections.Property("user.UserName").As("Username"))
                .Add(Projections.Property("profile.FirstName").As("FirstName"))
                .Add(Projections.Property("profile.LastName").As("LastName"))
                .Add(Projections.Property("user.Email").As("Email"))
                .Add(Projections.Property("user.RegistrationStatus").As("RegistrationStatus"))
                .Add(Projections.Property("user.CreatedUtc").As("CreatedUtc"))
                .Add(Projections.Property("user.LastLoginUtc").As("LastLoginUtc"))
            )
            .SetResultTransformer(Transformers.AliasToBean<UserProfileDto>())
            // FirstResult is row index start from 0 
            // pager.GetStartIndex() is calculated row index that we use
            // If we want page number, use pager.Page.
            .SetFirstResult(pager.GetStartIndex());

            switch (options.Order) {
                case UserOrder.FirstName:
                    query.AddOrder(Order.Asc("profile.FirstName"));
                    break;
                case UserOrder.Email:
                    query.AddOrder(Order.Asc("user.Email"));
                    break;
                case UserOrder.CreatedUtc:
                    query.AddOrder(Order.Asc("user.CreatedUtc"));
                    break;
                case UserOrder.LastLoginUtc:
                    query.AddOrder(Order.Asc("user.LastLoginUtc"));
                    break;
            }

            var results = query.List<UserProfileDto>();
            var model = new ViewModels.UsersIndexViewModel {
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

            return View(model);
        }
    }
}
