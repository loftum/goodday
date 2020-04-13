using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Bonjour.Server
{
    public class EchoServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly List<ClientHandler> _handlers = new List<ClientHandler>();

        public int Port => ((IPEndPoint) _listener.LocalEndpoint).Port;
        public bool IsRunning { get; private set; }
        private readonly Task _task;
        
        public EchoServer()
        {
            _listener = new TcpListener(IPAddress.Any, 0);
            _task = RunAsync();
        }

        public async Task RunAsync()
        {
            IsRunning = true;
            _listener.Start();
            Console.WriteLine($"Listening for connections on port {Port}");
            while (IsRunning)
            {
                var socket = await _listener.AcceptSocketAsync();
                var handler = new ClientHandler(socket);
                
                _handlers.Add(handler);
            }
            
        }

        public void Stop()
        {
            IsRunning = false;
            _listener.Stop();
        }

        public void Dispose()
        {
            foreach (var handler in _handlers)
            {
                handler.Stop();
            }
        }
    }
}