using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using Codesanook.Users.Models;

namespace Codesanook.Users.Drivers {
    public class UserProfilePartDriver : ContentPartDriver<BasicUserProfilePart> {

        protected override string Prefix => nameof(BasicUserProfilePart);

        public UserProfilePartDriver() {
        }

        protected override DriverResult Editor(BasicUserProfilePart part, dynamic shapeHelper) {

            return ContentShape(
                "Parts_UserProfile_Edit", // shapeType used in Placement.info
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/UserProfile", //inside EditorTemplates folder
                    Model: part,
                    Prefix: Prefix
                )
            );
        }

        protected override DriverResult Editor(BasicUserProfilePart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}