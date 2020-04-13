using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Bonjour.Server
{
    public class ClientHandler
    {
        private readonly Socket _socket;
        private readonly Task _task;

        public ClientHandler(Socket socket)
        {
            _socket = socket;
            _task = RunAsync();
        }

        public async Task RunAsync()
        {
            try
            {
                var buffer = new byte[4096];
                while (true)
                {
                    var bytes = new List<byte>();
                    var mesage = new StringBuilder();
                    var count = 0;
                    do
                    {
                        count = await _socket.ReceiveAsync(buffer, SocketFlags.None);
                        bytes.AddRange(buffer.Take(count));
                        mesage.Append(Encoding.UTF8.GetString(buffer));
                    } while (count == 4096);
                    Console.WriteLine(mesage.ToString());
                    await _socket.SendAsync(bytes.ToArray(), SocketFlags.None);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
            
        }

        public void Stop()
        {
            _socket.Disconnect(false);
            _socket.Close();
            _socket.Dispose();
        }
    }
}