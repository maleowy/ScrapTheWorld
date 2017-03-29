using System;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;

namespace Extensions
{
    public static class CefSharpExtensions
    {
        public static void WaitForInitialization(this IWebBrowser browser)
        {
            while (!browser.IsBrowserInitialized)
            {
                Thread.Sleep(100);
            }
        }

        public static async Task WaitForInitializationAsync(this IWebBrowser browser)
        {
            while (!browser.IsBrowserInitialized)
            {
                await Task.Delay(100);
            }
        }

        public static bool LoadPage(this IWebBrowser browser, string address = null)
        {
            AutoResetEvent waitHandle = new AutoResetEvent(false);

            browser.LoadingStateChanged += (obj, args) =>
            {
                if (address == null || args.Browser.MainFrame.Url.StartsWith(address))
                {
                    waitHandle.Set();
                }
            };

            browser.Load(address);

            return waitHandle.WaitOne();
        }

        public static async Task LoadPageAsync(this IWebBrowser browser, string address = null)
        {
            AsyncAutoResetEvent waitHandle = new AsyncAutoResetEvent();

            browser.LoadingStateChanged += (obj, args) =>
            {
                if (address == null || args.Browser.MainFrame.Url.StartsWith(address))
                {
                    waitHandle.Set();
                }
            };

            browser.Load(address);

            await waitHandle.WaitAsync();
        }

        public static async Task<string> EvaluateScriptWithReturnAsync(this IWebBrowser browser, string script, TimeSpan? timeout = null, string defaultValue = default(string))
        {
            return await EvaluateScriptWithReturnAsync<string>(browser, script, timeout, defaultValue);
        }

        public static async Task<T> EvaluateScriptWithReturnAsync<T>(this IWebBrowser browser, string script, TimeSpan? timeout = null, T defaultValue = default(T))
        {
            while (browser.IsLoading)
            {
                await Task.Delay(100);
            }

            return await browser.EvaluateScriptAsync(script, timeout)
                .ContinueWith(x => !x.IsFaulted && x.Result.Success ? (T)x.Result.Result : defaultValue);
        }
    }
}
