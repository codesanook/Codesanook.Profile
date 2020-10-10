using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Codesanook.Users.Models;

namespace Codesanook.Users.Handlers {
    public class UserProfileParthHandler : ContentHandler {

        public UserProfileParthHandler(IRepository<BasicUserProfilePartRecord> repository) =>
            Filters.Add(StorageFilter.For(repository));
    }
}