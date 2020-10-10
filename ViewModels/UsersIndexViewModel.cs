using System.Collections.Generic;
using Codesanook.Users.Models;
using Orchard.Users.ViewModels;

namespace Codesanook.Users.ViewModels {

    public class UsersIndexViewModel {
        public IList<UserEntry> Users { get; set; }
        public UserIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public dynamic ActivationEmailSent { get; set; }
    }

    public class UserEntry {
        public BasicUserProfileDto User { get; set; }
        public bool IsChecked { get; set; }
    }

    public class UserIndexOptions {
        public string Search { get; set; }
        public UsersOrder Order { get; set; }
        public UsersFilter Filter { get; set; }
        public UsersBulkAction BulkAction { get; set; }
    }
}
