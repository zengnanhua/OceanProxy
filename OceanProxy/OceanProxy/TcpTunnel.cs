using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OceanProxy
{
    public class TcpTunnel
    {
        /// <summary>
        /// 连接方客户端
        /// </summary>
        public TcpClient Client { get; private set; }
        /// <summary>
        /// 被连接方客户端
        /// </summary>
        public TcpClient ServerClient { get; private set; }

        public TcpTunnel(TcpClient Client, TcpClient ServerClient)
        {
            var time = 5 * 1000;
            Client.SendTimeout = time;
            Client.ReceiveTimeout = time;
            ServerClient.SendTimeout = time;
            ServerClient.ReceiveTimeout = time;

            this.Client = Client;
            this.ServerClient = ServerClient;
        }

        public async Task Start()
        {
            CancellationTokenSource transfering = new CancellationTokenSource();
            var s1 = this.Client.GetStream();
            var s2 = this.ServerClient.GetStream();
            try
            {
                var task1 = StreamTrans(s1, s2, transfering.Token);
                var task2 = StreamTrans(s2, s1, transfering.Token);
                var completeTask = await Task.WhenAny(task1, task2);
                await completeTask;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"+++++++++++++++{ex.Message}++++++++++++++++++");
            }
            finally
            {
                Console.WriteLine("ddd");
                transfering.Cancel();
                s1.Dispose();
                s2.Dispose();
                this.Client.Dispose();
                this.ServerClient.Dispose();
            }
            
        }

        private async Task StreamTrans(Stream fromStream, Stream toStream, CancellationToken ct)
        {
            int number = 0;
            while (true&&number<50) //50这个值和Task.Delay里面值的是相同的
            {
                byte[] buffer = new byte[10240];
                Console.WriteLine("ffff");
                var count = await fromStream.ReadAsync(buffer, 0, buffer.Length, ct);
                if (count == 0)
                {
                    await Task.Delay(100);
                    number++;
                }
                else 
                {
                    number = 0;
                }
                Console.WriteLine("fff1111");
                await toStream.WriteAsync(buffer, 0, count, ct).ConfigureAwait(false);
            }
        }
    }
}
