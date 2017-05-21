using Nancy;

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
