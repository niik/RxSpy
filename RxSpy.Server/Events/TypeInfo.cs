using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxSpy.Events
{
    public class TypeInfo: ITypeInfo
    {
        public string Name { get; private set; }
        public string Namespace { get; private set; }

        public TypeInfo(Type type)
        {
            Name = type.Name;
            Namespace = type.Namespace;
        }
    }
}
