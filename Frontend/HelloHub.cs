using System;
using System.Collections.Generic;
using System.Dynamic;
using Logic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Models;

namespace Frontend
{
    [HubName("helloHub")]
    public class HelloHub : Hub
    {
        private static IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<HelloHub>();

        public void Search(string message)
        {
            var arr = message.Split(' ');

            if (arr.Length == 0)
                return;

            var node = new Node();

            var connectionId = Context.ConnectionId;
            var token = Context.QueryString.Get("token");

            node.Data = new { Guid = connectionId };

            try
            {
                node = Factory.FindNodeByName(arr[0]);

                var args = new Dictionary<string, object> {
                    { "Guid", connectionId }
                };

                for (int i = 1; i < arr.Length - 1; i = i + 2)
                {
                    args.Add(arr[i], arr[i + 1]);
                }

                node.Data = ((ExpandoObject)node.Data).Merge(args.ToExpando());

                Program.Bus.Publish(node);
            }
            catch (Exception ex)
            {
                node.Error = ex.Message;
                Program.Bus.Publish(new ErrorResult { Node = node });
            }
        }

        public static void ReturnResults(string connectionId, string data, string image)
        {
            if (connectionId == null)
            {
                _hubContext.Clients.All.addResult(data, image);
            }
            else
            {
                _hubContext.Clients.Client(connectionId).addResult(data, image);
            }
        }
    }
}
