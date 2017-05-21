using System;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Helpers
{
    public static class Helpers
    {
        public static string GetBusConfiguration(string ipAddress = "localhost", string userName = "guest", string password = "guest", int prefetchCount = 1, int timeout = 60)
        {
            return $"host={ipAddress};username={userName};password={password};timeout={timeout};prefetchcount={prefetchCount}";
        }

        public static bool UrlExists(string url)
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

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("Local IP Address Not Found!");
        }

        public static string FindRabbit()
        {
            return FindServerInLocalNetwork(15672);
        }

        public static string FindServerInLocalNetwork(int port)
        {
            if (IsServerRunning("127.0.0.1", port))
            {
                return "localhost";
            }

            var root = new DirectoryEntry("WinNT:");

            foreach (DirectoryEntry computers in root.Children)
            {
                foreach (DirectoryEntry computer in computers.Children)
                {
                    if (computer.Name != "Schema")
                    {
                        Console.WriteLine(computer.Name);
                        var entry = Dns.GetHostEntry(computer.Name);

                        foreach (var ip in entry.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork))
                        {
                            if (IsServerRunning(ip.ToString(), port))
                            {
                                return ip.ToString();
                            }
                        }
                    }
                }
            }

            throw new Exception($"Server Listening On Port {port} Not Found!");
        }

        public static bool IsServerRunning(string ipAddress, int port)
        {
            try
            {
                var client = new Socket(
                     AddressFamily.InterNetwork,
                     SocketType.Stream,
                     ProtocolType.Tcp);

                IPAddress ip = IPAddress.Parse(ipAddress);
                IPEndPoint endPoint = new IPEndPoint(ip, port);

                client.Connect(endPoint, TimeSpan.FromSeconds(5));

                return client.Connected;
            }
            catch
            {
                return false;
            }
        }

        public static void IgnoreExceptions(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch { }
        }
    }
}
