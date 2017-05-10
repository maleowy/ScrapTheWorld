using Nancy;

namespace Frontend
{
    public class HelloModule : NancyModule
    {
        public HelloModule()
        {
            Get["/"] = o => View["index"];

            Get["/test"] = o =>
            {
                HelloHub.ReturnResults(null, "test", "info_outline");
                return HttpStatusCode.OK;
            };
        }
    }
}
