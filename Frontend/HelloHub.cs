using System;
using System.Collections.Generic;
using System.Linq;
using Logic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Models;
using RestSharp;
using static Logic.Logic;

namespace Frontend
{
    [HubName("helloHub")]
    public class HelloHub : Hub
    {
        private static IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<HelloHub>();

        public void Search(string message)
        {
            var arr = message.Split(' ');

            if (arr.Length == 0 || !arr[0].Contains("/"))
                return;

            var node = new Node();

            var connectionId = Context.ConnectionId;
            var token = Context.QueryString.Get("token");

            try
            {
                var req = PrepareRequest(arr, node, connectionId);

                if (node.Nodes.Count == 0 || !node.Nodes.Any(n => arr[0].Contains(n.Name)))
                    return;

                Program.Bus.Publish(req);
            }
            catch (Exception ex)
            {
                node.Error = ex.Message;
                Program.Bus.Publish(new ErrorResult { Node = node });
            }
        }

        private static Node PrepareRequest(string[] arr, Node node, string connectionId)
        {
            var tmp = arr[0].Split('/');

            var flow = tmp[0];
            node.Name = tmp[1];

            var args = new Dictionary<string, object> {
                { "Guid", connectionId }
            };

            for (int i = 1; i < arr.Length - 1; i = i + 2)
            {
                args.Add(arr[i], arr[i + 1]);
            }

            node.Data = args.ToExpando();

            var client = new RestClient($"http://{Program.PersistenceIP}:{Program.PersistencePort}");
            var req = new RestRequest("/api/Persistence?table=scripts");

            node.Scripts = new Dictionary<string, string>();

            foreach (var item in client.Execute<dynamic>(req).Data)
            {
                node.Scripts.Add(item["Key"], item["Value"]);
            }

            req = new RestRequest("/api/Persistence?table=flows&key=" + flow);

            var data = client.Execute<dynamic>(req).Data;
            var json = data?["Value"] ?? "{}";
            node.Nodes = GetNodes(json);

            var flowScripts = node.Nodes.Select(n => n.Script).ToArray();
            node.Scripts = node.Scripts.Where(kvp => flowScripts.Contains(kvp.Key)).ToDictionary(i => i.Key, i => i.Value);

            return node;
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
