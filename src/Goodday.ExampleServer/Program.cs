using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Goodday.Core;

namespace Bonjour.Server
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Bonjour server");
            try
            {
                using (var source = new CancellationTokenSource())
                {
                    using (var server = new EchoServer())
                    {
                        using (var host = new GooddayHost(Dns.GetHostName(), "_echo._tcp", (ushort) server.Port))
                        {
                            Console.CancelKeyPress += (s, e) =>
                            {
                                source.Cancel();
                                host.StopAsync().Wait(2000);
                            };
                            host.Start();
                            Console.ReadLine();
                        }    
                    }
                }

                return 0;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
        }
    }
}