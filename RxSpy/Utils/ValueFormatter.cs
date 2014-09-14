using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RxSpy.Utils
{
    public static class ValueFormatter
    {
        readonly static ConcurrentDictionary<Type, Lazy<Func<object, string>>> _cachedFormatters =
            new ConcurrentDictionary<Type, Lazy<Func<object, string>>>();

        public static string ToString(object value)
        {
            if (value == null)
                return "<null>";

            return ToString(value, value.GetType());
        }

        public static string ToString(object value, Type type)
        {
            var formatter = _cachedFormatters.GetOrAdd(type, CreateFormatter);

            return formatter.Value(value);
        }

        private static Lazy<Func<object, string>> CreateFormatter(Type type)
        {
            return new Lazy<Func<object, string>>(() => BuildFormatterDelegate(type));
        }

        private static Func<object, string> BuildFormatterDelegate(Type type)
        {
            if (type == typeof(string))
            {
                return o => o == null ? "null" : ('"' + (string)o + '"');
            }

            if (type.IsArray)
            {
                if (type.GetArrayRank() == 1)
                {
                    string typeName = TypeUtils.ToFriendlyName(type);

                    return o =>
                    {
                        if (o == null)
                            return typeName;

                        var arr = (Array)o;

                        if (arr.Length < 10)
                        {
                            var elements = new string[arr.Length];

                            for (int i = 0; i < arr.Length; i++)
                                elements[i] = ToString(arr.GetValue(i));

                            return typeName + " {" + string.Join(", ", elements) + "}";
                        }
                        else
                        {
                            return typeName + "[" + arr.Length + "]";
                        }
                    };
                }
            }

            if (typeof(System.Collections.IList).IsAssignableFrom(type))
            {
                string typeName = TypeUtils.ToFriendlyName(type);
                return o =>
                {
                    if (o == null)
                        return typeName;

                    var list = o as System.Collections.IList;

                    if (list != null && list.Count < 15)
                    {
                        return typeName + " {" + string.Join(", ", list.Cast<object>().Select(ToString)) + "}";
                    }
                    else
                    {
                        return typeName + "[" + list.Count + "]";
                    }
                };
            }

            Func<object, string> debuggerDisplayFormatter;

            if (DebuggerDisplayFormatter.TryGetDebuggerDisplayFormatter(type, out debuggerDisplayFormatter))
            {
                return debuggerDisplayFormatter;
            }

            return o => Convert.ToString(o);
        }
    }
}
