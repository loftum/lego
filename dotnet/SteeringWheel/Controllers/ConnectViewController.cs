using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Convenient.Gooday;
using CoreFoundation;
using Foundation;
using LCTP.Core.Server;
using Maths.Logging;
using UIKit;

namespace SteeringWheel.Controllers
{
    public class HostSource : UITableViewDataSource
    {
        private const string TableCellIdentitifier = "HostCell";
        
        public List<string> Services { get; } = new List<string>();
        
        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return Services.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(TableCellIdentitifier)
                       ?? new UITableViewCell(UITableViewCellStyle.Default, TableCellIdentitifier);
            
            cell.TextLabel.Text = Services[indexPath.Row];
            return cell;
        }
    }

    public class RowSelectedEventArgs : EventArgs
    {
        public RowSelectedEventArgs(NSIndexPath indexPath)
        {
            IndexPath = indexPath;
        }

        public NSIndexPath IndexPath { get; }
    }

    public class TableViewDelegate : UITableViewDelegate
    {
        public event EventHandler<RowSelectedEventArgs> OnRowSelected;
        
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            OnRowSelected?.Invoke(tableView, new RowSelectedEventArgs(indexPath));
        }
    }
    
    public class ConnectViewController : UIViewController
    {
        private readonly ILogger _logger = Log.For<ConnectViewController>();
        private readonly UITextField _hostField;
        private readonly UIButton _connectButton;
        private readonly UITableView _hostsTable;
        private readonly NSUserDefaults _userDefaults = new NSUserDefaults("app", NSUserDefaultsType.SuiteName);
        private readonly NetworkServiceBrowser _browser = new NetworkServiceBrowser("_legocar._tcp", logger: LogAdapter.For(nameof(NetworkServiceBrowser)));

        private readonly HostSource _hostSource = new HostSource();
        private readonly TableViewDelegate _tableViewDelegate = new TableViewDelegate();
        private readonly ConcurrentDictionary<string, NetworkService> _services = new ConcurrentDictionary<string, NetworkService>();
        
        public ConnectViewController()
        {
            View.BackgroundColor = UIColor.White;
            _hostField = new UITextField
            {
                BorderStyle = UITextBorderStyle.RoundedRect,
                Text = _userDefaults.StringForKey("host") ?? "host",
                TextColor = UIColor.DarkGray,
                SpellCheckingType = UITextSpellCheckingType.No,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
            }
            .WithParent(View)
            .WithConstraints(v => new[]
            {
                v.TopAnchor.ConstraintEqualTo(View.TopAnchor, 100),
                v.WidthAnchor.ConstraintGreaterThanOrEqualTo(200),
                v.TrailingAnchor.ConstraintEqualTo(View.CenterXAnchor, 20),
            })
            .With(v => v.SpellCheckingType = UITextSpellCheckingType.No);
            
            _connectButton = new UIButton()
            .WithParent(View)
            .WithTitle("Connect")
            .WithTitleColor(UIColor.Blue)
            .WithTouchUpInside(ConnectButton_TouchUpInside)
            .WithConstraints(v => new[]{
                v.TopAnchor.ConstraintEqualTo(View.TopAnchor, 100),
                v.LeadingAnchor.ConstraintEqualTo(View.CenterXAnchor, 20),
                v.WidthAnchor.ConstraintGreaterThanOrEqualTo(100)
            });
            
            _hostsTable = new UITableView()
            .WithParent(View)
            .With(t => t.Layer.CornerRadius = 5)
            .WithConstraints(v => new []
            {
                v.TopAnchor.ConstraintEqualTo(_hostField.BottomAnchor, 20),
                v.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                v.WidthAnchor.ConstraintGreaterThanOrEqualTo(400),
                v.HeightAnchor.ConstraintGreaterThanOrEqualTo(200)
            });

            _tableViewDelegate.OnRowSelected += HostSelected;
            _hostsTable.Delegate = _tableViewDelegate;
            
            _hostsTable.DataSource = _hostSource;
            
            _browser.FoundService += BrowserFoundService;
            _browser.RemovedService += BrowserRemovedService;
            _browser.Start();
        }

        private async void HostSelected(object sender, RowSelectedEventArgs e)
        {
            var name = _hostSource.Services[e.IndexPath.Row];
            var service = _services[name];
            await Connect(service.HostName, service.Port);
        }

        private void BrowserFoundService(object sender, NetworkServiceEventArgs e)
        {
            _logger.Trace($"Browser found service: {e.Service.Name}");
            _services[e.Service.Name] = e.Service;
            if (_hostSource.Services.Contains(e.Service.Name))
            {
                return;
            }
            _hostSource.Services.Add(e.Service.Name);
            DispatchQueue.MainQueue.DispatchAsync(_hostsTable.ReloadData);
        }
        
        private void BrowserRemovedService(object sender, NetworkServiceEventArgs e)
        {
            _logger.Trace($"Browser removed service: {e.Service.Name}");
            _services.TryRemove(e.Service.Name, out _);
            if (!_hostSource.Services.Contains(e.Service.Name))
            {
                return;
            }
            _hostSource.Services.Remove(e.Service.Name);
            DispatchQueue.MainQueue.DispatchAsync(_hostsTable.ReloadData);
        }

        ~ConnectViewController()
        {
            _browser.Stop();
            _browser.FoundService -= BrowserFoundService;
            _browser.RemovedService -= BrowserRemovedService;
        }

        private async void ConnectButton_TouchUpInside(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_hostField.Text))
            {
                return;
            }
            var parts = _hostField.Text.Split(':');
            var host = parts[0];
            var port = parts.Length > 1 && int.TryParse(parts[1], out var v) ? v : LctpServer.DefaultPort;
            await Connect(host, port);
        }

        private async Task Connect(string host, int port)
        {
            try
            {
                var vc = new SteeringWheelViewController();
                await vc.ConnectAsync(host, port);
                ShowViewController(vc, this);
                _userDefaults.SetString(_hostField.Text, "host");
            }
            catch (Exception ex)
            {
                using (var controller = new UIAlertController())
                {
                    using(var action = UIAlertAction.Create(ex.Message, UIAlertActionStyle.Default, null))
                    {
                        controller.AddAction(action);
                        ShowViewController(controller, this);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _browser.Stop();
            }
        }
    }
}