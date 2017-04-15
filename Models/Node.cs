using System.Collections.Generic;
using System.Dynamic;

namespace Models
{
    public class Node
    {
        public string Name { get; set; }
        public bool Open { get; set; }
        public string Script { get; set; }
        public string NextNode { get; set; }

        public bool ReturnResults { get; set; }

        public dynamic Data { get; set; }
        public List<object> Results { get; set; }

        public Node()
        {
            Data = new ExpandoObject();
            Results = new List<object>();
        }
    }
}
