using System;
using System.Text;
using Logic;

namespace Tests.Sprache
{
    class Program
    {
        static void Main(string[] args)
        {
            var commands = new StringBuilder();
            commands.Append("open Url \"https://duckduckgo.com/\"");
            commands.AppendLine();
            commands.Append("fill QuerySelector \"#search_form_input_homepage\" Value \"Test\"");
            commands.AppendLine();
            commands.Append("click QuerySelector \"#search_button_homepage\"");
            commands.AppendLine();
            commands.Append("--wait 1000");
            commands.AppendLine();
            commands.Append("urls");
            commands.AppendLine();
            commands.Append("limit Limit \"3\"");
            commands.AppendLine();
            commands.Append("split Limit \"1\"");
            commands.AppendLine();
            commands.Append("set From \"self.LastResults[0]\" To \"Url\"");
            commands.AppendLine();
            commands.Append("worker open");
            commands.AppendLine();
            commands.Append("title return");

            var nodes = NodesParser.ParseString("flow2", commands.ToString());
        }
    }
}
