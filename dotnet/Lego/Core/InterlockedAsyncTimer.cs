using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Lego.Core
{
    public class InterlockedAsyncTimer : IDisposable
    {
        public Func<Task> Elapsed { get; set; }
        
        private int _running;
        private readonly Timer _timer;

        public InterlockedAsyncTimer(double interval)
        {
            _timer = new Timer(interval);
            _timer.Elapsed += TimerElapsed;
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();

        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var elapsed = Elapsed;
            if (elapsed == null || Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            {
                return;
            }
            try
            {
                await elapsed();
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