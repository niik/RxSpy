using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using RxSpy.Models;

namespace RxSpy.ViewModels.Graphs
{
    public class ObservableVertex : ReactiveObject, IEquatable<ObservableVertex>
    {
        public long Id { get; set; }

        readonly ObservableAsPropertyHelper<string> _name;
        public string Name
        {
            get { return _name.Value; }
        }

        readonly ObservableAsPropertyHelper<bool> _hasError;
        public bool HasError
        {
            get { return _hasError.Value; }
        }

        readonly ObservableAsPropertyHelper<bool> _isActive;
        public bool IsActive
        {
            get { return _isActive.Value; }
        }

        readonly ObservableAsPropertyHelper<long> _signalsCount;
        public long SignalsCount
        {
            get { return _signalsCount.Value; }
        }

        readonly ObservableAsPropertyHelper<string> _tooltip;
        public string ToolTip
        {
            get { return _tooltip.Value; }
        }

        public ObservableVertex(RxSpyObservableModel model)
        {
            Id = model.Id;
            Model = model;

            this.WhenAnyValue(x => x.Model.Name)
                .ToProperty(this, x => x.Name, out _name, scheduler: Scheduler.Immediate);

            this.WhenAnyValue(x => x.Model.HasError)
                .ToProperty(this, x => x.HasError, out _hasError);

            this.WhenAnyValue(x => x.Model.IsActive)
                .ToProperty(this, x => x.IsActive, out _isActive);

            this.WhenAnyValue(x => x.Model.ValuesProduced)
                .ToProperty(this, x => x.SignalsCount, out _signalsCount);

            this.WhenAnyValue(x => x.Model.Id, x => x.Model.CallSite, (x, y) => { return "#" + Id + " " + Convert.ToString(y); })
                .ToProperty(this, x => x.ToolTip, out _tooltip);
        }

        public bool Equals(ObservableVertex other)
        {
            if (other == null) return false;

            return other.Id == Id;
        }

        public RxSpyObservableModel Model { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
