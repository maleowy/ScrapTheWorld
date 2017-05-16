using Nancy;

namespace Frontend
{
    public class HelloModule : NancyModule
    {
        public HelloModule()
        {
            Get["/"] = o => View["index"];

            Get["/flow"] = o => View["flow"];

            Get["/scripts"] = o =>
            {
                dynamic viewBag = new DynamicDictionary();
                viewBag.Url = "http://localhost:8081/api/Persistence?table=scripts";

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
