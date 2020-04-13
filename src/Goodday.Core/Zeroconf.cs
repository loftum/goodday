using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Goodday.Core
{
    public static class Zeroconf
    {
        public const int BroadcastPort = 5353;
        public static IPAddress BroadcastAddress { get; } = IPAddress.Parse("224.0.0.251");
        public static IPEndPoint BroadcastEndpoint { get; } = new IPEndPoint(BroadcastAddress, BroadcastPort);

        public static UdpClient CreateUdpClient()
        {
            var client = new UdpClient
            {
                EnableBroadcast = true,
                ExclusiveAddressUse = false,
                MulticastLoopback = false
            };

            //var info = Network.GetUsableInterfaces().First();
            
            var socket = client.Client;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            
            
            socket.Bind(new IPEndPoint(IPAddress.Any, BroadcastPort));
            //Console.WriteLine($"Scanning on iface {info.Interface.Name}, idx {info.Index}, IP: {info.Ipv4Address}");
            //socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, IPAddress.HostToNetworkOrder(info.Index));
            
            client.JoinMulticastGroup(Zeroconf.BroadcastAddress);
            return client;
        }

        public static IEnumerable<UdpClient> CreateUdpClients()
        {
            var adapters = Network.GetUsableInterfaces();
            return adapters.Select(CreateUdpClient);
        }

        public static UdpClient CreateUdpClient(InterfaceInfo adapter)
        {
            var client = new UdpClient
            {
                ExclusiveAddressUse = false,
                MulticastLoopback = false
            };
            var socket = client.Client;
                
            socket.SetSocketOption(SocketOptionLevel.IP,
                SocketOptionName.MulticastInterface,
                IPAddress.HostToNetworkOrder(adapter.Index));

            socket.SetSocketOption(SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress,
                true);
            socket.SetSocketOption(SocketOptionLevel.Socket,
                SocketOptionName.ReceiveTimeout,
                5000);
            socket.MulticastLoopback = false;

            socket.Bind(new IPEndPoint(IPAddress.Any, BroadcastPort));
                
            var multiCastOption = new MulticastOption(BroadcastAddress, adapter.Index);
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multiCastOption);
            Console.WriteLine($"Created UdpClient on {adapter.Ipv4Address}, interface {adapter.Interface.Name} ({adapter.Interface.Description})");
            return client;
        }
    }
}