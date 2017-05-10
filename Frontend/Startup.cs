using Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;

namespace Frontend
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app
            .Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration();
                map.RunSignalR(hubConfiguration);
            })
            .UseNancy();
        }
    }  
}
