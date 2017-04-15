using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CefSharp.Internals;

namespace Extensions
{
    public class AsyncAutoResetEvent
    {
        private static readonly Task Completed = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> _waits = new Queue<TaskCompletionSource<bool>>();
        private bool _signaled;

        public Task WaitAsync(int seconds = 0)
        {
            lock (_waits)
            {
                if (_signaled)
                {
                    _signaled = false;
                    return Completed;
                }

                var tcs = new TaskCompletionSource<bool>();

                if (seconds > 0)
                {
                    tcs.WithTimeout(TimeSpan.FromSeconds(seconds));
                }

                _waits.Enqueue(tcs);
                return tcs.Task;
            }
        }

        public void Set()
        {
            TaskCompletionSource<bool> toRelease = null;

            lock (_waits)
            {
                if (_waits.Count > 0)
                {
                    toRelease = _waits.Dequeue();
                }
                else if (!_signaled)
                {
                    _signaled = true;
                }
            }

            try
            {
                toRelease?.SetResult(true);
            }
            catch {}
        }
    }
}
