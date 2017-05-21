using System;
using static Helpers.Helpers;

namespace Tests.FindServerInLocalNetwork
{
    class Program
    {
        static void Main(string[] args)
        {
            string rabbitIp = FindRabbit();
            string persistenceIp = Helpers.Helpers.FindServerInLocalNetwork(8081);

            Console.WriteLine("RabbitMQ IP: {0}", rabbitIp);
            Console.WriteLine("Persistence IP: {0}", persistenceIp);
            Console.ReadLine();
        }
    }
}
