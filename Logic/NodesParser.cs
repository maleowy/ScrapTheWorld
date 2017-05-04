using System;
using System.Collections.Generic;
using System.Dynamic;
using Models;
using Sprache;
using System.Linq;

namespace Logic
{
    public static class NodesParser
    {
        private static readonly Parser<IOption<string>> CommentParser = Parse.Char('-').AtLeastOnce().Text().Token().Optional();

        private static readonly Parser<string> IdentifierParser = Parse.LetterOrDigit.AtLeastOnce().Text().Token();

        private static readonly Parser<string> QuotedTextParser =
            (from open in Parse.Char('"')
            from content in Parse.CharExcept('"').Many().Text()
            from close in Parse.Char('"')
            select content).Token();

        private static readonly Parser<IOption<int>> WaitTimeParser = Parse.Optional(
            (from ret in Parse.String("wait").Text()
            from white in Parse.WhiteSpace.AtLeastOnce()
            from wait in Parse.Number
            select int.Parse(wait)).Token());

        private static readonly Parser<KeyValuePair<string, object>> PairParser =
            from name in IdentifierParser
            from quote in QuotedTextParser
            select new KeyValuePair<string, object>(name, quote);

        private static readonly Parser<ExpandoObject> DataParser = PairParser.Many()
            .Select(pairs => pairs.ToDictionary(i => i.Key, i => i.Value).ToExpando());

        private static readonly Parser<IOption<string>> ReturnResultsParser = Parse.String("return").Text().Token().Optional().End();
        private static readonly Parser<IOption<string>> NextWorkerParser = Parse.String("worker").Text().Token().Optional();

        private static readonly Parser<Node> NodeParser =
            from comment in CommentParser
            from nextWorker in NextWorkerParser
            from waitTime in WaitTimeParser
            from script in IdentifierParser.Optional()
            from data in DataParser
            from returnResults in ReturnResultsParser
            select !comment.IsDefined ? new Node
            {
                NextWorker = !nextWorker.IsEmpty,
                WaitTime = waitTime.GetOrDefault(),
                Open = (script.GetOrDefault() ?? "") == "open",
                Script = (script.GetOrDefault() ?? "") == "open" ? null : script.GetOrDefault(),
                Data = data,
                ReturnResults = !returnResults.IsEmpty             
            } : null;

        public static List<Node> ParseString(string name, string commands)
        {
            var lines = commands.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var nodes = lines.Select(x => NodeParser.Parse(x)).Where(x => x != null).ToList();

            if (nodes.Count == 0)
            {
                return new List<Node>();
            }

            nodes[0].Name = name;

            for (int i = 1; i < nodes.Count; i++)
            {
                nodes[i].Name = name + (i+1);
            }

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                nodes[i].NextNode = nodes[i + 1].Name;
            }

            return nodes;
        }
    }
}
