using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxSpy.Observables
{
    public interface IOperatorObservable
    {
        OperatorInfo OperatorInfo { get; }
    }
}
