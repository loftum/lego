using System;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Lego.Core
{
    /// <summary>
    /// Timer which allows only one Elapsed to be run at a time. Supports async void Elapsed.
    /// </summary>
    public class InterlockedTimer : IDisposable
    {
        public event EventHandler<ElapsedEventArgs> Elapsed; 
        
        private int _running;
        private readonly Timer _timer;

        public InterlockedTimer(double interval)
        {
            _timer = new Timer(interval);
            _timer.Elapsed += TimerElapsed;
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var elapsed = Elapsed;
            if (elapsed == null || Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            {
                return;
            }
            try
            {
                elapsed(this, e);
            }
            finally
            {
                Interlocked.Exchange(ref _running, 0);
            }
        }

        public void Dispose()
        {
            _timer.Elapsed -= TimerElapsed;
            if (_timer.Enabled)
            {
                _timer.Stop();
            }
            _timer?.Dispose();
        }
    }
}