using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Orchard.DisplayManagement.Shapes;

namespace Codesanook.BasicUserProfile.ViewModels {
    public class UserRegistrationViewModel : Shape {

        [DisplayName("First Name")]
        [Required]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [DisplayName("Confirm Password")]
        [Required]
        public string ConfirmPassword { get; set; }

        [DisplayName("Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string MobilePhoneNumber { get; set; }

        [DisplayName("Organization Name")]
        public string OrganizationName { get; set; }

        public int PasswordLength { get; set; }
        public bool LowercaseRequirement { get; set; }
        public bool UppercaseRequirement { get; set; }
        public bool SpecialCharacterRequirement { get; set; }
        public bool NumberRequirement { get; set; }
    }
}