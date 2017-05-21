using System;
using System.Collections.Generic;
using System.Diagnostics;
using EasyNetQ;
using Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Logic;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Open.Nat;
using Serilog;
using Serilog.Core;
using static Helpers.Helpers;

namespace Frontend
{
    class Program
    {
        public static readonly string IP = GetLocalIPAddress();
        public static readonly int Port = 8080;
        public static string ExternalIP;
        public static readonly int ExternalPort = 80;

        public static string RabbitIP = "localhost";
        public static string PersistenceIP = "localhost";
        public static readonly int PersistencePort = 8081;
 
        public static IBus Bus;
        public static Logger Logger;

        public static string Dir;
        public static string WorkerDir;
        public static string WorkerExe;

        static void Main(string[] args)
        {
            Console.Title = "Frontend";
            Console.WriteLine("Waiting for RabbitMQ...");

            MapPorts().Wait();

            PreparePaths();
            AddChromePortable();
            AddRabbit();
            AddPersistence();

            Bus = RabbitHutch.CreateBus(GetBusConfiguration(RabbitIP, "guest", "guest", 50));

            Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            var resultSubscription = Bus.SubscribeAsync<Result>("subscriptionId", x =>
            {
                var data = JsonConvert.SerializeObject(x.Node.Results);
                HelloHub.ReturnResults(x.Node.Data.Guid, data, "done");
                Logger.Information(data);
                return Task.FromResult(true);
            });

            var errorSubscription = Bus.SubscribeAsync<ErrorResult>("subscriptionId", x =>
            {
                HelloHub.ReturnResults(x.Node.Data.Guid, x.Node.Error, "error");
                Logger.Error("{@Node}", x.Node);
                return Task.FromResult(true);
            });

            Console.Clear();
            Console.WriteLine($"Local - http://{IP}:{Port}");
            Console.WriteLine($"Public - http://{ExternalIP}:{ExternalPort}");
            Console.WriteLine();
            Console.WriteLine("o - offscreen, f - winforms, w - wpf, s - selenium, r - remote debugging, p - publish, 1 - flow, 2 - flow, esc - exit");

            var workers = new List<Process>();

            var url = $"http://+:{Port}";


            using (WebApp.Start<Startup>(url))
            {
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
                        case ConsoleKey.S:
                            workers.Add(StartWorker("Selenium"));
                            break;
                        case ConsoleKey.R:
                            workers.Add(StartWorker("RemoteDebugging"));
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
        }

        private static async Task MapPorts()
        {
            var discoverer = new NatDiscoverer();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
            ExternalIP = device.GetExternalIPAsync().Result.ToString();

            await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, Port, ExternalPort, "Scrap The World"));
            await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, PersistencePort, PersistencePort, "Persistence"));
        }

        private static void PreparePaths()
        {
            Dir = AppDomain.CurrentDomain.BaseDirectory;
            Dir = Path.GetFullPath(Path.Combine(Dir, @"..\..\..\"));

            WorkerDir = Path.Combine(Dir, @"Worker.{0}\bin\" + (Debugger.IsAttached ? "Debug" : "Release"));
            WorkerExe = Path.Combine(WorkerDir, @"Worker.{0}.exe");
        }

        private static void AddRabbit()
        {
            var erl = Process.GetProcessesByName("erl");

            if (erl.Length == 0)
            {
                try
                {
                    RabbitIP = FindRabbit();
                }
                catch
                {
                    if (Directory.Exists(Path.Combine(Dir, @"RabbitMQ")) && !Directory.Exists(Path.Combine(Dir, @"RabbitMQ\rabbitmq_server-3.6.6")))
                    {
                        ExtractRabbit();
                    }

                    StartRabbit();
                }
            }
        }

        private static void AddChromePortable()
        {
            if (Directory.Exists(Path.Combine(Dir, @"GoogleChromePortable")))
            {
                if (!File.Exists(Path.Combine(Dir, @"GoogleChromePortable\GoogleChromePortable.exe")))
                {
                    Process.Start(Path.Combine(Dir, @"GoogleChromePortable_58.0.3029.81_online.paf.exe"));
                }
            }
        }

        private static void AddPersistence()
        {
            var erl = Process.GetProcessesByName("Persistence");

            if (erl.Length == 0)
            {
                try
                {
                    PersistenceIP = FindServerInLocalNetwork(PersistencePort);
                }
                catch
                {
                    if (Directory.Exists(Path.Combine(Dir, @"Persistence")))
                    {
                        var persistencePath = Path.Combine(Dir, $@"Persistence\bin\{(Debugger.IsAttached ? "Debug" : "Release")}\Persistence.exe");

                        if (File.Exists(persistencePath))
                        {
                            Process.Start(persistencePath);
                        }
                    }
                }
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
