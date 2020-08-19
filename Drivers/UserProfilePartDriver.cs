using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using Codesanook.BasicUserProfile.Models;

namespace Codesanook.BasicUserProfile.Drivers {
    public class UserProfilePartDriver : ContentPartDriver<UserProfilePart> {

        protected override string Prefix => nameof(UserProfilePart);

        public UserProfilePartDriver() {
        }

        protected override DriverResult Editor(UserProfilePart part, dynamic shapeHelper) {

            return ContentShape(
                "Parts_UserProfile_Edit", // shapeType used in Placement.info
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/UserProfile", //inside EditorTemplates folder
                    Model: part,
                    Prefix: Prefix
                )
            );
        }

        protected override DriverResult Editor(UserProfilePart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}