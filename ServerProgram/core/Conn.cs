using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ServerProgram.core;
using MySqlX.Protocol;

namespace ServerProgram.Net
{
    public class Conn
    {
        // 缓冲区大小
        public const int BUFFER_SIZE = 1024;

        // 与客户端连接的套接字
        public Socket socket;

        // 是否使用
        public bool isUse = false;

        // 读缓冲区
        public byte[] readBuff = new byte[BUFFER_SIZE];
        // 当前读缓冲区的长度
        public int buffCount = 0;

        // 粘包分包
        // 转换成byte[] 类型的消息长度
        public byte[] lenBytes = new byte[sizeof(UInt32)];
        // 消息长度
        public Int32 msgLength = 0;

        // 心跳时间
        public long lastTickTime = long.MinValue;

        // 对应的player
        public Player player;


        public Conn()
        {
            readBuff = new byte[BUFFER_SIZE];
        }

        public void Init(Socket socket)
        {
            this.socket = socket;
            isUse = true;
            buffCount = 0;

            // 心跳处理
            lastTickTime = Sys.GetTimeStamp();
        }

        // 缓冲区剩余字节数
        public int BuffRemain()
        {
            return BUFFER_SIZE - buffCount;
        }

        public string GetAdress()
        {
            if (!isUse)
                return "无法获取地址";
            return socket.RemoteEndPoint.ToString();
        }

        public void Close()
        {
            if (!isUse)
                return;
            if (player != null)
            {
                // 玩家退出处理
                player.Logout();
                return;
            }
            Console.WriteLine("[断开连接]" + GetAdress());
            // Socket.Shutdown 方法允许关闭套接字的度、写或者两者都关闭
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            isUse = false;
        }

        // 发送协议
        //public void Send(ProtocolBase protocol)
        //{
        //    ServNet.instance.Send(this, protocol);
        //}
    }
}
