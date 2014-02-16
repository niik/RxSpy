using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxSpy.Utils
{
    internal class WeakObserverCache
    {
        readonly LinkedList<Tuple<WeakReference<object>, OperatorInfo>> cache =
            new LinkedList<Tuple<WeakReference<object>, OperatorInfo>>();

        public bool TryGetOrAdd<T>(IObserver<T> observer, out OperatorInfo operatorInfo)
        {
            // TODO: RWLock
            lock (cache)
            {
                var node = cache.First;

                if (node == null)
                {
                    operatorInfo = Add<T>(observer);
                    return false;
                }

                var stale = new List<LinkedListNode<Tuple<WeakReference<object>, OperatorInfo>>>();

                while (node != null)
                {
                    object obs;

                    if (node.Value.Item1.TryGetTarget(out obs))
                    {
                        operatorInfo = node.Value.Item2;
                        return true;
                    }
                    else
                    {
                        stale.Add(node);
                    }

                    node = node.Next;
                }

                foreach (var staleNode in stale)
                {
                    cache.Remove(staleNode);
                }

                operatorInfo = Add(observer);
                return false;
            }
        }

        OperatorInfo Add<T>(IObserver<T> observer)
        {
            var operatorInfo = CreateAnonymousOperatorInfo(observer);
            cache.AddFirst(Tuple.Create(new WeakReference<object>(observer), operatorInfo));

            return operatorInfo;
        }

        private OperatorInfo CreateAnonymousOperatorInfo<T>(IObserver<T> observer)
        {
            return new OperatorInfo(TypeUtils.ToFriendlyName(observer.GetType()));
        }
    }
}
