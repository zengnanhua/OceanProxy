using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OceanProxy
{
    public static class GlobalPort
    {
        /// <summary>
        /// 系统中已使用的端口
        /// </summary>
        public static List<int> UsedPortList { get; set; } = new List<int>();

        /// <summary>
        /// 随机获取服务端与客户端私有通讯端口
        /// </summary>
        /// <returns></returns>
        public static int GetRandomPrivatePort()
        {
            while (true)
            {
                Random rnd = new Random((int)DateTime.Now.Ticks);
                int biaoji = rnd.Next(20000, 20010);
                if (UsedPortList.Where(c => c == biaoji).Count() <= 0)
                {
                    UsedPortList.Add(biaoji);
                    return biaoji;
                }
            }
        }
        /// <summary>
        /// 添加系统中已使用的端口
        /// </summary>
        /// <param name="port"></param>
        public static void AddUsedPort(int port)
        {
            UsedPortList.Add(port);
        }
        /// <summary>
        /// 删除系统中已使用的端口
        /// </summary>
        public static void DeleteUsedPort(int port)
        {
            UsedPortList.Remove(port);
        }
    }
}
