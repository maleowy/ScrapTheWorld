using System;
using System.Collections.Concurrent;

namespace Frontend
{
    public static class MethodHandler
    {
        private static ConcurrentDictionary<Guid, ReturnWaiter> _runningMethodWaiters = new ConcurrentDictionary<Guid, ReturnWaiter>();

        public static TResult GetValue<TResult>(Guid key, Action action)
        {
            var returnWaiter = new ReturnWaiter(key);
            _runningMethodWaiters.TryAdd(key, returnWaiter);
            action.Invoke();
            returnWaiter.Signal.WaitOne(TimeSpan.FromSeconds(30));

            return (TResult)returnWaiter.Value;
        }

        public static void GetValueResult(Guid key, object value)
        {
            ReturnWaiter waiter;

            if (_runningMethodWaiters.TryRemove(key, out waiter))
            {
                waiter.Value = value;
            }
        }
    }
}