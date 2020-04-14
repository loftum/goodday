using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Goodday.Core.Domain;
using Goodday.Core.Domain.Types;

namespace Goodday.Core
{
    public class GooddayClient: IDisposable
    {
        public bool IsRunning { get; private set; }
        private readonly List<UdpClient> _clients;
        private Task _receiveTask;
        private Task _sendTask;

        private readonly string _serviceType;
        private readonly string _domain;
        
        public GooddayClient(string serviceType, string domain = "local")
        {
            _serviceType = serviceType.UnderscorePrefix();
            _domain = domain;
            _clients = Zeroconf.CreateUdpClients().ToList();
        }
        
        public void Stop()
        {
            IsRunning = false;
            foreach (var client in _clients)
            {
                client.Close();
            }
        }

        public void Start()
        {
            IsRunning = true;
            _sendTask = SendAsync();
            _receiveTask = ReceiveAsync();
        }

        private async Task SendAsync()
        {
            ushort ii = 0;
            do
            {
                ii = ii == ushort.MaxValue ? (ushort)0 : (ushort)(ii + 1);
                Console.WriteLine($"Sending {ii}");
                var request = CreateRequest(ii);
                Console.WriteLine(request);
                var bytes = new MessageWriter().Write(request);
                await _clients.AllSendAsync(bytes, bytes.Length, Zeroconf.BroadcastEndpoint);
                await Task.Delay(10000);
            } while (IsRunning);
        }

        private Task ReceiveAsync()
        {
            return Task.WhenAll(_clients.Select(ReceiveFromAsync));
        }

        private async Task ReceiveFromAsync(UdpClient client)
        {
            while (IsRunning)
            {
                try
                {
                    var result = await client.ReceiveAsync();
                    var message = MessageParser.Decode(result.Buffer);
                    Console.WriteLine("Got");
                    Console.WriteLine(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private Message CreateRequest(ushort id)
        {
            return new Message
            {
                Id = id,
                Type = MessageType.Query,
                Questions = new List<Question>
                {
                    new Question
                    {
                        QName = $"{_serviceType}.{_domain}.",
                        QClass = QClass.IN,
                        QType = QType.PTR
                    }
                }
            };
        }

        public void Dispose()
        {
            IsRunning = false;
            foreach (var client in _clients)
            {
                client.Close();
                client.Dispose();
            }
        }
    }
}