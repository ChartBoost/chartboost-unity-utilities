using System.Threading;

namespace Chartboost.Testing
{
    /// <summary>
    /// Records one two-argument callback (sender, value), and the thread it arrived on. Subscribe its
    /// <see cref="Invoke"/>, wait for <see cref="Fired"/>, then assert on what it recorded.
    /// </summary>
    public sealed class CallbackRecorder<TSender, TValue>
    {
        public bool Fired { get; private set; }
        public TSender Sender { get; private set; }
        public TValue Value { get; private set; }
        public int ThreadId { get; private set; }

        public void Invoke(TSender sender, TValue value)
        {
            Sender = sender;
            Value = value;
            ThreadId = Thread.CurrentThread.ManagedThreadId;
            Fired = true;
        }
    }
}
