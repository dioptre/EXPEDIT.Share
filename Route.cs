using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace EXPEDIT.Share
{
    public class Routes : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "share/{controller}/{action}/{id}",
                        new RouteValueDictionary {
                            {"area", "EXPEDIT.Share"},
                            {"controller", "User"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary {
                            {"area", "EXPEDIT.Share"},
                            {"controller", "User"}
                        },
                        new RouteValueDictionary {
                            {"area", "EXPEDIT.Share"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "share/{action}/{id}/{name}/{contactid}",
                        new RouteValueDictionary {
                            {"area", "EXPEDIT.Share"},
                            {"controller", "User"}                            
                        },
                        new RouteValueDictionary {
                            {"area", "EXPEDIT.Share"},
                            {"controller", "User"},                          
                        },
                        new RouteValueDictionary {
                            {"area", "EXPEDIT.Share"},
                            {"controller", "User"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "share/{action}/{id}/{name}",
                        new RouteValueDictionary {
                            {"area", "EXPEDIT.Share"},
                            {"controller", "User"}                            
                        },
                        new RouteValueDictionary {
                            {"area", "EXPEDIT.Share"},
                            {"controller", "User"},                          
                        },
                        new RouteValueDictionary {
                            {"area", "EXPEDIT.Share"},
                            {"controller", "User"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "share/{action}/{id}",
                        new RouteValueDictionary {
                            {"area", "EXPEDIT.Share"},
                            {"controller", "User"}                            
                        },
                        new RouteValueDictionary {
                            {"area", "EXPEDIT.Share"},
                            {"controller", "User"},                          
                        },
                        new RouteValueDictionary {
                            {"area", "EXPEDIT.Share"},
                            {"controller", "User"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                        Priority = 5,
                        Route = new Route(
                            "share/search",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "search"}
                            },
                            null,
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"}
                            },
                            new MvcRouteHandler())
                },
                new RouteDescriptor {
                        Priority = 5,
                        Route = new Route(
                            "share/checkin",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "checkin"}
                            },
                            null,
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"}
                            },
                            new MvcRouteHandler())
                },
                new RouteDescriptor {
                        Priority = 5,
                        Route = new Route(
                            "share/uploadfile",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "uploadfile"}
                            },
                            null,
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"}
                            },
                            new MvcRouteHandler())
                },
                new RouteDescriptor {
                        Priority = 5,
                        Route = new Route(
                            "share/pickfile",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "pickfile"}
                            },
                            null,
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"}
                            },
                            new MvcRouteHandler())
                },
                new RouteDescriptor {
                        Priority = 5,
                        Route = new Route(
                            "share/myfiles",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "myfiles"}
                            },
                            null,
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"}
                            },
                            new MvcRouteHandler())
                },
                new RouteDescriptor {
                        Priority = 50,
                        Route = new Route(
                            "share/myfiles/{*q}",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "myfiles"}
                            },
                            null,
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"}
                            },
                            new MvcRouteHandler())
                },
                new RouteDescriptor {
                        Priority = 5,
                        Route = new Route(
                            "share/picklocation",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "picklocation"}
                            },
                            null,
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"}
                            },
                            new MvcRouteHandler())
                },
                new RouteDescriptor {
                        Priority = 5,
                        Route = new Route(
                            "share/mylocations",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "mylocations"}
                            },
                            null,
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"}
                            },
                            new MvcRouteHandler())
                },
                new RouteDescriptor {
                        Priority = 50,
                        Route = new Route(
                            "share/mylocations/{*q}",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "mylocations"}
                            },
                            null,
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"}
                            },
                            new MvcRouteHandler())
                },
                new RouteDescriptor {
                        Priority = 50,
                        Route = new Route(
                            "share/loggedin",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "loggedin"}
                            },
                            null,
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"}
                            },
                            new MvcRouteHandler())
                },
                new RouteDescriptor {
                        Priority = 5,
                        Route = new Route(
                            "share/Login",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "Login"}
                            },
                            null,
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"}
                            },
                            new MvcRouteHandler())
                },
                new RouteDescriptor {
                        Priority = 5,
                        Route = new Route(
                            "share/Logout",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "Logout"}
                            },
                            null,
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"}
                            },
                            new MvcRouteHandler())
                },
                new RouteDescriptor {
                        Priority = 5,
                        Route = new Route(
                            "share/IsOnline",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "IsOnline"}
                            },
                            null,
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"}
                            },
                            new MvcRouteHandler())
                }

            };
        }
    }
}