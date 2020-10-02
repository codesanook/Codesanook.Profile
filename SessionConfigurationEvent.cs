using Orchard.Data;
using Orchard.Utility;
using NHibernate.Cfg;
using FluentNHibernate.Cfg;
using FluentNHibernate.Automapping;
using Codesanook.BasicUserProfile.Models;

namespace Codesanook.BasicUserProfile
{
    public class SessionConfigurationEvents : ISessionConfigurationEvents
    {
        /// Called when raw NHibernate configuration is being built, after applying all customizations.
        /// Allows applying final alterations to the raw NH configuration.
        public void Building(Configuration cfg)
        {
        }

        /// Called when configuration hash is being computed. If hash changes, configuration will be rebuilt and stored in mappings.bin.
        /// This method allows to alter the default hash to take into account custom configuration changes.
        /// It's a developer responsibility to make sure hash is correctly updated when config needs to be rebuilt.
        /// Otherwise the cached configuration (mappings.bin file) will be used as long as default Orchard configuration 
        /// is unchanged or until the file is manually removed.
        ///Current hash object
        public void ComputingHash(Hash hash)
        {
        }

        /// Called when an empty fluent configuration object has been created, 
        /// before applying any default Orchard config settings (alterations, conventions etc.).
        ///Empty fluent NH configuration object.
        ///Default persistence model that is about to be used.
        //https://github.com/FluentNHibernate/fluent-nhibernate/wiki/Fluent-mapping
        //https://github.com/FluentNHibernate/fluent-nhibernate/wiki/Auto-mapping
        //https://tpodolak.com/blog/2013/03/25/fluent-nhibernate-automappings/
        //http://www.ideliverable.com/blog/isessionconfigurationevents
        public void Created(FluentConfiguration cfg, AutoPersistenceModel defaultModel)
        {
            //https://github.com/FluentNHibernate/fluent-nhibernate/wiki/Auto-mapping
            //https://daveden.wordpress.com/2012/04/05/how-to-use-fluent-nhibernate-with-auto-mappings/
            //https://stackoverflow.com/questions/42100286/nhibernate-fluent-add-external-assembly-mappings
            //https://stackoverflow.com/questions/1858245/fluent-nhibernate-how-to-tell-it-not-to-map-a-base-class
        }

        /// Called when fluent configuration has been prepared but not yet built. 
        public void Prepared(FluentConfiguration cfg) =>
            cfg.Mappings(m => m.HbmMappings.AddFromAssemblyOf<UserProfilePartRecord>());

        /// Called when NHibernate configuration has been built or read from cache storage (mappings.bin file by default).
        ///Final, raw NH configuration object.
        public void Finished(Configuration cfg)
        {
        }
    }
}