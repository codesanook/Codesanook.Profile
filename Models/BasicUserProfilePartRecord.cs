using Orchard.ContentManagement.Records;

namespace Codesanook.Users.Models {

    public class BasicUserProfilePartRecord : ContentPartRecord {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public virtual string MobilePhoneNumber { get; set; }
        public virtual string OrganizationName { get; set; }
    }
}