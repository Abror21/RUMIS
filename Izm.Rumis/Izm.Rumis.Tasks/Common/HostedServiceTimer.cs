using System;
using System.Threading;

namespace Izm.Rumis.Tasks.Common
{
    public class HostedServiceTimer : IDisposable
    {
        private Timer timer;

        public void Disable()
        {
            timer.Change(Timeout.Infinite, 0);
        }

        public void Start(TimerCallback callback, TimeSpan startTime, TimeSpan interval)
        {
            double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            double intervalPeriod = interval.TotalMilliseconds;
            double scheduledTime = startTime.TotalMilliseconds;
            double waitMilliseconds = scheduledTime - currentTime;

            if (waitMilliseconds < 0)
                waitMilliseconds = TimeSpan.FromDays(1).TotalMilliseconds - waitMilliseconds;

            timer = new Timer(callback, null, Convert.ToInt32(waitMilliseconds), Convert.ToInt32(intervalPeriod));
        }

        public void Change(TimeSpan startTime, TimeSpan interval)
        {
            double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            double intervalPeriod = interval.TotalMilliseconds;
            double scheduledTime = startTime.TotalMilliseconds;
            double waitMilliseconds = scheduledTime - currentTime;

            if (waitMilliseconds < 0)
                waitMilliseconds = TimeSpan.FromDays(1).TotalMilliseconds - waitMilliseconds;

            timer.Change(Convert.ToInt32(waitMilliseconds), Convert.ToInt32(intervalPeriod));
        }

        public void Dispose() => timer?.Dispose();
    }
}
