// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.
using System;
using CefSharp;
using CefSharp.OffScreen;
using Extensions;
using Logic;

namespace Worker.CefSharp.OffScreen
{
    public class Program
    {
        public static ChromiumWebBrowser Browser;

        public static void Main(string[] args)
        {
            Console.Title = "Worker CefSharp OffScreen";

            var settings = new CefSettings();
            settings.IgnoreCertificateErrors = true;

            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, true, null);

            // Create the offscreen Chromium Browser.
            Browser = new ChromiumWebBrowser("about:blank");

            var boundObj = new BoundObject();
            Browser.RegisterJsObject("bound", boundObj);

            Browser.ConsoleMessage += (sender, eventArgs) =>
            {
                Console.WriteLine(eventArgs.Message);
            };

            TestLogic.Run(Browser).Wait();

            Console.ReadLine();

            // Clean up Chromium objects.  You need to call this in your application otherwise
            // you will get a crash when closing.
            Cef.Shutdown();
        }
    }
}
