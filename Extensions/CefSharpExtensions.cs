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
            try
            {
                AutoResetEvent waitHandle = new AutoResetEvent(false);

                browser.LoadingStateChanged += (obj, args) =>
                {
                    if (address == null || IsSameDomain(args.Browser.MainFrame.Url, address))
                    {
                        waitHandle.Set();
                    }
                };

                browser.Load(address);

                return waitHandle.WaitOne();
            }
            catch (Exception ex)
            {
                throw new Exception($"LoadPage error {address}", ex);
            }
        }

        public static async Task LoadPageAsync(this IWebBrowser browser, string address = null)
        {
            try
            {
                AsyncAutoResetEvent waitHandle = new AsyncAutoResetEvent();

                browser.LoadingStateChanged += (obj, args) =>
                {
                    if (address == null || IsSameDomain(args.Browser.MainFrame.Url, address))
                    {
                        waitHandle.Set();
                    }
                };

                browser.Load(address);

                await waitHandle.WaitAsync(30);
            }
            catch (Exception ex)
            {
                throw new Exception($"LoadPageAsync error {address}",  ex);
            }
        }

        private static bool IsSameDomain(string url1, string url2)
        {
            if (string.IsNullOrEmpty(url1))
                return false;

            var domain1 = new Uri(url1).Host.Replace("www.", "");
            var domain2 = new Uri(url2).Host.Replace("www.", "");

            return domain1.Equals(domain2, StringComparison.Ordinal);
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
