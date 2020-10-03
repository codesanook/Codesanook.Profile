using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Codesanook.BasicUserProfile {
    public class Routes : IRouteProvider {
        private const string areaName = "Codesanook.BasicUserProfile";
        private const string areaOrchardUsers = "Orchard.Users";
        
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
                Priority = 1, // 
                Route = new Route(
                    url:"admin/users", // Route cannot start with /
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
            },
             new RouteDescriptor {
                Name = "BasicUserAdminCreate",
                Priority = 1, // To override existing route in Orchard.Users
                Route = new Route(
                    url:"admin/users/create", // Route cannot start with /
                    defaults: new RouteValueDictionary {
                        { "controller", "Admin" },
                        { "action", "create" }
                    },
                    constraints: new RouteValueDictionary(),
                    dataTokens: new RouteValueDictionary {
                        { "area", areaOrchardUsers }, // namespace
                    },
                    routeHandler: new MvcRouteHandler()
                )
            },
             new RouteDescriptor {
                Name = "BasicUserAdminEdit",
                Priority = 1, // To override existing route in Orchard.Users
                Route = new Route(
                    url:"admin/users/edit/{id}", // Route cannot start with /
                    defaults: new RouteValueDictionary {
                        { "controller", "Admin" }, 
                        { "action", "edit" },
                        { "id", UrlParameter.Optional },
                    },
                    constraints: new RouteValueDictionary(),
                    dataTokens: new RouteValueDictionary {
                        { "area", areaOrchardUsers } // namespace
                    },
                    routeHandler: new MvcRouteHandler()
                )
            },
             new RouteDescriptor {
                Name = "BasicUserAdminDelete",
                Priority = 1, // To override existing route in Orchard.Users
                Route = new Route(
                    url:"admin/users/delete/{id}", // Route cannot start with /
                    defaults: new RouteValueDictionary {
                        { "controller", "Admin" },
                        { "action", "delete" },
                        { "id", UrlParameter.Optional },
                    },
                    constraints: new RouteValueDictionary(),
                    dataTokens: new RouteValueDictionary {
                        { "area", areaOrchardUsers} // namespace
                    },
                    routeHandler: new MvcRouteHandler()
                )
            },
             new RouteDescriptor {
                Name = "BasicUserAdminApprove",
                Priority = 1, // To override existing route in Orchard.Users
                Route = new Route(
                    url:"admin/users/approve/{id}", // Route cannot start with /
                    defaults: new RouteValueDictionary {
                        { "area", areaOrchardUsers}, // namespace
                        { "controller", "Admin" },
                        { "action", "approve" },
                        { "id", UrlParameter.Optional },
                    },
                    constraints: new RouteValueDictionary(),
                    dataTokens: new RouteValueDictionary {
                        { "area", areaOrchardUsers} // namespace
                    },
                    routeHandler: new MvcRouteHandler()
                )
            },
             new RouteDescriptor {
                Name = "BasicUserAdminModerate",
                Priority = 1, // To override existing route in Orchard.Users
                Route = new Route(
                    url:"admin/users/moderate/{id}", // Route cannot start with /
                    defaults: new RouteValueDictionary {
                        { "controller", "Admin" },
                        { "action", "moderate" },
                        { "id", UrlParameter.Optional },
                    },
                    constraints: new RouteValueDictionary(),
                    dataTokens: new RouteValueDictionary {
                        { "area", areaOrchardUsers} // namespace
                    },
                    routeHandler: new MvcRouteHandler()
                )
            },
             new RouteDescriptor {
                Name = "BasicUserAdminSendChallengeEmail",
                Priority = 1, // To override existing route in Orchard.Users
                Route = new Route(
                    url:"admin/users/sendChallengeEmail/{id}", // Route cannot start with /
                    defaults: new RouteValueDictionary {
                        { "controller", "Admin" },
                        { "action", "sendChallengeEmail" },
                        { "id", UrlParameter.Optional },
                    },
                    constraints: new RouteValueDictionary(),
                    dataTokens: new RouteValueDictionary {
                        { "area", areaOrchardUsers} // namespace
                    },
                    routeHandler: new MvcRouteHandler()
                )
            }



        };
    }
}
