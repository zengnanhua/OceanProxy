using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace OceanProxy.ConsoleClient
{
    class Program
    {
        public static NetworkStream kongzhins = null;
        static void Main(string[] args)
        {
            try
            {
                TcpClient tc = new TcpClient();
                tc.Connect(new IPEndPoint(IPAddress.Parse("192.168.2.179"), 8078));
                kongzhins = tc.GetStream();
                byte[] bt = Encoding.Default.GetBytes("ok");//这里发送一个连接提示
                kongzhins.Write(bt, 0, bt.Length);
                jieshou();
                WaitHandle.WaitAll(new ManualResetEvent[] { new ManualResetEvent(false) });//这里为什么要这样呢?我发现sqlserver执行是localsystem账号如果console.read()程序马上退出
            }
            catch { }
        }
        public static void jieshou()
        {
            while (true)
            {
                byte[] bt = new byte[4];
                kongzhins.Read(bt, 0, bt.Length);
                TcpClient tc1 = new TcpClient();
                tc1.Connect(new IPEndPoint(IPAddress.Parse("192.168.2.179"), 8078));
                TcpClient tc2 = new TcpClient();
                tc2.Connect(new IPEndPoint(IPAddress.Parse("120.78.69.254"), 54321));
                tc1.SendTimeout = 10000;
                tc1.ReceiveTimeout = 10000;
                tc2.SendTimeout = 10000;
                tc2.ReceiveTimeout = 10000;
                tc1.GetStream().Write(bt, 0, bt.Length);
                object obj1 = (object)(new TcpClient[] { tc1, tc2 });
                object obj2 = (object)(new TcpClient[] { tc2, tc1 });
                ThreadPool.QueueUserWorkItem(new WaitCallback(transfer), obj1);
                ThreadPool.QueueUserWorkItem(new WaitCallback(transfer), obj2);
            }
        }
        public static void transfer(object obj)
        {
            TcpClient tc1 = ((TcpClient[])obj)[0];
            TcpClient tc2 = ((TcpClient[])obj)[1];
            NetworkStream ns1 = tc1.GetStream();
            NetworkStream ns2 = tc2.GetStream();
            while (true)
            {
                try
                {
                    byte[] bt = new byte[10240];
                    int count = ns1.Read(bt, 0, bt.Length);
                    ns2.Write(bt, 0, count);
                }
                catch(Exception e)
                {
                    ns1.Dispose();
                    ns2.Dispose();
                    tc1.Close();
                    tc2.Close();
                    Console.WriteLine($"++++++++++{e.Message}+++++++++++++");
                    break;
                }
            }
        }
    }
}
