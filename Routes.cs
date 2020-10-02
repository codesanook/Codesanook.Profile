using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Codesanook.BasicUserProfile {
    public class Routes : IRouteProvider {
        private const string areaName = "Codesanook.BasicUserProfile";
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
                        { "area", areaName } // namespace
                    },
                    routeHandler: new MvcRouteHandler()
                )
            },
            new RouteDescriptor {
                Name = "BasicUserAdmin",
                Priority = 1, // To override existing route in Orchard.Users
                Route = new Route(
                    url:"admin/users/index", // Route cannot start with /
                    defaults: new RouteValueDictionary {
                        { "controller", "Admin" },
                        { "action", "index" }
                    },
                    constraints: new RouteValueDictionary(),
                    dataTokens: new RouteValueDictionary {
                        { "area", areaName } // namespace
                    },
                    routeHandler: new MvcRouteHandler()
                )
            }
        };
    }
}
