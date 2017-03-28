// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.
using System;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;

namespace Worker.CefSharp.OffScreen
{
    public class Program
    {
        public static ChromiumWebBrowser Browser;

        public static void Main(string[] args)
        {
            Console.Title = "Worker CefSharp OffScreen";

            var settings = new CefSettings();

            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, true, null);

            // Create the offscreen Chromium Browser.
            Browser = new ChromiumWebBrowser("about:blank");

            Browser.ConsoleMessage += (sender, eventArgs) =>
            {
                Console.WriteLine(eventArgs.Message);
            };

            Run().Wait();

            Console.ReadLine();

            // Clean up Chromium objects.  You need to call this in your application otherwise
            // you will get a crash when closing.
            Cef.Shutdown();
        }

        public static async Task Run()
        {
            await Task.Delay(3000);
            Browser.Load("https://duckduckgo.com");
            await Task.Delay(3000);
            await Browser.EvaluateScriptAsync("document.querySelector('#search_form_input_homepage').value = 'Test';");
            await Task.Delay(1000);
            await Browser.EvaluateScriptAsync("document.querySelector('#search_button_homepage').click();");
            await Task.Delay(1000);
            await Browser.EvaluateScriptAsync("console.log(document.title)");
        }
    }
}
