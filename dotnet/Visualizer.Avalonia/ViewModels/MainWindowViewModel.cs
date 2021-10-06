using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Lego.Client;
using Lego.Core;
using ReactiveUI;

namespace Visualizer.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly LegoCarClient _client = new(Environment.MachineName);
        
        private string _host;
        private bool _isConnected;
        private string _throttle;
        private string _steer;
        public ICommand Connect { get; }
        public ICommand Disconnect { get; }
        public ICommand PerformUturn { get; }
        
        public string Host
        {
            get => _host;
            set => this.RaiseAndSetIfChanged(ref _host, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => this.RaiseAndSetIfChanged(ref _isConnected, value);
        }

        public StateViewModel State { get; } = new();

        public string Throttle
        {
            get => _throttle;
            set => this.RaiseAndSetIfChanged(ref _throttle, value);
        }

        public string Steer
        {
            get => _steer;
            set => this.RaiseAndSetIfChanged(ref _steer, value);
        }

        public MainWindowViewModel()
        {
            Connect = ReactiveCommand.CreateFromTask(ConnectAsync);
            Disconnect = ReactiveCommand.CreateFromTask(DisconnectAsync);
            PerformUturn = ReactiveCommand.CreateFromTask(PerformUturnAsync);
            _client.WillUpdate = ClientWillUpdate;
            _client.DidUpdate = ClientDidUpdate;
            _client.Disconnected = ClientDisconnected;
            _client.Connected = ClientConnected;
        }

        private Task PerformUturnAsync()
        {
            return Task.CompletedTask;
        }

        private Task ClientConnected(LegoCarClient client)
        {
            Console.WriteLine("Client connected");
            Dispatcher.UIThread.Post(() =>
            {
                IsConnected = client.IsConnected;    
            });
            return Task.CompletedTask;
        }

        private Task ClientDisconnected(LegoCarClient client)
        {
            Console.WriteLine("Client disconnected");
            return Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsConnected = client.IsConnected;    
            });
        }

        private Task ClientDidUpdate(LegoCarClient client, LegoCarState state)
        {
            //Console.WriteLine("Client did update");
            return Dispatcher.UIThread.InvokeAsync(() => State.Update(state));
        }

        private Task ClientWillUpdate(LegoCarClient client)
        {
            //Console.WriteLine("Client will update");
            if (int.TryParse(Throttle, out var throttle))
            {
                client.SetMotorSpeed(throttle);    
            }

            if (int.TryParse(Steer, out var steer))
            {
                client.SetSteer(steer);
            }
            return Task.CompletedTask;
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("Connecting");
                await _client.ConnectAsync(Host);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                IsConnected = _client.IsConnected;
            }
        }
        
        private async Task DisconnectAsync()
        {
            try
            {
                Console.WriteLine("Disconnecting");
                await _client.DisconnectAsync();
                await Task.Delay(500);
                Console.WriteLine("Disconnected");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Console.WriteLine("Setting IsConnected");
                IsConnected = _client.IsConnected;
                Console.WriteLine("Did set IsConnected");
            }
        }
    }

    public class StateViewModel : ViewModelBase
    {
        private string _motion;

        public string Motion
        {
            get => _motion;
            set => this.RaiseAndSetIfChanged(ref _motion, value);
        }

        public RotateTransform CarTransform { get; } = new RotateTransform();

        public void Update(LegoCarState state)
        {
            Motion = state.Motion.ToString();
            CarTransform.Angle = Degrees(state.EulerAngles.Z);
        }

        private static double Degrees(double radians)
        {
            return 180 / Math.PI * radians;
        }
    }
}