using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Extensions.MonoHttp;

namespace Logic
{
    public class Logic
    {
        public static List<Node> GetNodes(string json)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(json);

            if (jsonObj == null)
            {
                return new List<Node>();
            }

            var nodesDictionary = new Dictionary<string, List<Node>>();

            foreach (dynamic node in jsonObj.operators ?? new List<dynamic>())
            {
                string id = node.Name.ToString();
                var title = node.First.properties.title.ToString();
                var commands = node.First.properties.commands.ToString();

                var nodes = NodesParser.ParseString(title, commands);
                nodesDictionary.Add(id, nodes);
            }

            foreach (dynamic node in jsonObj.links ?? new List<dynamic>())
            {
                string fromId = node.First.fromOperator.ToString();
                string toId = node.First.toOperator.ToString();

                if (fromId == toId)
                    continue;

                var last = nodesDictionary[fromId].LastOrDefault();

                if (last != null)
                {
                    last.NextNode = nodesDictionary[toId].FirstOrDefault()?.Name;
                }
            }

            return nodesDictionary.SelectMany(x => x.Value).ToList();
        }

        public static Func<Node, Task> GetLogic(Action<Node> onStart, Func<string, Task> onNavigate, Func<string, Task<string>> onEvaluate, Func<Node, Task> onNext, Func<Node, Task> onResult, Action<Node> onError, Action<Node> onEnd)
        {
            return async node =>
            {
                try
                {
                    UpdateFlow(node);

                    if (node.NextWorker == false && string.IsNullOrEmpty(node.Name) == false)
                    {
                        var found = Factory.FindNodeByName(node.Name);
                        found.Data = ((ExpandoObject)found.Data).Merge((ExpandoObject)node.Data);
                        node = found;
                    }

                    await Process(node, onStart, onNavigate, onEvaluate, onNext, onResult, onError, onEnd);
                }
                catch (Exception ex)
                {
                    node.Error = ex.ToString();
                    onError(node);
                }
            };
        }

        private static void UpdateFlow(Node node)
        {
            if ((node.Nodes ?? new List<Node>()).Any())
            {
                Factory.Nodes = node.Nodes.Clone();
                node.Nodes = null;
            }

            if ((node.Scripts ?? new Dictionary<string, string>()).Any())
            {
                Factory.Scripts = node.Scripts.Clone();
                node.Scripts = null;
            }
        }

        private static async Task Process(Node node, Action<Node> onStart, Func<string, Task> onNavigate, Func<string, Task<string>> onEvaluate, Func<Node, Task> onNext, Func<Node, Task> onResult, Action<Node> onError, Action<Node> onEnd)
        {
            try
            { 
                onStart(node);
            
                if (node.Open)
                {
                    if (!IsValidUrl(node.Data.Url))
                    {
                        throw new ArgumentException("Url not valid", node.Data.Url);
                    }

                    await onNavigate.Invoke(node.Data.Url);
                }

                if (node.WaitTime > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(node.WaitTime));
                }

                if (node.AskQuestion)
                {
                    var client = new RestClient("http://" + node.Data.Frontend);
                    var req = new RestRequest("/questionAnswer?q=" + HttpUtility.UrlEncode(node.Data.Question)) { Timeout = int.MaxValue };
                    var response = client.Execute(req);
                    node.Data.Answer = response.Content;
                }

                if (!string.IsNullOrEmpty(node.Script))
                {
                    string script = "window.alert = function() {};" +
                                    "window.onbeforeunload = null;" +
                                    "window.onunload = null;" +

                                    @"(function() { " +
                                    $"var self = {JsonConvert.SerializeObject(node)};" +
                                    "try { " +
                                    $"{(Factory.Scripts.ContainsKey(node.Script) ? Factory.Scripts[node.Script] : node.Script)}" +
                                    "} catch(err) {" +
                                    "self.Error = err.message;" +
                                    "self.NextNode = null;" +
                                    "}" +
                                    "return JSON.stringify([self]);" +
                                    "})();";

                    string scriptResult = await onEvaluate.Invoke(script);
                    var list = JsonConvert.DeserializeObject<List<object>>(scriptResult);
                    var nodes = (list ?? new List<object>()).Select(x => JsonConvert.DeserializeObject<Node>(x.ToString())).ToList();

                    foreach (var n in nodes)
                    {
                        if (n.Error != null)
                        {
                            throw new Exception(n.Error);
                        }

                        n.Results = n.Results ?? new List<dynamic>();

                        if (n.ReturnResults)
                        {
                            await onResult(n);
                        }

                        if (n.NextNode != null)
                        {
                            await Next(n, onStart, onNavigate, onEvaluate, onNext, onResult, onError, onEnd);
                        }
                    }

                }
                else if (node.NextNode != null)
                {
                    await Next(node, onStart, onNavigate, onEvaluate, onNext, onResult, onError, onEnd);
                }

                onEnd(node);
            }
            catch (Exception ex)
            {
                node.Error = ex.ToString();
                onError(node);
            }
        }

        private static async Task Next(Node node, Action<Node> onStart, Func<string, Task> onNavigate, Func<string, Task<string>> onEvaluate, Func<Node, Task> onNext, Func<Node, Task> onResult, Action<Node> onError, Action<Node> onEnd)
        {
            var newNode = Factory.FindNodeByName(node.NextNode);
            newNode.LastResults = new List<object>(node.Results);
            newNode.Data = ((ExpandoObject)node.Data).Merge((ExpandoObject)newNode.Data);

            if (newNode.NextWorker)
            {
                newNode.Nodes = Factory.Nodes;
                newNode.Scripts = Factory.Scripts;

                await onNext(newNode);
            }
            else
            {
                await Process(newNode, onStart, onNavigate, onEvaluate, onNext, onResult, onError, onEnd);
            }
        }

        private static bool IsValidUrl(string uriName)
        {
            Uri uriResult;

            return Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
