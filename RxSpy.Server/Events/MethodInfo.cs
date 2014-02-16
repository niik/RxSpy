using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Utils;
using bcl = System.Reflection;

namespace RxSpy.Events
{
    public class MethodInfo
    {
        public string Namespace { get; private set; }
        public string DeclaringType { get; private set; }
        public string Name { get; private set; }
        public string Signature { get; private set; }

        public MethodInfo(bcl.MethodBase method)
        {
            Namespace = method.Name;
            DeclaringType = method.DeclaringType.Name;
            Name = GetName(method);
            Signature = Name + " (" + GetArguments(method) + ")";
        }

        string GetName(bcl.MethodBase method)
        {
            if (method.IsGenericMethod)
            {
                var genericArgs = method.GetGenericArguments();
                return method.Name + "<" + string.Join(", ", genericArgs.Select(TypeUtils.ToFriendlyName)) + ">";
            }

            return method.Name;
        }

        string GetArguments(bcl.MethodBase method)
        {
            var arguments = new List<string>();

            foreach (var arg in method.GetParameters())
            {
                arguments.Add(GetArgument(arg));
            }

            return string.Join(", ", arguments);
        }

        string GetArgument(bcl.ParameterInfo arg)
        {
            if (arg.ParameterType.IsGenericParameter)
            {
            }

            return TypeUtils.ToFriendlyName(arg.ParameterType) + " " + arg.Name;
        }
    }
}
