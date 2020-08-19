using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Codesanook.BasicUserProfile.Models;

namespace Codesanook.BasicUserProfile.Handlers {
    public class UserProfileParthHandler : ContentHandler {

        public UserProfileParthHandler(IRepository<UserProfilePartRecord> repository) =>
            Filters.Add(StorageFilter.For(repository));
    }
}