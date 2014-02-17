using System.Reactive.Linq;
using System.Text;
using ReactiveUI;
using RxSpy.Models;

namespace RxSpy.ViewModels
{
    public class RxSpyObservableDetailsViewModel : ReactiveObject
    {
        private RxSpyObservableModel _model;

        readonly ObservableAsPropertyHelper<IReadOnlyReactiveList<RxSpyObservedValueViewModel>> _observedValues;
        public IReadOnlyReactiveList<RxSpyObservedValueViewModel> ObservedValues
        {
            get { return _observedValues.Value; }
        }

        readonly ObservableAsPropertyHelper<bool> _valuesGridIsEnabled;
        public bool ValuesGridIsEnabled
        {
            get { return _valuesGridIsEnabled.Value; }
        }

        readonly ObservableAsPropertyHelper<RxSpyObservablesGridViewModel> _parents;
        public RxSpyObservablesGridViewModel Parents
        {
            get { return _parents.Value; }
        }

        readonly ObservableAsPropertyHelper<RxSpyObservablesGridViewModel> _children;
        public RxSpyObservablesGridViewModel Children
        {
            get { return _children.Value; }
        }

        readonly ObservableAsPropertyHelper<bool> _showErrorTab;
        public bool ShowErrorTab
        {
            get { return _showErrorTab.Value; }
        }

        readonly ObservableAsPropertyHelper<string> _errorText;
        public string ErrorText
        {
            get { return _errorText.Value; }
        }

        public RxSpyObservableDetailsViewModel(RxSpyObservableModel model)
        {
            _model = model;

            this.WhenAnyValue(x => x._model.ObservedValues)
                .Select(x => x.CreateDerivedCollection(m => new RxSpyObservedValueViewModel(m)))
                .Scan((prev, cur) => { using (prev) return cur; })
                .ToProperty(this, x => x.ObservedValues, out _observedValues);

            this.WhenAnyValue(x => x._model.Parents, x => new RxSpyObservablesGridViewModel(x))
                .ToProperty(this, x => x.Parents, out _parents);

            this.WhenAnyValue(x => x._model.Children, x => new RxSpyObservablesGridViewModel(x))
                .ToProperty(this, x => x.Children, out _children);

            this.WhenAnyValue(x => x._model.HasError)
                .ToProperty(this, x => x.ShowErrorTab, out _showErrorTab);

            this.WhenAnyValue(x => x._model.Error, FormatErrorText)
                .ToProperty(this, x => x.ErrorText, out _errorText);
        }

        string FormatErrorText(RxSpyErrorModel err)
        {
            if (err == null)
                return "";

            var sb = new StringBuilder();

            sb.AppendLine("Received: " + err.Received);
            sb.AppendLine(err.ErrorType.Namespace + err.ErrorType.Name);
            sb.AppendLine();
            sb.AppendLine(err.Message);

            if (!string.IsNullOrEmpty(err.StackTrace))
            {
                sb.AppendLine();
                sb.AppendLine("Stacktrace: ");
                sb.AppendLine(err.StackTrace);
            }

            return sb.ToString();
        }
    }
}