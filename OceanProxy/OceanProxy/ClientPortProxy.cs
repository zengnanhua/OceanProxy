using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OceanProxy
{
    public class ClientPortProxy
    {
        public string PrivateIp { get; private set; }
        /// <summary>
        /// 私有端口
        /// </summary>
        public int PrivatePort { get;private set; }

        public string ProxyPortIp { get; private set; }
        /// <summary>
        /// 代理端口
        /// </summary>
        public int ProxyPort { get; private set; }
        /// <summary>
        /// 通知TcpClient
        /// </summary>
        private TcpClient _notifyTcpClient = null;
        /// <summary>
        /// 通知网络流
        /// </summary>
        private NetworkStream _networkStream = null;

        public ClientPortProxy(string PrivateIp,int PrivatePort,string ProxyPortIp,int ProxyPort)
        {
            this.PrivateIp = PrivateIp;
            this.PrivatePort = PrivatePort;
            this.ProxyPortIp = ProxyPortIp;
            this.ProxyPort = ProxyPort;
        }

        public async Task Start()
        {
            Jie();
        }
        private async Task Jie()
        {
            while (true)
            {
                try
                {
                    _notifyTcpClient = new TcpClient();
                    _notifyTcpClient.Connect(new IPEndPoint(IPAddress.Parse(PrivateIp), PrivatePort));
                    _networkStream = _notifyTcpClient.GetStream();
                    byte[] bt = Encoding.Default.GetBytes("ok");//这里发送一个连接提示
                    _networkStream.Write(bt, 0, bt.Length);
                    Intercommunicate();
                }
                catch (Exception ex)
                {
                    _notifyTcpClient?.Dispose();
                    _networkStream?.Dispose();
                    await Task.Delay(500);
                }
            }
        }
        /// <summary>
        /// 本地和服务端建立连接
        /// </summary>
        /// <returns></returns>
        private async Task Intercommunicate()
        {
            while (true)
            {
                try
                {
                    byte[] bt = new byte[4];
                    _networkStream.Read(bt, 0, bt.Length);
                    TcpClient tc1 = new TcpClient();
                    tc1.Connect(new IPEndPoint(IPAddress.Parse(PrivateIp), PrivatePort));
                    TcpClient tc2 = new TcpClient();
                    tc2.Connect(new IPEndPoint(IPAddress.Parse(ProxyPortIp), ProxyPort));
                    tc1.GetStream().Write(bt, 0, bt.Length);
                    TcpTunnel tcpTunnel = new TcpTunnel(tc1, tc2);
                    tcpTunnel.Start();
                }
                catch (Exception ex)
                {
                    break;
                }
            }
        }

    }
}
