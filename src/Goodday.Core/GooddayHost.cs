using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Goodday.Core.Domain;
using Goodday.Core.Domain.Records;
using Goodday.Core.Domain.Types;

namespace Goodday.Core
{
    public class GooddayHost: IDisposable
    {
        public bool IsRunning { get; private set; }
        private readonly List<UdpClient> _clients;
        private Task _run;

        private readonly string _instanceName;
        private readonly string _serviceType;
        private readonly string _domain;
        private readonly ushort _port;
        
        public GooddayHost(string instanceName, string serviceType, ushort port, string domain = "local")
        {
            _instanceName = instanceName.WithoutPostfix();
            _serviceType = serviceType.UnderscorePrefix();
            _domain = domain;
            _port = port;
            _clients = Zeroconf.CreateUdpClients().ToList();
        }

        public void Start()
        {
            IsRunning = true;
            _run = RunAsync();
        }

        public async Task StopAsync()
        {
            try
            {
                IsRunning = false;
                var goodbye = GetGoodbyeMessage();
                Console.WriteLine(goodbye);
                var data = MessageParser.Encode(goodbye);
                await _clients.AllSendAsync(data, data.Length, Zeroconf.BroadcastEndpoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not stop:");
                Console.WriteLine(e);
            }
            finally
            {
                _clients.AllClose();
            }
        }

        private Task RunAsync()
        {
            return Task.WhenAll(_clients.Select(ReceiveFromAsync));
        }
        
        private async Task ReceiveFromAsync(UdpClient client)
        {
            var hello = CreateResponse(0, 120);
            var bytes = MessageParser.Encode(hello);
            await client.SendAsync(bytes, bytes.Length, Zeroconf.BroadcastEndpoint);
            while (IsRunning)
            {
                try
                {
                    var result = await client.ReceiveAsync();
                    var message = MessageParser.Decode(result.Buffer);

                    Console.WriteLine("Got");
                    Console.WriteLine(message);
                    
                    if (message.Type == MessageType.Query)
                    {
                        if (message.Questions.Any(q => q.QType == QType.PTR && q.QName == $"{_serviceType}.{_domain}."))
                        {
                            var response = CreateResponse(message.Id, 120);
                            Console.WriteLine("Hey! Someone asked for me!");
                            Console.WriteLine(response);
                            var packet = MessageParser.Encode(response);
                            await client.SendAsync(packet, packet.Length, result.RemoteEndPoint);
                        }   
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private Message GetGoodbyeMessage()
        {
            return CreateResponse(0, 0);
        }

        private Message CreateResponse(ushort id, uint ttl)
        {
            return new Message
            {
                Id = id,
                Type = MessageType.Response,
                AuthoriativeAnswer = true,
                Answers = new List<ResourceRecord>
                {
                    new ResourceRecord
                    {
                        Name = "_services._dns-sd._udp.local",
                        Type = RRType.PTR, 
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new PointerRecord
                        {
                            PTRDName = $"{_serviceType}.{_domain}."
                        }
                    },
                    new ResourceRecord
                    {
                        Name = $"{_serviceType}.{_domain}.",
                        Type = RRType.PTR, 
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new PointerRecord
                        {
                            PTRDName = $"{_instanceName}.{_serviceType}.{_domain}."
                        }
                    },
                    new ResourceRecord
                    {
                        Name = $"{_instanceName}.{_serviceType}.{_domain}.",
                        Type = RRType.SRV, 
                        Class = Class.IN,
                        Ttl = ttl,
                        Record = new ServiceRecord
                        {
                            Priority = 0,
                            Weight = 0,
                            Port = _port,
                            Target = $"{_instanceName}.{_domain}."
                        }
                    },
                }
            };
        }

        public void Dispose()
        {
            foreach (var client in _clients)
            {
                client.Close();
                client.Dispose();
            }
        }
    }
}