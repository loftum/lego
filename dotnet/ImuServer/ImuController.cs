using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Devices.Adafruit.BNO055;
using LCTP.Core;
using LCTP.Core.Routing;
using Maths;
using Timer = System.Timers.Timer;

namespace ImuServer
{
    public class ImuController : BaseController, IDisposable
    {
        private Double3 _orientation;
        private readonly Timer _timer = new Timer(100);
        private readonly object _updateLock = new object();
        private readonly BNO055Sensor _imu;
        private readonly bool _verbose;
        
        public ImuController(BNO055Sensor imu, bool verbose)
        {
            _imu = imu;
            _verbose = verbose;
            Get("orientation", GetOrientation);
            _timer.Elapsed += Update;
        }

        private void Update(object sender, ElapsedEventArgs e)
        {
            if (Monitor.TryEnter(_updateLock))
            {
                try
                {
                    var euler = _imu.ReadEulerData();
                    _orientation = euler;
                }
                finally
                {
                    Monitor.Exit(_updateLock);    
                }
            }
        }

        private Task<ResponseMessage> GetOrientation(RequestMessage request, Match match)
        {
            if (_verbose)
            {
                Console.WriteLine($"orientation: {_orientation}");    
            }
            return Task.FromResult(ResponseMessage.Ok(_orientation));
        }

        public override void ConnectionClosed()
        {
            _timer.Stop();
        }

        public override void ConnectionOpened()
        {
            _timer.Start();
        }

        public void Dispose()
        {
            _timer.Elapsed -= Update;
            _timer?.Dispose();
        }
    }
}