using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Models;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using static Logic.Logic;
using static Helpers.Helpers;

namespace Worker.RemoteDebugging
{
    class Program
    {
        public static IBus Bus;
        public static ISubscriptionResult SubscriptionResult;
        public static Logger Logger;

        static void Main(string[] args)
        {
            Console.Title = "Worker Remote Debugging";

            var dir = AppDomain.CurrentDomain.BaseDirectory;
            dir = Path.GetFullPath(Path.Combine(dir, @"..\..\..\"));

            Process.Start(Path.Combine(dir, @"GoogleChromePortable\GoogleChromePortable.exe"), "--remote-debugging-port=9222 about:blank");

            while (!UrlExists("http://localhost:9222"))
            {
                Thread.Sleep(1000);
            }

            Bus = RabbitHutch.CreateBus(GetBusConfiguration());

            Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole(LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
                .WriteTo.RollingFile("log.txt", retainedFileCountLimit: 7)
                .CreateLogger();

            var urls = MyClientWebSocket.GetUrls();
            var firstUrl = urls.First();
            Console.Title += " - " + firstUrl;

            SubscriptionResult = Bus.SubscribeAsync("subscriptionId", GetLogic(node => Logger.Information("{@Node}", node),
                url => 
                {
                    using (MyClientWebSocket ws = new MyClientWebSocket(firstUrl))
                    {
                        ws.Open(url, CancellationToken.None);
                        ws.WaitForDocumentReady();

                        return Task.FromResult(true);
                    }
                },
                script =>
                {
                    using (MyClientWebSocket ws = new MyClientWebSocket(firstUrl))
                    {
                        string value = ws.EvaluateWithReturn(script, CancellationToken.None);
                        ws.WaitForDocumentReady();

                        return Task.FromResult(value);
                    }
                },
                async node => await Bus.PublishAsync(node),
                async node => await Bus.PublishAsync(new Result { Node = node }),
                async node =>
                {
                    Logger.Error("{@Node}", node);
                    await Bus.PublishAsync(new ErrorResult { Node = node });
                },
                node => { }));

            //Test(firstUrl);

            Console.ReadLine();

            SubscriptionResult.Dispose();

            var chromePoratable = Process.GetProcessesByName("GoogleChromePortable");
            chromePoratable.ToList().ForEach(x => IgnoreExceptions(() => x.Kill()));

            Environment.Exit(0);
        }

        private static void Test(string firstUrl)
        {
            using (MyClientWebSocket ws = new MyClientWebSocket(firstUrl))
            {
                ws.Open("http://www.duckduckgo.com", CancellationToken.None);
                ws.WaitForDocumentReady();
                ws.FillAsync("#search_form_input_homepage", "Test", CancellationToken.None).Wait();
                ws.ClickAsync("#search_button_homepage", CancellationToken.None).Wait();
                ws.WaitForDocumentReady();
            }

            using (MyClientWebSocket ws = new MyClientWebSocket(firstUrl))
            {
                var title = ws.EvaluateWithReturn("document.title", CancellationToken.None);
                Console.WriteLine(title);
            }
        }
    }
}
