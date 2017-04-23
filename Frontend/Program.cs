using System;
using System.Collections.Generic;
using System.Diagnostics;
using EasyNetQ;
using Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Logic;
using Serilog;
using Serilog.Core;
using static Logic.Logic;

namespace Frontend
{
    class Program
    {
        public static IBus Bus;
        public static Logger Logger;

        public static string Dir;
        public static string WorkerDir;
        public static string WorkerExe;

        static void Main(string[] args)
        {
            Console.Title = "Frontend";

            PreparePaths();
            AddRabbit();

            Bus = RabbitHutch.CreateBus("host=localhost");

            Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            var resultSubscription = Bus.SubscribeAsync<Result>("subscriptionId", x =>
            {
                Logger.Information(x.Data);
                return Task.FromResult(true);
            });

            var errorSubscription = Bus.SubscribeAsync<ErrorResult>("subscriptionId", x =>
            {
                Logger.Error("{@Node}", x.Node);
                return Task.FromResult(true);
            });

            Console.WriteLine("o - offscreen, f - winforms, w - wpf, p - publish, 1 - flow, 2 - flow, esc - exit");

            var workers = new List<Process>();

            while (true)
            {
                ConsoleKey key = Console.ReadKey().Key;

                switch (key)
                {
                    case ConsoleKey.O:
                        workers.Add(StartWorker("CefSharp.OffScreen"));
                        break;
                    case ConsoleKey.F:
                        workers.Add(StartWorker("CefSharp.WinForms"));
                        break;
                    case ConsoleKey.W:
                        workers.Add(StartWorker("CefSharp.WPF"));
                        break;
                    case ConsoleKey.P:
                        Publish();
                        break;
                    case ConsoleKey.D1:
                        Flow1();
                        break;
                    case ConsoleKey.D2:
                        Flow2();
                        break;
                    case ConsoleKey.Escape:

                        resultSubscription.Dispose();
                        errorSubscription.Dispose();

                        workers.ForEach(p =>
                        {
                            IgnoreExceptions(() => p.Kill());
                        });
                        //erl.ToList().ForEach(e => e.Kill());
                        Environment.Exit(0);
                        break;
                }

                Console.WriteLine();
            }
        }

        private static void PreparePaths()
        {
            Dir = AppDomain.CurrentDomain.BaseDirectory;
            Dir = Path.GetFullPath(Path.Combine(Dir, @"..\..\..\..\"));

            WorkerDir = Path.Combine(Dir, @"Worker.{0}\bin\x86\Debug");
            WorkerExe = Path.Combine(WorkerDir, @"Worker.{0}.exe");
        }

        private static void AddRabbit()
        {
            var erl = Process.GetProcessesByName("erl");

            if (erl.Length == 0 && Directory.Exists(Path.Combine(Dir, @"RabbitMQ")))
            {
                if (!Directory.Exists(Path.Combine(Dir, @"RabbitMQ\rabbitmq_server-3.6.6")))
                {
                    ExtractRabbit();
                }

                StartRabbit();
            }
        }

        private static Process StartWorker(string name)
        {
            var p = new Process
            {
                StartInfo =
                {
                    FileName = string.Format(WorkerExe, name),
                    WorkingDirectory = string.Format(WorkerDir, name)
                }
            };

            p.Start();
            return p;
        }

        private static void Publish()
        {
            Bus.Publish(Factory.GetTitleNode("http://www.wp.pl"));
            Bus.Publish(Factory.GetTitleNode("http://www.onet.pl"));
            Bus.Publish(Factory.GetTitleNode("http://www.interia.pl"));
        }

        private static void Flow1()
        {
            Bus.Publish(new Node { Name = "flow1", Data = new { Guid = Guid.NewGuid() } });
        }

        private static void Flow2()
        {
            Bus.Publish(new Node { Name = "flow2", Data = new { Guid = Guid.NewGuid() } });
        }

        private static void StartRabbit()
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(Dir, "RabbitMQ", "start.bat"),
                    WorkingDirectory = Path.Combine(Dir, "RabbitMQ")
                }
            };
            process.Start();

            while (!UrlExists("http://localhost:15672"))
            {
                Thread.Sleep(1000);
            }
        }

        private static void ExtractRabbit()
        {
            var extract = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(Dir, "RabbitMQ", "extract.bat"),
                    WorkingDirectory = Path.Combine(Dir, "RabbitMQ")
                }
            };
            extract.Start();
            extract.WaitForExit();
        }
    }
}
