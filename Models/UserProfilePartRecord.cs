using Orchard.ContentManagement.Records;

namespace Codesanook.BasicUserProfile.Models {

    public class UserProfilePartRecord : ContentPartRecord {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public virtual string MobilePhoneNumber { get; set; }
        public virtual string OrganizationName { get; set; }
    }
}