using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OceanProxy
{
    public class ServerPortListener:IDisposable
    {
        /// <summary>
        /// 外网端口监听
        /// </summary>
        public int PublicPort { get;private set; }
        /// <summary>
        /// 内部端口监听
        /// </summary>
        public int PrivatePort { get; private set; }
        private TcpListener _publicTcpListener { get; set; }
        private TcpListener _privateTcpListener { get; set; }
        /// <summary>
        /// 外网请求过来的Tcp连接
        /// </summary>
        private  Dictionary<int, TcpClient> _publicRequestTcpClient = new Dictionary<int, TcpClient>();

        /// <summary>
        /// 通知TcpClient
        /// </summary>
        private TcpClient _notifyTcpClient = null;
        /// <summary>
        /// 通知网络流
        /// </summary>
        private NetworkStream _networkStream = null;
        /// <summary>
        /// 是否停止
        /// </summary>
        private bool IsStop = false;
        /// <summary>
        /// 上次接收tcp时间
        /// </summary>
        private DateTime PreAcceptTcpClientDateTime { get; set; }
        public ServerPortListener(int port)
        {
            this.PublicPort = port;
        }
        public async Task StartListenerAsync()
        {
            //开启公有接口监听
            ListenerPublicPortAsync();
            //开启私有接口监听
            ListenerPrivatePortAsync();
        }

        private async Task ListenerPublicPortAsync()
        {
            _publicTcpListener = new TcpListener(IPAddress.Parse("0.0.0.0"), this.PublicPort);
            while (!IsStop)
            {
                var client = await _publicTcpListener.AcceptTcpClientAsync();
                _publicRequestTcpClient.Add(GetRandomInt(), client);
            }
        }
        private async Task ListenerPrivatePortAsync()
        {
            _privateTcpListener = new TcpListener(IPAddress.Parse("0.0.0.0"), this.PrivatePort);
            while (!IsStop)
            {
                var client = await _privateTcpListener.AcceptTcpClientAsync();
                //这里体现的是一个配对的问题，自己体会一下吧
                NetworkStream ns = client.GetStream();
                byte[] bt = new byte[4];
                int count = ns.Read(bt, 0, bt.Length);
                if (count == 2 && bt[0] == 0x6f && bt[1] == 0x6b)
                {
                    if (_notifyTcpClient != null)
                    {
                        //把之前通知对象的资源释放
                        _notifyTcpClient.Dispose();
                        _networkStream.Dispose();
                    }
                    _notifyTcpClient = client;
                    _networkStream = ns;
                }
                else
                {
                    int biaoji = BitConverter.ToInt32(bt, 0);
                    if (_publicRequestTcpClient.ContainsKey(biaoji))
                    {
                        TcpClient tempTcpClient = null;
                        _publicRequestTcpClient.TryGetValue(biaoji, out tempTcpClient);
                        _publicRequestTcpClient.Remove(biaoji);
                        //创建通道对象
                        TcpTunnel tcpTunnel = new TcpTunnel(client, tempTcpClient);
                        tcpTunnel.Start();
                    }
                }

            }
        }


        private int GetRandomInt()
        {
            Random rnd = new Random(Convert.ToInt32(DateTime.Now.Ticks));
            int biaoji = rnd.Next(1000000000, 2000000000);
            return biaoji;
        }

        public void Dispose()
        {
            IsStop = true;
            _publicTcpListener.Stop();
            _privateTcpListener.Stop();
            _notifyTcpClient.Dispose();
            _networkStream.Dispose();
        }
    }
}
