using System;
using Nancy.Hosting.Self;
using static Helpers.Helpers;

namespace Flow
{
    class Program
    {
        public static readonly string IP = GetLocalIPAddress();
        public static readonly int Port = 8082;
        public static string PersistenceIP;
        public static readonly int PersistencePort = 8081;

        static void Main(string[] args)
        {
            Console.Title = "Flow";
            PersistenceIP = FindServerInLocalNetwork(PersistencePort);

            using (var host = new NancyHost(new Uri($"http://localhost:{Port}")))
            {
                host.Start();
                Console.WriteLine($"http://{IP}:{Port}");
                Console.ReadLine();
            }
        }
    }
}
