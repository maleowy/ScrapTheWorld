using Nancy;
using Nancy.Extensions;
using static Logic.Logic;

namespace Flow
{
    public class HelloModule : NancyModule
    {
        public HelloModule()
        {
            Get["/"] = o =>
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
        }
    }
}
