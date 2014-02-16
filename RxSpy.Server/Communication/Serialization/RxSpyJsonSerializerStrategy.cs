using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxSpy.Communication.Serialization
{
    public class RxSpyJsonSerializerStrategy: PocoJsonSerializerStrategy
    {
        protected override object SerializeEnum(Enum p)
        {
            return Enum.GetName(p.GetType(), p);
        }
    }
}
