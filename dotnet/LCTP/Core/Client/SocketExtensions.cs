using System;
using System.Net;
using System.Net.Sockets;

namespace LCTP.Core.Client
{
    public static class SocketExtensions
    {
        public static void Connect(this Socket socket, EndPoint endpoint, TimeSpan timeout)
        {
            var result = socket.BeginConnect(endpoint, null, null);
            if (result.AsyncWaitHandle.WaitOne(timeout, true))
            {
                socket.EndConnect(result);
                return;
            }
            socket.Close();
            throw new SocketException((int)SocketError.TimedOut);
        }
    }
}