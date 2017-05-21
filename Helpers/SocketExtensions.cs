using System;
using System.Net;
using System.Net.Sockets;

namespace Helpers
{
    public static class SocketExtensions
    {
        public static void Connect(this Socket socket, EndPoint endpoint, TimeSpan timeout)
        {
            var result = socket.BeginConnect(endpoint, null, null);

            bool success = result.AsyncWaitHandle.WaitOne(timeout, true);

            if (success)
            {
                socket.EndConnect(result);
            }
            else
            {
                socket.Close();
                throw new SocketException(10060); // Connection timed out.
            }
        }
    }
}