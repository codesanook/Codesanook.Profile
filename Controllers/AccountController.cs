using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Security;
using Codesanook.Common;
using Codesanook.BasicUserProfile.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Utility.Extensions;
using Codesanook.BasicUserProfile.ViewModels;
using OrchardAccountController = Orchard.Users.Controllers.AccountController;

namespace Codesanook.BasicUserProfile.Controllers {

    [ValidateInput(false), Themed]
    public class AccountController : Controller {
        private readonly IOrchardServices orchardService;
        private readonly ISiteService siteService;
        private readonly IContentManager contentManager;
        private readonly IMembershipService membershipService;
        private readonly IUserService userService;
        private readonly IUserEventHandler userEventHandler;
        private readonly IAuthenticationService authenticationService;
        private readonly dynamic shapeFactory;

        public AccountController(
            IOrchardServices orchardService,
            ISiteService siteService,
            IContentManager contentManager,
            IShapeFactory shapeFactory,
            IMembershipService membershipService,
            IUserService userService,
            IUserEventHandler userEventHandler,
            IAuthenticationService authenticationService
        ) {
            T = NullLocalizer.Instance;
            this.orchardService = orchardService;
            this.siteService = siteService;
            this.contentManager = contentManager;
            this.shapeFactory = shapeFactory;
            this.membershipService = membershipService;
            this.userService = userService;
            this.userEventHandler = userEventHandler;
            this.authenticationService = authenticationService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        protected ILogger Logger { get; set; }
        public Localizer T { get; set; }

        // users/account/register
        public ActionResult Register() {
            // ensure users can register
            var membershipSettings = membershipService.GetSettings();
            if (!membershipSettings.UsersCanRegister) {
                return HttpNotFound();
            }

            // strongly type as property of UserRegistrationViewModel
            var shape = shapeFactory.RegisterUser(typeof(UserRegistrationViewModel));
            SetMembershipSettings(shape, membershipSettings);
            return new ShapeResult(this, shape);
        }

        [HttpPost]
        public ActionResult Register(UserRegistrationViewModel viewModel, string returnUrl = null) {
            var membershipSettings = membershipService.GetSettings();
            // ensure users can register
            if (!membershipSettings.UsersCanRegister) {
                return HttpNotFound();
            }

            if (!ModelState.IsValid) {
                return new ShapeResult(this, CreateRegisterShape(viewModel, membershipSettings));
            }

            var emailWithoutDomain = viewModel.Email.Substring(0, viewModel.Email.LastIndexOf('@'));
            // Generate username from emailWithoutDoman + random 8 characters
            var username = $"{emailWithoutDomain}-{RandomHelper.GenerateRandomCharacters(8)}";

            if (ValidateRegistration(username, viewModel.Email, viewModel.Password, viewModel.ConfirmPassword)) {
                // Attempt to register the user
                // No need to report this to IUserEventHandler because _membershipService does that for us
                var user = membershipService.CreateUser(
                    new CreateUserParams(
                        username,
                        viewModel.Password,
                        viewModel.Email,
                        null,
                        null,
                        false
                    )
                );

                if (user != null) {

                    UpdateUserProfile(user, viewModel);
                    TempData["Email"] = viewModel.Email;

                    if (user.As<UserPart>().EmailStatus == UserStatus.Pending) {
                        var siteUrl = orchardService.WorkContext.CurrentSite.BaseUrl;
                        if (string.IsNullOrWhiteSpace(siteUrl)) {
                            siteUrl = HttpContext.Request.ToRootUrlString();
                        }

                        // To make it available in an email template without changing Orchard.User core project
                        HttpContext.Items["userProfilePart"] = user.As<UserProfilePart>();

                        userService.SendChallengeEmail(
                            user.As<UserPart>(),
                            nonce => Url.MakeAbsolute(
                                Url.Action("ChallengeEmail",
                                "Account",
                                new { Area = "Orchard.Users", nonce }),
                                siteUrl
                            )
                        );

                        userEventHandler.SentChallengeEmail(user);
                        return RedirectToAction(
                            nameof(OrchardAccountController.ChallengeEmailSent),
                            "Account",
                            new { ReturnUrl = returnUrl, Area = "Orchard.Users" }
                        );
                    }

                    if (user.As<UserPart>().RegistrationStatus == UserStatus.Pending) {
                        return RedirectToAction(
                            nameof(OrchardAccountController.RegistrationPending),
                            "Account",
                            new { ReturnUrl = returnUrl, Area = "Orchard.Users" }
                        );
                    }

                    userEventHandler.LoggingIn(username, viewModel.Password);
                    // Force log in user
                    authenticationService.SignIn(user, false /* createPersistentCookie */);
                    userEventHandler.LoggedIn(user);
                    return this.RedirectLocal(returnUrl);
                }

                ModelState.AddModelError(
                    "_FORM",
                    T(ErrorCodeToString(/*createStatus*/MembershipCreateStatus.ProviderError))
                );
            }

            // If we got this far, something failed, redisplay form
            return new ShapeResult(this, CreateRegisterShape(viewModel, membershipSettings));
        }

        private UserRegistrationViewModel CreateRegisterShape(UserRegistrationViewModel viewModel, IMembershipSettings membershipSettings) {
            UserRegistrationViewModel shape = shapeFactory.RegisterUser(typeof(UserRegistrationViewModel));
            SetViewModelValue(viewModel, shape);
            SetMembershipSettings(shape, membershipSettings);
            return shape;
        }

        private static void SetViewModelValue(UserRegistrationViewModel viewModel, UserRegistrationViewModel shape) {
            shape.FirstName = viewModel.FirstName;
            shape.LastName = viewModel.LastName;
            shape.Email = viewModel.Email;
            shape.Password = viewModel.Password;
            shape.MobilePhoneNumber = viewModel.MobilePhoneNumber;
            shape.OrganizationName = viewModel.OrganizationName;
        }

        private static void UpdateUserProfile(IUser user, UserRegistrationViewModel viewModel) {
            var userProfilePart = user.As<UserProfilePart>();
            userProfilePart.FirstName = viewModel.FirstName;
            userProfilePart.LastName = viewModel.LastName;
            userProfilePart.MobilePhoneNumber = viewModel.MobilePhoneNumber;
            userProfilePart.OrganizationName = viewModel.OrganizationName;
        }

        private void SetMembershipSettings(UserRegistrationViewModel viewModel, IMembershipSettings memnerSetting) {
            viewModel.PasswordLength = memnerSetting.GetMinimumPasswordLength();
            viewModel.LowercaseRequirement = memnerSetting.GetPasswordLowercaseRequirement();
            viewModel.UppercaseRequirement = memnerSetting.GetPasswordUppercaseRequirement();
            viewModel.SpecialCharacterRequirement = memnerSetting.GetPasswordSpecialRequirement();
            viewModel.NumberRequirement = memnerSetting.GetPasswordNumberRequirement();
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus) {
            // See http://msdn.microsoft.com/en-us/library/system.web.security.membershipcreatestatus.aspx for
            // a full list of status codes.
            switch (createStatus) {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return
                        "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return
                        "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return
                        "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        private bool ValidateRegistration(string userName, string email, string password, string confirmPassword) {
            bool validate = true;

            if (string.IsNullOrEmpty(userName)) {
                ModelState.AddModelError("username", T("You must specify a username."));
                validate = false;
            }
            else {
                if (userName.Length >= UserPart.MaxUserNameLength) {
                    ModelState.AddModelError("username", T("The username you provided is too long."));
                    validate = false;
                }
            }

            if (string.IsNullOrEmpty(email)) {
                ModelState.AddModelError("email", T("You must specify an email address."));
                validate = false;
            }
            else if (email.Length >= UserPart.MaxEmailLength) {
                ModelState.AddModelError("email", T("The email address you provided is too long."));
                validate = false;
            }
            else if (!Regex.IsMatch(email, UserPart.EmailPattern, RegexOptions.IgnoreCase)) {
                // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                ModelState.AddModelError("email", T("You must specify a valid email address."));
                validate = false;
            }

            if (!validate)
                return false;

            if (!userService.VerifyUserUnicity(userName, email)) {
                ModelState.AddModelError("userExists", T("User with that username and/or email already exists."));
            }

            ValidatePassword(password);

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal)) {
                ModelState.AddModelError("_FORM", T("The new password and confirmation password do not match."));
            }
            return ModelState.IsValid;
        }

        private void ValidatePassword(string password) {
            if (!userService.PasswordMeetsPolicies(password, out IDictionary<string, LocalizedString> validationErrors)) {
                foreach (var error in validationErrors) {
                    ModelState.AddModelError(error.Key, error.Value);
                }
            }
        }
    }
}
