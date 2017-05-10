using System;
using System.Net;
using System.Net.Sockets;

namespace Helpers
{
    public static class Helpers
    {
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

        public static void IgnoreExceptions(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch { }
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
    }
}
