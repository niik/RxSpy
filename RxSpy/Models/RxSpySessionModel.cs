using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace RxSpy.Models
{
    public class RxSpySessionModel: ReactiveObject
    {
        int _currentlyTrackedObservables;
        public int CurrentlyTrackedObservables
        {
            get { return _currentlyTrackedObservables; }
            set { this.RaiseAndSetIfChanged(ref _currentlyTrackedObservables, value); }
        }

        long _totalObservables;
        public long TotalObservables
        {
            get { return _totalObservables; }
            set { this.RaiseAndSetIfChanged(ref _totalObservables, value); }
        }

    }
}
