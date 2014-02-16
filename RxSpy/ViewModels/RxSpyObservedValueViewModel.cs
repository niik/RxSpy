using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using RxSpy.Models;

namespace RxSpy.ViewModels
{
    public class RxSpyObservedValueViewModel: ReactiveObject
    {
        RxSpyObservedValueModel _model;

        readonly ObservableAsPropertyHelper<TimeSpan> _received;
        public TimeSpan Received
        {
            get { return _received.Value; }
        }

        readonly ObservableAsPropertyHelper<string> _value;
        public string Value
        {
            get { return _value.Value; }
        }

        readonly ObservableAsPropertyHelper<string> _valueType;
        public string ValueType
        {
            get { return _valueType.Value; }
        }

        public RxSpyObservedValueViewModel(RxSpyObservedValueModel model)
        {
            _model = model;

            this.WhenAnyValue(x => x._model.Received)
                .ToProperty(this, x => x.Received, out _received);
            
            this.WhenAnyValue(x => x._model.Value)
                .ToProperty(this, x => x.Value, out _value);
            
            this.WhenAnyValue(x => x._model.ValueType)
                .ToProperty(this, x => x.ValueType, out _valueType);
        }
    }
}