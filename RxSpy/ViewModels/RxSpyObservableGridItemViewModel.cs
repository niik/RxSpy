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
        public RxSpyObservableModel Model { get; private set; }

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

        readonly ObservableAsPropertyHelper<int> _parents;
        public int Parents
        {
            get { return _parents.Value; }
        }

        readonly ObservableAsPropertyHelper<int> _children;
        public int Children
        {
            get { return _children.Value; }
        }

        readonly ObservableAsPropertyHelper<int> _ancestors;
        public int Ancestors
        {
            get { return _ancestors.Value; }
        }

        readonly ObservableAsPropertyHelper<int> _descendants;
        public int Descendants
        {
            get { return _descendants.Value; }
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
            Model = model;

            this.WhenAnyValue(x => x.Model.Name)
                .ToProperty(this, x => x.Name, out _name);

            this.WhenAnyValue(x => x.Model.ValuesProduced)
                .ToProperty(this, x => x.ValuesProduced, out _valuesProduced);

            this.WhenAnyValue(x => x.Model.Children.Count)
                .ToProperty(this, x => x.TotalSubscriptions, out _totalSubscriptions);

            this.WhenAnyValue(x => x.Model.Parents.Count)
                .ToProperty(this, x => x.Parents, out _parents);

            this.WhenAnyValue(x => x.Model.Children.Count)
                .ToProperty(this, x => x.Children, out _children);

            this.WhenAnyValue(x => x.Model.Ancestors)
                .ToProperty(this, x => x.Ancestors, out _ancestors);

            this.WhenAnyValue(x => x.Model.Descendants)
                .ToProperty(this, x => x.Descendants, out _descendants);

            this.WhenAnyValue(x => x.Model.CallSite, x => GetCallSiteString(x))
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
