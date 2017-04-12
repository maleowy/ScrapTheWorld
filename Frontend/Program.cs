using System;
using System.Collections.Generic;
using System.Diagnostics;
using EasyNetQ;
using Models;
using System.IO;
using System.Net;
using System.Threading;

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
            Bus.Subscribe<Result>("subscriptionId", x => Console.WriteLine(x.Data));

            Console.WriteLine("o - offscreen, f - winforms, w - wpf, p - publish, esc - exit");

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
                    case ConsoleKey.Escape:
                        workers.ForEach(p => p.Kill());
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
            Bus.Publish(new Node { Url = "http://www.wp.pl", Script = "document.title" });
            Bus.Publish(new Node { Url = "http://www.onet.pl", Script = "document.title" });
            Bus.Publish(new Node { Url = "http://www.interia.pl", Script = "document.title" });
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
