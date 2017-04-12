using System;
using System.Collections.Generic;
using System.Diagnostics;
using EasyNetQ;
using Models;
using System.IO;
using System.Threading;

namespace Frontend
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Frontend";

            var dir = AppDomain.CurrentDomain.BaseDirectory;
            dir = Path.GetFullPath(Path.Combine(dir, @"..\..\..\..\"));

            if (Directory.Exists(Path.Combine(dir, @"RabbitMQ")))
            {
                if (!Directory.Exists(Path.Combine(dir, @"RabbitMQ\rabbitmq_server-3.6.6")))
                {
                    var extract = new Process
                    {
                        StartInfo =
                        {
                            FileName = Path.Combine(dir, "RabbitMQ", "extract.bat"),
                            WorkingDirectory = Path.Combine(dir, "RabbitMQ")
                        }
                    };
                    extract.Start();
                    extract.WaitForExit();
                }

                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = Path.Combine(dir, "RabbitMQ", "start.bat"),
                        WorkingDirectory = Path.Combine(dir, "RabbitMQ")
                    }
                };
                process.Start();

                Thread.Sleep(20*1000);
            }

            IBus Bus = RabbitHutch.CreateBus("host=localhost");
            Bus.Subscribe<Result>("subscriptionId", x => Console.WriteLine(x.Data));

            Console.WriteLine("o - offscreen, f - winforms, w - wpf, p - publish, esc - exit");

            var workerDir = Path.Combine(dir, @"Worker.CefSharp.{0}\bin\x86\Debug");
            var workerExe = Path.Combine(workerDir, @"Worker.CefSharp.{0}.exe");

            var processes = new List<Process>();

            while (true)
            {
                ConsoleKey key = Console.ReadKey().Key;

                switch (key)
                {
                    case ConsoleKey.O:
                        var o = new Process {
                            StartInfo =
                            {
                                FileName = string.Format(workerExe, "OffScreen"),
                                WorkingDirectory = string.Format(workerDir, "OffScreen")
                            }
                        };
                        processes.Add(o);
                        o.Start();
                        break;
                    case ConsoleKey.F:
                        var f = new Process
                        {
                            StartInfo =
                            {
                                FileName = string.Format(workerExe, "WinForms"),
                                WorkingDirectory = string.Format(workerDir, "WinForms")
                            }
                        };
                        processes.Add(f);
                        f.Start();
                        break;
                    case ConsoleKey.W:
                        var w = new Process
                        {
                            StartInfo =
                            {
                                FileName = string.Format(workerExe, "WPF"),
                                WorkingDirectory = string.Format(workerDir, "WPF")
                            }
                        };
                        processes.Add(w);
                        w.Start();
                        break;
                    case ConsoleKey.P:
                        Bus.Publish(new Node { Url = "http://www.wp.pl", Script = "document.title" });
                        Bus.Publish(new Node { Url = "http://www.onet.pl", Script = "document.title" });
                        Bus.Publish(new Node { Url = "http://www.interia.pl", Script = "document.title" });
                        break;
                    case ConsoleKey.Escape:
                        processes.ForEach(p => p.Kill());
                        Environment.Exit(0);
                        break;

                }

                Console.WriteLine();
            }
        }
    }
}
