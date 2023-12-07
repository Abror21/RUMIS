using System;

namespace Izm.Rumis.Tasks.Common
{
    public abstract class TaskOptions
    {
        public TimeSpan StartTime { get; set; }
        public int IntervalMinutes { get; set; }
        public bool Enabled { get; set; }
    }
}
