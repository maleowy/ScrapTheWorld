using Nancy;

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
        }
    }
}
