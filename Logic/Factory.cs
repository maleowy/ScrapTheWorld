using System;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace Logic
{
    public static class Factory
    {
        public static List<Node> Nodes = new List<Node>
        {
            new Node { Name = "publish", Open = true, Data = new { Url = "https://duckduckgo.com/" }, NextNode = "title" },

            new Node { Name = "flow1", Open = true, Data = new { Url = "https://duckduckgo.com/" }, NextNode = "fill1" },
            new Node { Name = "fill1", Script = "fill", Data = new { QuerySelector = "#search_form_input_homepage", Value = "Test" }, NextNode = "click1" },
            new Node { Name = "click1", Script = "click", Data = new { QuerySelector = "#search_button_homepage" }, NextNode = "title" },
            new Node { Name = "title", Script = "title", ReturnResults = true },

            new Node { Name = "flow2", Open = true, Data = new { Url = "https://duckduckgo.com/" }, NextNode = "fill2" },
            new Node { Name = "fill2", Script = "fill", Data = new { QuerySelector = "#search_form_input_homepage", Value = "Test" }, NextNode = "click2" },
            new Node { Name = "click2", Script = "click", Data = new { QuerySelector = "#search_button_homepage" }, NextNode = "urls" },
            new Node { Name = "urls", Script = "urls", NextNode = "limit" },
            new Node { Name = "limit", Script = "limit", Data = new { Limit = 3 }, NextNode = "split" },
            new Node { Name = "split", Script = "split", Data = new { Limit = 1 }, NextNode = "set" },
            new Node { Name = "set", Script = "set", Data = new { From = "self.LastResults[0]", To = "Url" }, NextNode = "open" },
            new Node { Name = "open", Open = true, NextNode = "title", NextWorker = true }
        };

        public static Dictionary<string, string> Scripts = new Dictionary<string, string>
        {
            { "fill", @"document.querySelector(self.Data.QuerySelector).value = self.Data.Value;" },
            { "click", "document.querySelector(self.Data.QuerySelector).click();" },
            { "title", @"self.Results = [document.title];" },
            { "split", "var arr = []; if (self.LastResults && self.LastResults.length > 0) { var stringified = JSON.stringify(self); for(var i=0; i < self.LastResults.length; i = i + self.Data.Limit) { var m = JSON.parse(stringified); m.LastResults = []; m.Results = []; for (var j=i; j < i + self.Data.Limit && j < self.LastResults.length; j++) { m.Results.push(self.LastResults[j]); } arr.push(m); } } return JSON.stringify(arr);" },
            { "limit", "var results = []; if (self.LastResults && self.LastResults.length > self.Data.Limit) { for(var i=0; i < self.Data.Limit; i++) { results.push(self.LastResults[i]); } } self.LastResults = []; self.Results = results;" },
            { "urls", @"var urls = []; document.body.innerText.match(/https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)/g).forEach(function(element) { if (urls.indexOf(element) == -1) { urls.push(element) } }); var results = []; urls.forEach(function(element) { results.push(element); }); self.Results = results;" },
            { "set", @"self.Data[self.Data.To] = eval(self.Data.From);" }
        };

        public static List<Node> GetNodes()
        {
            return Nodes;
        }

        public static Dictionary<string, string> GetScripts()
        {
            return Scripts;
        }

        public static Node FindNodeByName(string name)
        {
            return Nodes.First(n => n.Name == name).Clone();
        }

        public static Node GetTitleNode(string url)
        {
            return new Node
            {
                Open = true,
                Data = new { Guid = Guid.NewGuid(), Url = url },
                Script = "self.Results = [document.title];",
                ReturnResults = true
            };
        }
    }
}
