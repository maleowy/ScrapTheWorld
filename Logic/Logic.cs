using System;
using System.Globalization;
using System.Threading.Tasks;
using Models;

namespace Logic
{
    public class Logic
    {
        public static string GetBusConfiguration()
        {
            return "host=localhost;username=guest;password=guest;timeout=60;prefetchcount=1";
        }

        public static Func<Node, Task> GetLogic(Func<string, Task> onNavigate, Func<string, Task<string>> onEvaluate)
        {
            return async message =>
            {
                Console.WriteLine($"Start {message.Url} {GetCurrentTime()}");

                if (!string.IsNullOrEmpty(message.Url))
                {
                    await onNavigate.Invoke(message.Url);
                }

                if (!string.IsNullOrEmpty(message.Script))
                {
                    Console.WriteLine(await onEvaluate.Invoke(message.Script));
                }

                Console.WriteLine($"End {message.Url} {GetCurrentTime()}");
            };
        }

        private static string GetCurrentTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }
    }
}
