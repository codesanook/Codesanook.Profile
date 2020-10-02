using System;
using Orchard.Users.Models;

namespace Codesanook.Profile.Models {
    public class UserProfileDto {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public UserStatus EmailStatus { get; set; }
        public UserStatus RegistrationStatus { get; set; }
        public DateTime? CreatedUtc { get; set; }
        public DateTime? LastLoginUtc { get; set; }
    }
}