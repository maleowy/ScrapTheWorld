using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Models
{
    public class Node
    {
        public string Name { get; set; }
        public bool Open { get; set; }
        public string Script { get; set; }
        public string NextNode { get; set; }
        public int WaitTime { get; set; }

        public bool NextWorker { get; set; }
        public bool ReturnResults { get; set; }

        public dynamic Data { get; set; }
        public List<object> LastResults { get; set; }
        public List<object> Results { get; set; }
        public string Error { get; set; }

        public Node()
        {
            Data = new ExpandoObject();
            LastResults = new List<object>();
            Results = new List<object>();
        }

        public override string ToString()
        {
            return $"{(NextWorker ? "worker " : "")}{(WaitTime > 0 ? "wait " + WaitTime + " " : "")}{(Open ? "open" : Script)}{ExpandoToString(Data)}{(ReturnResults ? " return" : "")}";
        }

        private string ExpandoToString(ExpandoObject expando)
        {
            return expando.Any() ? " " + string.Join(" ", expando.Select(x => x.Key + " \"" + x.Value + "\"")) : "";
        }
    }
}
