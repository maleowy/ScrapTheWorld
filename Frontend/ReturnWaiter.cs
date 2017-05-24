using System;
using System.Threading;

namespace Frontend
{
    public class ReturnWaiter
    {
        private ManualResetEvent _signal = new ManualResetEvent(false);
        public ManualResetEvent Signal { get { return _signal; } }
        public Guid Key { get; private set; }

        public ReturnWaiter(Guid key)
        {
            Key = key;
        }

        private object _value;
        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                Signal.Set();
            }
        }
    }
}