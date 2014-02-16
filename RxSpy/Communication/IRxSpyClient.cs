using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Communication
{
    public interface IRxSpyClient
    {
        Task Connect(Uri address, TimeSpan timeout);

        IObservable<IEvent> Events { get; }
    }
}
