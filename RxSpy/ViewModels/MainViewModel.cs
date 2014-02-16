using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace RxSpy.ViewModels
{
    public class MainViewModel: ReactiveObject
    {
        RxSpySessionViewModel _rxSpySession;
        public RxSpySessionViewModel SpySessionViewModel
        {
            get { return _rxSpySession; }
            set { this.RaiseAndSetIfChanged(ref _rxSpySession, value); }
        }

        public MainViewModel(RxSpySessionViewModel sessionViewModel)
        {
            SpySessionViewModel = sessionViewModel;
        }
    }
}
