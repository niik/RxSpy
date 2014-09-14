using System;
using System.Threading;
using freakcode.frequency;

namespace RxSpy.Events
{
    internal abstract class Event : IEvent
    {
        static long counter = 0;

        readonly EventType _type;
        readonly long _id;
        readonly long _ts;

        public EventType EventType { get { return _type; } }
        public long EventId { get { return _id; } }
        public long EventTime { get { return _ts; } }

        Event(long id, long ts)
        {
            _id = id;
            _ts = ts;
        }

        protected Event(EventType type)
            : this(Interlocked.Increment(ref counter), Monotonic.Time())
        {
            _type = type;
        }

        public static OperatorCreatedEvent OperatorCreated(OperatorInfo operatorInfo)
        {
            return new OperatorCreatedEvent(operatorInfo);
        }

        public static OnNextEvent OnNext(OperatorInfo operatorInfo, Type type, object value)
        {
            return new OnNextEvent(operatorInfo, type, value, Thread.CurrentThread.ManagedThreadId);
        }

        public static OnErrorEvent OnError(OperatorInfo operatorInfo, Exception error)
        {
            return new OnErrorEvent(operatorInfo, error);
        }

        public static OnCompletedEvent OnCompleted(OperatorInfo operatorInfo)
        {
            return new OnCompletedEvent(operatorInfo);
        }

        internal static SubscribeEvent Subscribe(OperatorInfo child, OperatorInfo parent)
        {
            return new SubscribeEvent(child, parent);
        }

        internal static UnsubscribeEvent Unsubscribe(long subscriptionId)
        {
            return new UnsubscribeEvent(subscriptionId);
        }

        internal static TagOperatorEvent Tag(OperatorInfo operatorInfo, string tag)
        {
            return new TagOperatorEvent(operatorInfo, tag);
        }

        internal static ConnectedEvent Connect(OperatorInfo operatorInfo)
        {
            return new ConnectedEvent(operatorInfo);
        }

        internal static DisconnectedEvent Disconnect(long connectionId)
        {
            return new DisconnectedEvent(connectionId);
        }
    }
}
