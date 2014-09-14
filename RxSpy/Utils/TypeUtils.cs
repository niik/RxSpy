using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RxSpy.Utils
{
    public static class TypeUtils
    {
        static Dictionary<Type, string> csFriendlyTypeNames;
        readonly static ConcurrentDictionary<Type, Lazy<string>> _typeNameCache =
            new ConcurrentDictionary<Type, Lazy<string>>();

        static TypeUtils()
        {
            csFriendlyTypeNames = new Dictionary<Type, string>();

            csFriendlyTypeNames.Add(typeof(sbyte), "sbyte");
            csFriendlyTypeNames.Add(typeof(byte), "byte");
            csFriendlyTypeNames.Add(typeof(short), "short");
            csFriendlyTypeNames.Add(typeof(ushort), "ushort");
            csFriendlyTypeNames.Add(typeof(int), "int");
            csFriendlyTypeNames.Add(typeof(uint), "uint");
            csFriendlyTypeNames.Add(typeof(long), "long");
            csFriendlyTypeNames.Add(typeof(ulong), "ulong");
            csFriendlyTypeNames.Add(typeof(float), "float");
            csFriendlyTypeNames.Add(typeof(double), "double");
            csFriendlyTypeNames.Add(typeof(bool), "bool");
            csFriendlyTypeNames.Add(typeof(char), "char");
            csFriendlyTypeNames.Add(typeof(string), "string");
            csFriendlyTypeNames.Add(typeof(object), "object");
            csFriendlyTypeNames.Add(typeof(decimal), "decimal");
        }

        public static string ToFriendlyName(Type type)
        {
            var lazy = _typeNameCache.GetOrAdd(type, _ => new Lazy<string>(
                    () => toFriendlyNameImpl(type),
                    LazyThreadSafetyMode.ExecutionAndPublication));

            return lazy.Value;
        }

        static string toFriendlyNameImpl(Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return ToFriendlyName(type.GetGenericArguments()[0]) + "?";
                }
                else
                {
                    var definition = type.GetGenericTypeDefinition();

                    return GetNameWithoutGenerics(definition) + "<" + string.Join(", ", type.GetGenericArguments().Select(ToFriendlyName)) + ">";
                }
            }

            if (type.IsArray)
            {
                return ToFriendlyName(type.GetElementType()) + Repeat("[]", type.GetArrayRank());
            }

            string name;

            if (csFriendlyTypeNames.TryGetValue(type, out name))
                return name;

            return type.Name;
        }

        private static string GetNameWithoutGenerics(Type definition)
        {
            var n = definition.Name;
            var p = n.IndexOf('`');

            if (p == -1)
                return n;

            return n.Substring(0, p);
        }

        private static string Repeat(string str, int count)
        {
            if (count == 1)
                return str;

            var arr = new string[count];

            for (int i = 0; i < count; i++)
                arr[i] = str;

            return string.Concat(arr);
        }
    }
}
