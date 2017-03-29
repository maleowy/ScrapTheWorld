using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace Extensions
{
    public class BoundObject
    {
        public void Test(string text)
        {
            Console.WriteLine(new string(text.Reverse().ToArray()));
        }
    }
}
