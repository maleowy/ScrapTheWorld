using System;
using Nancy.Hosting.Self;

namespace Flow
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Flow";
            string url = "http://localhost:8082";

            using (var host = new NancyHost(new Uri(url)))
            {
                host.Start();
                Console.WriteLine(url);
                Console.ReadLine();
            }
        }
    }
}
