using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Communication
{
    internal interface IRxSpyServer: IDisposable
    {
        Uri Address { get; }

        void WaitForConnection(TimeSpan timeout);
        void EnqueueEvent(IEvent ev);
    }
}
