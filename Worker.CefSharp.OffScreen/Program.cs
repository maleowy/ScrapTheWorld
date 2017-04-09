// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.
using System;
using CefSharp;
using CefSharp.OffScreen;
using EasyNetQ;
using Extensions;
using Models;
using static Logic.Logic;

namespace Worker.CefSharp.OffScreen
{
    public class Program
    {
        public static IBus Bus;
        public static ChromiumWebBrowser Browser;

        public static void Main(string[] args)
        {
            Console.Title = "Worker CefSharp OffScreen";

            Bus = RabbitHutch.CreateBus(GetBusConfiguration());
            InitializeChromium();

            Bus.SubscribeAsync("subscriptionId", GetLogic(async url => await Browser.LoadPageAsync(url),
                async script => await Browser.EvaluateScriptWithReturnAsync(script)));

            Publish();

            Console.ReadLine();

            // Clean up Chromium objects.  You need to call this in your application otherwise
            // you will get a crash when closing.
            Cef.Shutdown();
        }

        private static void Publish()
        {
            Bus.Publish(new Node { Url = "http://www.wp.pl", Script = "document.title" });
            Bus.Publish(new Node { Url = "http://www.onet.pl", Script = "document.title" });
            Bus.Publish(new Node { Url = "http://www.interia.pl", Script = "document.title" });
        }

        private static void InitializeChromium()
        {
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

            Browser.WaitForInitialization();
        }
    }
}
