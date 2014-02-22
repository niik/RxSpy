using System;
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
        public static string ToString(object value)
        {
            if (value == null)
                return "<null>";

            return ToString(value, value.GetType());
        }

        public static string ToString(object value, Type valueType)
        {
            if (value == null)
                return "<null>";

            var s = value as string;

            if (s != null)
                return '"' + s + '"';

            if (valueType.IsArray)
            {
                if (valueType.GetArrayRank() == 1)
                {
                    var arr = (Array)value;

                    if (arr.Length < 100)
                    {
                        var elements = new string[arr.Length];

                        for (int i = 0; i < arr.Length; i++)
                            elements[i] = ToString(arr.GetValue(i));

                        return TypeUtils.ToFriendlyName(valueType) + " {" + string.Join(", ", elements) + "}";
                    }
                }
            }

            var list = value as System.Collections.IList;

            if (list != null && list.Count < 100)
            {
                return TypeUtils.ToFriendlyName(valueType) + " {" + string.Join(", ", list.Cast<object>().Select(ToString)) + "}";
            }

            string debuggerDisplayValue;

            if (DebuggerDisplayFormatter.TryFormat(valueType, value, out debuggerDisplayValue))
                return debuggerDisplayValue;

            return Convert.ToString(value);
        }
    }
}
