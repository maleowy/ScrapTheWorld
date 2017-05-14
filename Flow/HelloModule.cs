using Nancy;

namespace Flow
{
    public class HelloModule : NancyModule
    {
        public HelloModule()
        {
            Get["/"] = o => View["flow"];
        }
    }
}
