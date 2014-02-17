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

            var s = value as string;

            if (s != null)
                return '"' + s + '"';

            var valueType = value.GetType();

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

            var debuggerDisplayAttributes = valueType.GetCustomAttributes(typeof(DebuggerDisplayAttribute), false);

            if (debuggerDisplayAttributes != null && debuggerDisplayAttributes.Length > 0)
            {
                return FormatWithDebuggerDisplay(value, (DebuggerDisplayAttribute)debuggerDisplayAttributes[0]);
            }

            return Convert.ToString(value);
        }

        static string FormatWithDebuggerDisplay(object value, DebuggerDisplayAttribute attribute)
        {
            return FormatWithDebuggerDisplay(value, attribute.Value);
        }

        static readonly Regex DebuggerDisplayPropertyRe = new Regex(@"\{\s*(\w[\w\d]+)(,nq)?\s*\}");

        public static string FormatWithDebuggerDisplay(object value, string debuggerDisplayFormat)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            // We only support simple property getters for now, no method invocation

            try
            {
                var type = value.GetType();

                return DebuggerDisplayPropertyRe.Replace(debuggerDisplayFormat, m =>
                {
                    var propertyName = m.Groups[1].Value;
                    var propertyValue = GetValueForFieldOrProperty(propertyName, value, type);

                    if (!m.Groups[2].Success)
                    {
                        return '"' + propertyValue + '"';
                    }

                    return propertyValue;
                });
            }
            catch (Exception exc)
            {
                return "Invocation failed " + exc.Message;
            }
        }

        static string GetValueForFieldOrProperty(string propertyName, object value, Type type)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty);

            if (property != null)
                return Convert.ToString(property.GetValue(value));

            var field = type.GetField(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

            if (field != null)
                return Convert.ToString(field.GetValue(value));

            return "<err>";
        }
    }
}
