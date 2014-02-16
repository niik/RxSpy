/*
 * Copyright (c) 2008 Markus Olsson 
 * var mail = string.Join(".", new string[] {"j", "markus", "olsson"}) + string.Concat('@', "gmail.com");
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this 
 * software and associated documentation files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify, merge, publish, 
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING 
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ConstrainedExecution;
using System.Threading;

namespace freakcode.frequency
{
    /// <summary>
    /// Provides a consistently increasing values for time.
    /// </summary>
    /// <remarks>All methods of this class is guaranteed to be thread safe</remarks>
    internal static class Monotonic
    {
        /// <summary>
        /// The most recently stored timestamp
        /// </summary>
        private static long CurrentTimeStamp;

        /// <summary>
        /// The environment tickcount at first initialization
        /// </summary>
        private static readonly uint InitialTickCount;

        /// <summary>
        /// Generation upgrade guarantee timer
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "The reference must be saved in order to prevent the timer from being garbage collected")]
        private static readonly Timer updateTimer;

        /// <summary>
        /// Gets the number of elapsed milliseconds since first initialized.
        /// </summary>
        /// <remarks>This method is thread safe</remarks>
        public static long Now
        {
            get { return Time(); }
        }

        /// <summary>
        /// Initializes the <see cref="MonotonicTime"/> class.
        /// </summary>
        static Monotonic()
        {
            Monotonic.InitialTickCount = GetUnsignedEnvironmentTickCount();

            // Using a timer to ensure that we don't miss an entire cycle
            // of environment.TickCount (~49 days) and thus return the 
            // same value twice.

            // Run the timer once every 24 hours
            TimeSpan timerPeriod = TimeSpan.FromHours(24);
            updateTimer = new Timer(state => Time(), null, timerPeriod, timerPeriod);

            CurrentTimeStamp = PackTimeStamp(0, 0);
        }

        /// <summary>
        /// Gets the tick count from Environment.TickCount as an unsigned integer.
        /// </summary>
        private static uint GetUnsignedEnvironmentTickCount()
        {
            // Environment.TickCount uses GetTickCount (http://msdn.microsoft.com/en-us/library/ms724408(v=vs.85).aspx)
            // internally and according to MSDN [the] function is limited to the resolution of the system timer, which 
            // is typically in the range of 10 milliseconds to 16 milliseconds. The resolution of the GetTickCount function 
            // is not affected by adjustments made by the GetSystemTimeAdjustment function.
            return (uint)Environment.TickCount;
        }

        /// <summary>
        /// Gets the number of elapsed milliseconds since first initialized.
        /// Guaranteed to be a non-negative increasing number.
        /// </summary>
        public static long Time()
        {
            long ts = Interlocked.Read(ref Monotonic.CurrentTimeStamp);
            uint newTick = GetUnsignedEnvironmentTickCount();

            int gen = UnpackGeneration(ts);
            uint oldTick = UnpackTickCount(ts);

            // If newTick < oldTick we've flipped around and need to increment the generation
            if (newTick < oldTick)
            {
                gen++;

                long newTimeStamp = PackTimeStamp(gen, newTick);
                long originalStamp;

                originalStamp = Interlocked.CompareExchange(ref Monotonic.CurrentTimeStamp, newTimeStamp, ts);

                if (originalStamp != ts)
                {
                    // We did not succeeded in replacing the timestamp.

                    // Continue trying to update the timeInfo until we succeed
                    // or the generation value is upgraded by someone else
                    while (UnpackGeneration(originalStamp) < gen)
                    {
                        originalStamp = Interlocked.CompareExchange(ref Monotonic.CurrentTimeStamp, newTimeStamp, originalStamp);
                    }
                }
            }
            else
            {
                // Reducing contention by never updating the timeinfo
                // more than once every 10 seconds (10*1000 = 10s in ms),
                if ((newTick - oldTick) > (10 * 1000))
                {
                    long newTimeStamp = PackTimeStamp(gen, newTick);
                    long originalStamp;

                    bool exchangeSucceeded;

                    do
                    {
                        originalStamp = Interlocked.CompareExchange(ref Monotonic.CurrentTimeStamp, newTimeStamp, ts);
                        exchangeSucceeded = (originalStamp == ts);
                    } while (!exchangeSucceeded && UnpackTickCount(originalStamp) < oldTick);
                }
            }

            return ((gen * ((long)uint.MaxValue + 1)) + newTick) - InitialTickCount;
        }

        /// <summary>
        /// Unpacks the tick count from the last 32 bits of a 64-bit timeStamp.
        /// </summary>
        /// <param name="timeStamp">The time stamp.</param>
        /// <returns></returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static uint UnpackTickCount(long timeStamp)
        {
            return (uint)(timeStamp & 0xFFFFFFFF);
        }

        /// <summary>
        /// Unpacks the generation count from the first 32-bits of a 64-bit timeStamp
        /// </summary>
        /// <param name="timeStamp">The time stamp.</param>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static int UnpackGeneration(long timeStamp)
        {
            return (int)(timeStamp >> 32);
        }

        /// <summary>
        /// Packs the generation and tickCount into one 64-bit integer.
        /// </summary>
        /// <param name="generation">The generation.</param>
        /// <param name="tickCount">The tick count.</param>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static long PackTimeStamp(int generation, uint tickCount)
        {
            return ((long)generation << 32) + tickCount;
        }
    }
}
