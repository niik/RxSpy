using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using RxSpy.Events;
using bcl = System.Reflection;

namespace RxSpy.Utils
{
    public static class CallSiteCache
    {
        readonly static GetStackFrameInfo _stackFrameFast;

        readonly static ConcurrentDictionary<Tuple<IntPtr, int>, CallSite> _cache =
            new ConcurrentDictionary<Tuple<IntPtr, int>, CallSite>();

        delegate Tuple<IntPtr, int> GetStackFrameInfo(int skipFrames);

        static CallSiteCache()
        {
            try
            {
                _stackFrameFast = CreateInternalStackFrameInfoMethod();
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Could not create fast stack info method, things are going to get slooooow. Exception: " + exc);

                // If we end up here it's bad, the .NET framework authors has changed the private implementation that 
                // we rely on (which is entirely within their rights to do).
                if (Debugger.IsAttached)
                    Debugger.Break();
            }
        }

        static GetStackFrameInfo CreateInternalStackFrameInfoMethod()
        {
            var mscorlib = typeof(object).Assembly;

            var sfhType = mscorlib.GetType("System.Diagnostics.StackFrameHelper");
            var sfhCtor = sfhType.GetConstructor(new Type[] { typeof(bool), typeof(Thread) });
            var sfhRgMethodHandle = sfhType.GetField("rgMethodHandle", bcl.BindingFlags.NonPublic | bcl.BindingFlags.Instance);
            var sfhRgiILOffsetField = sfhType.GetField("rgiILOffset", bcl.BindingFlags.NonPublic | bcl.BindingFlags.Instance);

            var sfhGetMethodBaseMethod = sfhType.GetMethod("GetMethodBase",
                bcl.BindingFlags.Instance | bcl.BindingFlags.Public);

            var getStackFramesInternalMethod = typeof(StackTrace).GetMethod("GetStackFramesInternal",
                bcl.BindingFlags.Static | bcl.BindingFlags.NonPublic);

            var calculateFramesToSkipMethod = typeof(StackTrace).GetMethod("CalculateFramesToSkip",
                bcl.BindingFlags.Static | bcl.BindingFlags.NonPublic);

            var currentThreadProperty = typeof(Thread).GetProperty("CurrentThread");
            var tupleCtor = typeof(Tuple<IntPtr, int>).GetConstructor(new Type[] { typeof(IntPtr), typeof(int) });

            var skipParam = Expression.Parameter(typeof(int), "iSkip");
            var sfhVariable = Expression.Variable(sfhType, "sfh");
            var actualSkip = Expression.Variable(typeof(int), "iNumFrames");

            var zero = Expression.Constant(0, typeof(int));

            var methodLambda = Expression.Lambda<GetStackFrameInfo>(
                Expression.Block(
                    new[] { sfhVariable },

                    Expression.Assign(
                        sfhVariable,
                        Expression.New(sfhCtor,
                            Expression.Constant(false, typeof(bool)), // fNeedFileLineColInfo
                            Expression.Constant(null, typeof(Thread)) // target (thread)
                        )
                    ),

                    Expression.Call(getStackFramesInternalMethod, sfhVariable, zero, Expression.Constant(null, typeof(Exception))),

                    Expression.New(tupleCtor,
                        Expression.ArrayIndex(Expression.Field(sfhVariable, sfhRgMethodHandle), skipParam),
                        Expression.ArrayIndex(Expression.Field(sfhVariable, sfhRgiILOffsetField), skipParam)
                    )
                ),
                skipParam
            ).Compile();

            return methodLambda;
        }

        public static CallSite Get(int skipFrames)
        {
            // Account for ourselves
            skipFrames++;

            if (_stackFrameFast == null)
            {
                // This is terribad, this session is going to be soooo sloooooooooooow.
                // This will eventually happen when the .NET framework authors
                // excercise their right to change the private implementation we're
                // depending on.
                // Fall back to expensive full frame

                return new CallSite(new StackFrame(skipFrames, true));
            }
            
            // Don't exactly know why we need to skip 2 and not 1 here. 
            // I suspect expression tree trickery.
            var key = _stackFrameFast(skipFrames + 2);

            CallSite cached;

            if (_cache.TryGetValue(key, out cached))
                return cached;

            var frame = new StackFrame(skipFrames, true);

            var callSite = new CallSite(frame);

            return _cache.GetOrAdd(key, callSite);
        }
    }
}
