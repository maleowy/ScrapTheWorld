using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.SystemMessages;
using EasyNetQ.Topology;

namespace Tests.EasyNetQ
{
    class Program
    {
        public static IBus Bus;
        public static int WaitTime { get; } = 3000;
        public const string ErrorQueue = "EasyNetQ_Default_Error_Queue";

        static void Main(string[] args)
        {
            Bus = RabbitHutch.CreateBus("host=localhost;timeout=5;prefetchcount=1");

            RpcCall(sleepToLong: false);

            SubscribeAsync(randomSleep: false, randomThrow: false, showEnd: false);

            HandleErrors();

            for (int i = 1; i <= 5; i++)
            {
                Bus.Publish(new Question($"Question {i}"));
            }

            Console.ReadLine();
        }

        private static void RpcCall(bool sleepToLong = false)
        {
            Bus.Respond<Question, Answer>(q => 
            {
                if (sleepToLong)
                {
                    Thread.Sleep(6000);
                }
                return new Answer("Answer to " + q.Text);
            });

            var request = new Question("Question");
            var response = Bus.Request<Question, Answer>(request);

            Console.WriteLine(response.Text);
        }

        private static void Subscribe(bool randomSleep, bool randomThrow, bool showEnd)
        {
            Bus.Subscribe<Question>("subscriptionId", x => GetAction(randomSleep, randomThrow, showEnd).Invoke(1, x));
            Bus.Subscribe<Question>("subscriptionId", x => GetAction(randomSleep, randomThrow, showEnd).Invoke(2, x));
        }

        private static void SubscribeAsync(bool randomSleep, bool randomThrow, bool showEnd)
        {
            Bus.SubscribeAsync<Question>("subscriptionId", x => GetFunctionAsync(randomSleep, randomThrow, showEnd).Invoke(1, x));
            Bus.SubscribeAsync<Question>("subscriptionId", x => GetFunctionAsync(randomSleep, randomThrow, showEnd).Invoke(2, x));
        }

        private static Action<int, Question> GetAction(bool randomSleep, bool randomThrow, bool showEnd)
        {
            return (id, q) =>
            {
                ShowStart(id, q);
                Wait(randomSleep);
                RandomThrow(id, q, randomThrow);
                ShowEnd(id, q, showEnd);
            };
        }

        private static Action<int, Question> GetActionAsync(bool randomSleep, bool randomThrow, bool showEnd)
        {
            return async (id, q) =>
            {
                ShowStart(id, q);
                await WaitAsync(randomSleep);
                RandomThrow(id, q, randomThrow);
                ShowEnd(id, q, showEnd);
            };
        }

        private static Func<int, Question, Task> GetFunction(bool randomSleep, bool randomThrow, bool showEnd)
        {
            return (id, q) =>
            {
                ShowStart(id, q);
                Wait(randomSleep);
                RandomThrow(id, q, randomThrow);
                ShowEnd(id, q, showEnd);
                return Task.FromResult(true);
            };
        }

        private static Func<int, Question, Task> GetFunctionAsync(bool randomSleep, bool randomThrow, bool showEnd)
        {
            return async (id, q) =>
            {
                ShowStart(id, q);
                await WaitAsync(randomSleep);
                RandomThrow(id, q, randomThrow);
                ShowEnd(id, q, showEnd);
            };
        }

        private static void ShowStart(int id, Question q)
        {
            Console.WriteLine($"Start Worker {id} - {q.Text} {GetCurrentTime()}");
        }

        private static string GetCurrentTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        private static void Wait(bool random)
        {
            Thread.Sleep(random ? new Random().Next(WaitTime) : WaitTime);
        }

        private static async Task WaitAsync(bool random)
        {
            await Task.Delay(random ? new Random().Next(WaitTime) : WaitTime);
        }

        private static void RandomThrow(int id, Question q, bool randomThrow)
        {
            if (randomThrow && new Random().Next(0, 2) == 0)
            {
                var ex = $"throw: Worker {id} - {q.Text}";
                Console.WriteLine(ex);
                throw new Exception(ex);
            }
        }

        private static void ShowEnd(int id, Question q, bool show)
        {
            if (show)
            {
                Console.WriteLine($"End Worker {id} - {q.Text} {GetCurrentTime()}");
            }
        }

        private static void HandleErrors()
        {
            Action<IMessage<Error>, MessageReceivedInfo> handleErrorMessage = HandleErrorMessage;

            IQueue queue = new Queue(ErrorQueue, false);
            Bus.Advanced.Consume(queue, handleErrorMessage);
        }

        private static void HandleErrorMessage(IMessage<Error> msg, MessageReceivedInfo info)
        {
            Console.WriteLine("catch: " + msg.Body.Message);
        }
    }
}
