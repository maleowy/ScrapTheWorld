using System;
using System.Collections.Generic;
using System.Diagnostics;
using EasyNetQ;
using Models;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Logic;

namespace Frontend
{
    class Program
    {
        public static IBus Bus;

        public static string Dir;
        public static string WorkerDir;
        public static string WorkerExe;

        static void Main(string[] args)
        {
            Console.Title = "Frontend";

            PreparePaths();
            AddRabbit();

            Bus = RabbitHutch.CreateBus("host=localhost");
            Bus.SubscribeAsync<Result>("subscriptionId", x =>
            {
                Console.WriteLine(x.Data);
                return Task.FromResult(true);
            });

            Console.WriteLine("o - offscreen, f - winforms, w - wpf, p - publish, 1 - flow, esc - exit");

            var workers = new List<Process>();

            while (true)
            {
                ConsoleKey key = Console.ReadKey().Key;

                switch (key)
                {
                    case ConsoleKey.O:
                        workers.Add(StartWorker("OffScreen"));
                        break;
                    case ConsoleKey.F:
                        workers.Add(StartWorker("WinForms"));
                        break;
                    case ConsoleKey.W:
                        workers.Add(StartWorker("WPF"));
                        break;
                    case ConsoleKey.P:
                        Publish();
                        break;
                    case ConsoleKey.D1:
                        Flow1();
                        break;
                    case ConsoleKey.Escape:
                        workers.ForEach(p =>
                        {
                            try
                            {
                                p.Kill();
                            }
                            catch {}
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

            WorkerDir = Path.Combine(Dir, @"Worker.CefSharp.{0}\bin\x86\Debug");
            WorkerExe = Path.Combine(WorkerDir, @"Worker.CefSharp.{0}.exe");
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
            Bus.Publish(new Node { Name = "flow1" });
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

        private static bool UrlExists(string url)
        {
            var req = WebRequest.Create(url);

            try
            {
                req.GetResponse();
            }
            catch
            {
                return false;
            }

            return true;
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
