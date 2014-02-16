using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Models.Events
{
    public class TypeInfo: ITypeInfo
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
    }
}
