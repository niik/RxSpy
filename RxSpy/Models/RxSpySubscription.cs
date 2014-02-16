using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace RxSpy.Models
{
    public class RxSpySubscription: ReactiveObject
    {
        RxSpyObservableModel _parent;
        public RxSpyObservableModel Parent
        {
            get { return _parent; }
            set { this.RaiseAndSetIfChanged(ref _parent, value); }
        }

        RxSpyObservableModel _child;
        public RxSpyObservableModel Child
        {
            get { return _child; }
            set { this.RaiseAndSetIfChanged(ref _child, value); }
        }

        bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set { this.RaiseAndSetIfChanged(ref _isActive, value); }
        }


    }
}
