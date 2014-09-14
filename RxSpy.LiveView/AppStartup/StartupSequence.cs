using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;
using RxSpy.Communication;
using RxSpy.Events;
using RxSpy.Models;
using RxSpy.Models.Events;
using RxSpy.ViewModels;
using RxSpy.Views;
using RxSpy.Views.Controls;
using Splat;

namespace RxSpy.AppStartup
{
    public static class StartupSequence
    {
        public static void Start()
        {
            var args = Environment.GetCommandLineArgs();

            var address = args.Length > 1
                ? new Uri(args[1])
                : new Uri("http://localhost:65073/rxspy/");

            var client = new RxSpyHttpClient();

            var session = new RxSpySessionModel();

            client.Connect(address, TimeSpan.FromSeconds(5))
                .Where(x => x != null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(session.OnEvent, ex => { MessageBox.Show(App.Current.MainWindow, "Lost connection to host", "Host disconnected", MessageBoxButton.OK, MessageBoxImage.Error); });

            //session.OnEvent(new OperatorCreatedEvent
            //{
            //    EventType = EventType.OperatorCreated,
            //    Name = "Dummy1",
            //    Id = 0,
            //});

            //session.OnEvent(new OperatorCreatedEvent
            //{
            //    EventType = EventType.OperatorCreated,
            //    Name = "Dummy2",
            //    Id = 1,
            //});

            //session.OnEvent(new OperatorCreatedEvent
            //{
            //    EventType = EventType.OperatorCreated,
            //    Name = "Dummy3",
            //    Id = 2
            //});

            //session.OnEvent(new SubscribeEvent { EventType = EventType.Subscribe, ChildId = 2, ParentId = 1 });
            //session.OnEvent(new SubscribeEvent { EventType = EventType.Subscribe, ChildId = 1, ParentId = 0 });

            var mainViewModel = new MainViewModel(session);

            Locator.CurrentMutable.RegisterConstant(mainViewModel, typeof(MainViewModel));

            Locator.CurrentMutable.Register(() => new MainView(), typeof(IViewFor<MainViewModel>));
            Locator.CurrentMutable.Register(() => new TrackedObservablesGrid(), typeof(IViewFor<RxSpyObservablesGridViewModel>));
            Locator.CurrentMutable.Register(() => new ObservableDetails(), typeof(IViewFor<RxSpyObservableDetailsViewModel>));
        }
    }
}
