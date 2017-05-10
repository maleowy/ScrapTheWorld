﻿using System;
using System.Collections.Generic;
using System.Dynamic;
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

        public static Func<Node, Task> GetLogic(Action<Node> onStart, Func<string, Task> onNavigate, Func<string, Task<string>> onEvaluate, Func<Node, Task> onNext, Func<Node, Task> onResult, Action<Node> onError, Action<Node> onEnd)
        {
            return async node =>
            {
                try
                {
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

        private static async Task Process(Node node, Action<Node> onStart, Func<string, Task> onNavigate, Func<string, Task<string>> onEvaluate, Func<Node, Task> onNext, Func<Node, Task> onResult, Action<Node> onError, Action<Node> onEnd)
        {
            try
            { 
                onStart(node);
            
                if (node.Open)
                {
                    await onNavigate.Invoke(node.Data.Url);
                }

                if (node.WaitTime > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(node.WaitTime));
                }

                if (!string.IsNullOrEmpty(node.Script))
                {
                    string script = "window.alert = function() {};" +
                                    "window.onbeforeunload = null;" +
                                    "window.onunload = null;" +

                                    @"(function() { " +
                                    $"var self = {JsonConvert.SerializeObject(node)};" +
                                    "try { " +
                                    $"{(Scripts.ContainsKey(node.Script) ? Scripts[node.Script] : node.Script)}" +
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
                await onNext(newNode);
            }
            else
            {
                await Process(newNode, onStart, onNavigate, onEvaluate, onNext, onResult, onError, onEnd);
            }
        }
    }
}
