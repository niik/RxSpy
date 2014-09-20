using System;
using System.Windows;
using RxSpy.Analyzer.AppStartup;

namespace RxSpy.Analyzer
{
    public partial class App : Application
    {
        public App()
        {
            StartupSequence.Start();
        }
    }
}
