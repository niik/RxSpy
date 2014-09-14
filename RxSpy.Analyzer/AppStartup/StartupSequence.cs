using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using RxSpy.Analyzer.ViewModels;
using RxSpy.Analyzer.Views;
using Splat;

namespace RxSpy.Analyzer.AppStartup
{
    public static class StartupSequence
    {
        public static void Start()
        {
            RegisterViewsAndViewModels();
        }

        private static void RegisterViewsAndViewModels()
        {
            var mainViewModel = new MainViewModel();

            Locator.CurrentMutable.RegisterConstant(mainViewModel, typeof(MainViewModel));

            Locator.CurrentMutable.Register(() => new MainView(), typeof(IViewFor<MainViewModel>));
        }
    }
}
