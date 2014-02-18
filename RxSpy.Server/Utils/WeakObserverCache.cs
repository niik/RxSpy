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

        public bool TryGetOrAdd(object value, out OperatorInfo operatorInfo)
        {
            // TODO: RWLock
            lock (cache)
            {
                var node = cache.First;

                if (node == null)
                {
                    operatorInfo = Add(value);
                    return false;
                }

                var stale = new List<LinkedListNode<Tuple<WeakReference<object>, OperatorInfo>>>();

                while (node != null)
                {
                    object obs;

                    if (node.Value.Item1.TryGetTarget(out obs))
                    {
                        if (object.ReferenceEquals(obs, value))
                        {
                            operatorInfo = node.Value.Item2;
                            return true;
                        }
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

                operatorInfo = Add(value);
                return false;
            }
        }

        OperatorInfo Add(object value)
        {
            var operatorInfo = CreateAnonymousOperatorInfo(value);
            cache.AddFirst(Tuple.Create(new WeakReference<object>(value), operatorInfo));

            return operatorInfo;
        }

        private OperatorInfo CreateAnonymousOperatorInfo(object value)
        {
            return new OperatorInfo(TypeUtils.ToFriendlyName(value.GetType()));
        }
    }
}
