// How to integrate React server side rendering
// Install React.Core install-package React.Core -version 5.0.0
// Add <add namespace="Codesanook.ReactJS" /> to <system.web.webPages.razor> in Web.config root level
// Add Script.Require("React").AtHead(), ReactDOM and our script in a module Razor page
// Add @Html.ReactInitJavaScript() at bottom of Document.cshtml of an active theme to initialize React script
using Orchard.Environment;
using React;

namespace Codesanook.Users {
    public class ShellEvent : IOrchardShellEvents {
        public void Activated() {
            ReactSiteConfiguration.Configuration
                // Disable load Babel because we already transformed TypeScript with Webpack 
                .SetLoadBabel(false)
                .AddScriptWithoutTransform(
                    "~/Modules/Codesanook.Users/Scripts/codesanook-users.js"
                );
        }

        public void Terminating() {
        }
    }
}
