using System;

namespace Visualizer.ViewControllers
{
    public class ConnectEventArgs : EventArgs
    {
        public string Host { get; }
        public int Port { get; }

        public ConnectEventArgs(string host, int port)
        {
            Host = host;
            Port = port;
        }
    }
}