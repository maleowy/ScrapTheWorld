// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.
using System;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;
using EasyNetQ;
using Extensions;
using Logic;
using Models;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using static Logic.Logic;

namespace Worker.CefSharp.OffScreen
{
    public class Program
    {
        public static IBus Bus;
        public static ISubscriptionResult SubscriptionResult;
        public static Logger Logger;
        public static ChromiumWebBrowser Browser;

        public static void Main(string[] args)
        {
            Console.Title = "Worker CefSharp OffScreen";

            Bus = RabbitHutch.CreateBus(GetBusConfiguration());

            Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .WriteTo.RollingFile("log.txt", retainedFileCountLimit: 7)
                .CreateLogger();

            InitializeChromium();

            SubscriptionResult = Bus.SubscribeAsync("subscriptionId", GetLogic(node => Logger.Information("{@Node}", node),
                url => Task.FromResult(Browser.LoadPage(url)),
                script => Task.FromResult(Browser.EvaluateScriptWithReturn(script)),
                async node => await Bus.PublishAsync(node),
                async node => await Bus.PublishAsync(new Result { Node = node }),
                async node =>
                {
                    Logger.Error("{@Node}", node);
                    await Bus.PublishAsync(new ErrorResult { Node = node });
                },
                node => {}));

            Console.ReadLine();

            SubscriptionResult.Dispose();

            // Clean up Chromium objects.  You need to call this in your application otherwise
            // you will get a crash when closing.
            Cef.Shutdown();

            Environment.Exit(0);
        }

        private static async void Test()
        {
            var title = TestLogic.InvalidUrl(Browser).Result;

            await TestLogic.Run(Browser);

            Bus.Publish(new Node {Name = "flow1", Data = new { Guid = Guid.NewGuid() } });

            Bus.Publish(Factory.GetTitleNode("http://www.wp.pl"));
            Bus.Publish(Factory.GetTitleNode("http://www.onet.pl"));
            Bus.Publish(Factory.GetTitleNode("http://www.interia.pl"));

            Bus.Subscribe<Result>("subscriptionId", x => Logger.Information(JsonConvert.SerializeObject(x.Node.Results)));
        }

        private static void InitializeChromium()
        {
            var settings = new CefSettings();
            settings.IgnoreCertificateErrors = true;

            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, true, null);

            // Create the offscreen Chromium Browser.
            Browser = new ChromiumWebBrowser("about:blank")
            {
                LifeSpanHandler = new MyLifeSpanHandler(),
                RequestHandler = new MyRequestHandler(),
                RenderProcessMessageHandler = new MyRenderProcessMessageHandler(),
                BrowserSettings = { ImageLoading = CefState.Disabled }
            };

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
