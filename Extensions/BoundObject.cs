using System;
using System.Linq;

namespace Extensions
{
    public class BoundObject
    {
        public void ReverseText(string text)
        {
            Console.WriteLine(new string(text.Reverse().ToArray()));
        }
    }
}
