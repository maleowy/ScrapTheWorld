using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.SelfHost;
using Swashbuckle.Application;

namespace Persistence
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Persistence";

            var url = "http://localhost:8081";
            var config = new HttpSelfHostConfiguration(url);

            config.Routes.MapHttpRoute(
                "API Default", "api/{controller}/{id}",
                new { id = RouteParameter.Optional });

            config.EnableSwagger(x =>
            {
                x.SingleApiVersion("v1", "Persistence");
                x.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            })
            .EnableSwaggerUi();

            var cors = new EnableCorsAttribute(
                origins: "*",
                headers: "*",
                methods: "*");

            config.EnableCors(cors);

            using (HttpSelfHostServer server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Console.WriteLine(url + "/api/Persistence");
                Console.WriteLine(url + "/swagger/ui/index#/Persistence");

                Console.ReadLine();
            }
        }
    }
}
