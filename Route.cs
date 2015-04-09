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
                            "share/uploadbase64png",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "uploadbase64png"}
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
                },
                new RouteDescriptor {
                        Priority = 5,
                        Route = new Route(
                            "share/GetUsernames",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "GetUsernames"}
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
                            "share/GetCompanies",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "GetCompanies"}
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
                            "share/GetContacts",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "GetContacts"}
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
                            "share/uploadphoto",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "uploadphoto"}
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
                            "share/forms",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "forms"}
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
                            "share/pickworkflow",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "pickworkflow"}
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
                            "share/captcha",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "captcha"}
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
                            "share/signup",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "signup"}
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
                            "share/duplicateuser",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "duplicateuser"}
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
                            "share/requestpassword",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "requestpassword"}
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
                            "share/GetMyCompanies",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "GetMyCompanies"}
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
                            "share/DuplicateCompany",
                            new RouteValueDictionary {
                                {"area", "EXPEDIT.Share"},
                                {"controller", "User"},
                                {"action", "DuplicateCompany"}
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