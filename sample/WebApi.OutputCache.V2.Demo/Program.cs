﻿using System;
using System.Web.Http;
using System.Web.Http.SelfHost;
using WebApi.OutputCache.Core.Cache;

namespace WebApi.OutputCache.V2.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new HttpSelfHostConfiguration("http://localhost:9999");
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                  name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            var server = new HttpSelfHostServer(config);

            config.CacheOutputConfiguration().RegisterCacheOutputProvider(() => new CouchbaseCache());

            server.OpenAsync().Wait();

            Console.ReadKey();

            server.CloseAsync().Wait();
        }
    }
}
