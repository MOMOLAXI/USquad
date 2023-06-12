using System;
using System.Diagnostics;

namespace UniverseEngine
{
    internal readonly struct ValueStopwatch
    {
        static readonly double s_TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        readonly long m_StartTimestamp;

        public static ValueStopwatch StartNew() => new(Stopwatch.GetTimestamp());

        ValueStopwatch(long startTimestamp)
        {
            m_StartTimestamp = startTimestamp;
        }

        public TimeSpan Elapsed => TimeSpan.FromTicks(ElapsedTicks);

        public double TotalSeconds => Elapsed.TotalSeconds;
        public double TotalHours => Elapsed.TotalHours;
        public double TotalMinutes => Elapsed.TotalMinutes;
        public double TotalMilliSeconds => Elapsed.TotalMilliseconds;
        
        public bool IsInvalid => m_StartTimestamp == 0;

        public long ElapsedTicks
        {
            get
            {
                if (IsInvalid)
                {
                    throw new InvalidOperationException("Detected invalid initialization(use 'default'), only to create from StartNew().");
                }

                long delta = Stopwatch.GetTimestamp() - m_StartTimestamp;
                return (long)(delta * s_TimestampToTicks);
            }
        }
    }
}