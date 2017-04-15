using System.Collections.Generic;
using Models;

namespace Logic
{
    public static class Factory
    {
        public static List<Node> GetNodes()
        {
            return new List<Node>
            {
                new Node { Name = "flow1", Open = true, AdditionalData = new { Url = "https://duckduckgo.com/" }, NextNode = "fill" },
                new Node { Name = "fill", Script = "fill", AdditionalData = new { QuerySelector = "#search_form_input_homepage", Value = "Test" }, NextNode = "click" },
                new Node { Name = "click", Script = "click", AdditionalData = new { QuerySelector = "#search_button_homepage" }, NextNode = "title" },
                new Node { Name = "title", Script = "title", ReturnResults = true }
            };
        }

        public static Dictionary<string, string> GetScripts()
        {
            return new Dictionary<string, string>
            {
                { "fill", @"document.querySelector(self.AdditionalData.QuerySelector).value = self.AdditionalData.Value;" },
                { "click", "document.querySelector(self.AdditionalData.QuerySelector).click();" },
                { "title", @"self.Results = [document.title];" }
            };
        }

        public static Node GetTitleNode(string url)
        {
            return new Node
            {
                Open = true,
                AdditionalData = new { Url = url },
                Script = "self.Results = [document.title];",
                ReturnResults = true
            };
        }
    }
}
