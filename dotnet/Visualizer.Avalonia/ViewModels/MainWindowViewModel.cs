using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using Lego.Client;
using Lego.Core;
using Maths;
using ReactiveUI;

namespace Visualizer.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly LegoCarClient _client = new(Environment.MachineName);
        
        private string _host;
        private bool _isConnected;
        private int _throttle;
        public ICommand Connect { get; }
        public ICommand Disconnect { get; }
        
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

        public int Throttle
        {
            get => _throttle;
            set => this.RaiseAndSetIfChanged(ref _throttle, value);
        }

        public MainWindowViewModel()
        {
            Connect = ReactiveCommand.CreateFromTask(ConnectAsync);
            Disconnect = ReactiveCommand.CreateFromTask(DisconnectAsync);
            _client.WillUpdate = ClientWillUpdate;
            _client.DidUpdate = ClientDidUpdate;
            _client.Disconnected = ClientDisconnected;
            _client.Connected = ClientConnected;
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
            return Dispatcher.UIThread.InvokeAsync(() =>
            {
                State.Motion = state.Motion.ToString();
            });
        }

        private Task ClientWillUpdate(LegoCarClient client)
        {
            //Console.WriteLine("Client will update");
            client.SetMotorSpeed(Throttle);
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
                IsConnected = true; //_client.IsConnected;
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
                IsConnected = false; //_client.IsConnected;
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
    }
}