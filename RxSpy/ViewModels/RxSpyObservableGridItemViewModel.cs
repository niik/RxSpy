using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using RxSpy.Events;
using RxSpy.Models;

namespace RxSpy.ViewModels
{
    public class RxSpyObservableGridItemViewModel : ReactiveObject
    {
        RxSpyObservableModel _model { get; set; }

        readonly ObservableAsPropertyHelper<string> _name;
        public string Name
        {
            get { return _name.Value; }
        }

        readonly ObservableAsPropertyHelper<long> _valuesProduced;
        public long ValuesProduced
        {
            get { return _valuesProduced.Value; }
        }

        readonly ObservableAsPropertyHelper<int> _activeSubscriptions;
        public int ActiveSubscriptions
        {
            get { return _activeSubscriptions.Value; }
        }

        readonly ObservableAsPropertyHelper<int> _totalSubscriptions;
        public int TotalSubscriptions
        {
            get { return _totalSubscriptions.Value; }
        }

        readonly ObservableAsPropertyHelper<string> _callSite;
        public string CallSite
        {
            get { return _callSite.Value; }
        }

        public RxSpyObservableGridItemViewModel(RxSpyObservableModel model)
        {
            _model = model;

            this.WhenAnyValue(x => x._model.Name)
                .ToProperty(this, x => x.Name, out _name);

            this.WhenAnyValue(x => x._model.ValuesProduced)
                .ToProperty(this, x => x.ValuesProduced, out _valuesProduced);

            this.WhenAnyValue(x => x._model.Subscriptions.Count)
                .ToProperty(this, x => x.TotalSubscriptions, out _totalSubscriptions);

            this.WhenAnyValue(x => x._model.ActiveSubscriptions.Count)
                .ToProperty(this, x => x.ActiveSubscriptions, out _activeSubscriptions);

            this.WhenAnyValue(x => x._model.CallSite, x => GetCallSiteString(x))
                .ToProperty(this, x => x.CallSite, out _callSite);
        }

        private string GetCallSiteString(ICallSite callSite)
        {
            if (callSite == null || callSite.Method.Name == null)
                return "";

            string typeAndMethod = callSite.Method.DeclaringType + "." + callSite.Method.Signature;

            if (callSite.File != null && callSite.Line != -1)
                return typeAndMethod + " in " + Path.GetFileName(callSite.File) + ":" + callSite.Line;

            return typeAndMethod;
        }
    }
}
