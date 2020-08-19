using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Codesanook.BasicUserProfile {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() => new[] {
            new RouteDescriptor {
                Name = "BasicUserRegistration",
                Priority = 1, // To override existing route in Orchard.Users
                Route = new Route(
                    url:"users/account/register", // Route cannot start with /
                    defaults: new RouteValueDictionary {
                        { "controller", "Account" },
                        { "action", "Register" }
                    },
                    constraints: new RouteValueDictionary(),
                    dataTokens: new RouteValueDictionary {
                        { "area", "Codesanook.BasicUserProfile" } // namespace
                    },
                    routeHandler: new MvcRouteHandler()
                )
            }
        };
    }
}
