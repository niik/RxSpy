using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace RxSpy.Utils
{
    public class DebuggerDisplayFormatter
    {
        readonly static ConcurrentDictionary<Type, Lazy<Func<object, string>>> _cachedFormatters =
            new ConcurrentDictionary<Type, Lazy<Func<object, string>>>();

        static readonly Regex DebuggerDisplayPropertyRe = new Regex(@"\{\s*(\w[\w\d]+)(,nq)?\s*\}");

        public static bool TryFormat(Type type, object target, out string value)
        {
            Func<object, string> formatter;

            if (!TryGetDebuggerDisplayFormatter(type, out formatter))
            {
                value = null;
                return false;
            }

            value = formatter(target);
            return true;
        }

        public static bool TryGetDebuggerDisplayFormatter(Type type, out Func<object, string> formatter)
        {
            var cacheEntry = _cachedFormatters.GetOrAdd(type, CreateFormatter);

            if (cacheEntry == null)
            {
                formatter = null;
                return false;
            }

            formatter = cacheEntry.Value;
            return true;
        }

        static Lazy<Func<object, string>> CreateFormatter(Type type)
        {
            var debuggerDisplayAttributes = type.GetCustomAttributes(typeof(DebuggerDisplayAttribute), false);

            if (debuggerDisplayAttributes == null || debuggerDisplayAttributes.Length == 0)
            {
                return null;
            }

            var attribute = (DebuggerDisplayAttribute)debuggerDisplayAttributes[0];

            return new Lazy<Func<object, string>>(
                () => BuildFormatterDelegate(type, attribute.Value),
                LazyThreadSafetyMode.ExecutionAndPublication
            );
        }

        static Func<object, string> BuildFormatterDelegate(Type type, string format)
        {
            // We only support simple property getters for now, no method invocation

            try
            {
                int count = 0;

                var substitionFuncs = new List<Func<object, object>>();

                string newFormat = DebuggerDisplayPropertyRe.Replace(format, m =>
                {
                    int p = count++;

                    var propertyName = m.Groups[1].Value;
                    //var propertyValue = GetValueForFieldOrProperty(propertyName, value, type);

                    substitionFuncs.Add(CreatePropertyValueDelegate(type, propertyName));
                    if (!m.Groups[2].Success)
                    {
                        return "\"{" + p + "}\"";
                    }
                    else
                    {
                        return "{" + p + "}";
                    }
                });

                return o =>
                {
                    return string.Format(newFormat, substitionFuncs.Select(x => x(o)).ToArray());
                };
            }
            catch (Exception exc)
            {
                return new Func<object, string>(_ => "Could not create debugger display formatter " + exc.Message);
            }
        }

        static Func<object, object> CreatePropertyValueDelegate(Type type, string propertyName)
        {
            var propertyInfo = type.GetProperty(propertyName);

            if (propertyInfo != null)
            {
                return o => propertyInfo.GetValue(o) ?? "null";
            }

            var fieldInfo = type.GetField(propertyName);

            if (fieldInfo != null)
            {
                return o => fieldInfo.GetValue(o) ?? "null";
            }

            return o => "No such property or field " + propertyName;
        }
    }
}
