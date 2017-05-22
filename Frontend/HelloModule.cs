using Nancy;
using Nancy.Extensions;
using static Logic.Logic;

namespace Frontend
{
    public class HelloModule : NancyModule
    {
        public HelloModule()
        {
            Get["/"] = o => View["index"];

            Get["/flow"] = o =>
            {
                dynamic viewBag = new DynamicDictionary();
                viewBag.PersistencePort = Program.PersistencePort;

                return View["flow", viewBag];
            };

            Post["/validate"] = o =>
            {
                var json = Context.Request.Body.AsString();
                GetNodes(json);

                return HttpStatusCode.OK;
            };

            Get["/scripts"] = o =>
            {
                dynamic viewBag = new DynamicDictionary();
                viewBag.PersistencePort = Program.PersistencePort;
                viewBag.Table = "scripts";

                return View["scripts", viewBag];
            };

            Get["/test"] = o =>
            {
                HelloHub.ReturnResults(null, "test", "info_outline");
                return HttpStatusCode.OK;
            };
        }
    }
}
