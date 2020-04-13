using System;
using System.Threading;
using Goodday.Core;

namespace Bonjour.Client
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Bonjour Client");
            try
            {
                using (var source = new CancellationTokenSource())
                {
                    using (var client = new GooddayClient("_echo._tcp"))
                    {
                        Console.CancelKeyPress += (s, e) =>
                        {
                            source.Cancel();
                            client.Stop();
                        };
                        client.Start();
                        Console.ReadLine();
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