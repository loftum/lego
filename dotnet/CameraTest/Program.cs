using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Camera;

namespace CameraTest
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                var host = args.Length > 0 ? args[0] : "255.255.255.255";
                var port = args.Length > 1 && int.TryParse(args[1], out var v) ? v : 5081;
                
                Console.WriteLine("CameraTest");
                using (var source = new CancellationTokenSource())
                {
                    Console.CancelKeyPress += (s, e) =>
                    {
                        Console.WriteLine("Stopping ...");
                        source.Cancel();
                        e.Cancel = true;
                    };
                    await RunAsync(host, port, source.Token);
                }
            }
            catch (TaskCanceledException)
            {
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }

            Console.WriteLine("Bye!");
            return 0;
        }

        private static async Task RunAsync(string host, int port, CancellationToken cancellationToken)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(host), port);
            Console.WriteLine($"Broadcasting to {endpoint}");
            using (var udpClient = new UdpClient())
            {
                udpClient.EnableBroadcast = true;
                using (var controller = CameraController.Instance)
                {
                    Console.WriteLine("Opening video stream");
                    var settings = new CameraVideoSettings
                    {
                        CaptureFramerate = 30,
                    };
                    controller.OpenVideoStream(settings, bytes =>
                    {
                        udpClient.Send(bytes, bytes.Length, endpoint);
                    });
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                    Console.WriteLine("Closing video stream");
                    controller.CloseVideoStream();
                }    
            }
        }
    }
}
