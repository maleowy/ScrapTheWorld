using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Models;
using Newtonsoft.Json;

namespace Logic
{
    public class Logic
    {
        public static string GetBusConfiguration()
        {
            return "host=localhost;username=guest;password=guest;timeout=60;prefetchcount=1";
        }

        public static List<Node> Nodes = Factory.GetNodes();

        public static Dictionary<string, string> Scripts = Factory.GetScripts();

        public static Func<Node, Task> GetLogic(Func<string, Task> onNavigate, Func<string, Task<string>> onEvaluate, Func<string, Task> onResult, Action<Exception> onError)
        {
            return async node =>
            {
                try
                {
                    if (string.IsNullOrEmpty(node.Name) == false)
                    {
                        node = FindNodeByName(node.Name);
                    }

                    await Process(node, onNavigate, onEvaluate, onResult);
                }
                catch (Exception ex)
                {
                    onError(ex);
                }
            };
        }

        private static async Task Process(Node node, Func<string, Task> onNavigate, Func<string, Task<string>> onEvaluate, Func<string, Task> onResult)
        {
            Console.WriteLine($"Start {node.Name} {(node.Open ? node.AdditionalData.Url : "")} {GetCurrentTime()}");

            if (node.Open)
            {
                await onNavigate.Invoke(node.AdditionalData.Url);
            }

            if (!string.IsNullOrEmpty(node.Script))
            {
                string script = @"(function() { " +
                                $"var self = {JsonConvert.SerializeObject(node)};" +
                                $"{(Scripts.ContainsKey(node.Script) ? Scripts[node.Script] : node.Script)}" +
                                "return JSON.stringify([self]);" +
                                "})();";

                string scriptResult = await onEvaluate.Invoke(script);
                var list = JsonConvert.DeserializeObject<List<object>>(scriptResult);
                var nodes = (list ?? new List<object>()).Select(x => JsonConvert.DeserializeObject<Node>(x.ToString())).ToList();

                nodes.ForEach(async n =>
                {
                    n.Results = n.Results ?? new List<dynamic>();

                    if (n.ReturnResults)
                    {
                        await onResult(JsonConvert.SerializeObject(n.Results));
                    }

                    if (n.NextNode != null)
                    {
                        await Next(n, onNavigate, onEvaluate, onResult);
                    }
                });

            }
            else if (node.NextNode != null)
            {
                await Next(node, onNavigate, onEvaluate, onResult);
            }

            //Console.WriteLine($"End {node.Name} {GetCurrentTime()}");
        }

        private static async Task Next(Node node, Func<string, Task> onNavigate, Func<string, Task<string>> onEvaluate, Func<string, Task> onResult)
        {
            var newNode = FindNodeByName(node.NextNode);

            await Process(newNode, onNavigate, onEvaluate, onResult);               
        }

        private static Node FindNodeByName(string name)
        {
            return Nodes.First(n => n.Name == name).Clone();
        }

        private static string GetCurrentTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }
    }
}
