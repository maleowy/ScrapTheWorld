using CefSharp;

namespace Extensions
{
    public class MyRenderProcessMessageHandler : IRenderProcessMessageHandler
    {
        void IRenderProcessMessageHandler.OnContextCreated(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
            string script = "document.addEventListener('DOMContentLoaded', function(){ console.log('DomLoaded'); });";

            frame.ExecuteJavaScriptAsync(script);
        }

        public void OnFocusedNodeChanged(IWebBrowser browserControl, IBrowser browser, IFrame frame, IDomNode node)
        {
            
        }
    }
}