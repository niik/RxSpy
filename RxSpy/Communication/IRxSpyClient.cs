using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using RxSpy.Events;

namespace RxSpy.Communication
{
    public interface IRxSpyClient: IReactiveNotifyPropertyChanged
    {
        IObservable<IEvent> Connect(Uri address, TimeSpan timeout);
    }
}
