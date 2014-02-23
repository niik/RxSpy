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
                int lastCharacterPosition = 0;

                var subs = new List<Tuple<int, Func<object, string>>>();
                var parts = new List<string>();

                var matches = DebuggerDisplayPropertyRe.Matches(format);
                
                foreach(Match m in matches)
                {
                    if (lastCharacterPosition != m.Index)
                    {
                        parts.Add(format.Substring(lastCharacterPosition, m.Index - lastCharacterPosition));
                        lastCharacterPosition = m.Index + m.Length;
                    }

                    var propertyName = m.Groups[1].Value;

                    var sub = CreatePropertyValueDelegate(type, propertyName, !m.Groups[2].Success);
                    subs.Add(Tuple.Create(parts.Count, sub));
                    parts.Add(null);
                }

                if (lastCharacterPosition != format.Length)
                    parts.Add(format.Substring(lastCharacterPosition));

                return o =>
                {
                    var combine = parts.ToArray();

                    foreach (var sub in subs)
                        combine[sub.Item1] = sub.Item2(o);

                    return string.Concat(combine);
                };
            }
            catch (Exception exc)
            {
                return new Func<object, string>(_ => "Could not create debugger display formatter " + exc.Message);
            }
        }

        static Func<object, string> CreatePropertyValueDelegate(Type type, string propertyName, bool quote)
        {
            var propertyInfo = type.GetProperty(propertyName);

            if (propertyInfo != null)
            {
                return o => Convert.ToString(propertyInfo.GetValue(o) ?? "null");
            }

            var fieldInfo = type.GetField(propertyName);

            if (fieldInfo != null)
            {
                return o => Convert.ToString(fieldInfo.GetValue(o) ?? "null");
            }

            return o => "No such property or field " + propertyName;
        }
    }
}
