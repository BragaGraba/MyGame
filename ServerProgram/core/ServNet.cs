using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ServerProgram.Net;
using MySql.Data.MySqlClient;
using ServerProgram.core.protocol;
using ServerProgram.logic;
using System.Reflection;

namespace ServerProgram.core
{
    // 网络管理类
    public class ServNet
    {
        // 监听套接字
        public Socket listenfd;

        // 客户端连接
        public Conn[] conns;

        // 数据库连接
        MySqlConnection sqlConn;

        // 最大连接数
        public int maxConn = 50;

        // 主定时器
        System.Timers.Timer timer = new System.Timers.Timer(1000);

        // 心跳时间
        /// <summary>
        /// 通常TCP断开连接需经历四次挥手，如客户端自己、断网等，四次挥手无法完成。
        /// 而服务器在同一时间能够接入的客户端数量是有限的，过量死连接会导致新连接无法进入。
        /// </summary>
        public long heartBeatTime = 180;

        // 协议
        public ProtocolBase proto;

        // 消息分发
        public HandleConnMsg handleConnMsg = new HandleConnMsg();
        public HandlePlayerMsg handlePlayerMsg = new HandlePlayerMsg();
        public HandlePlayerEvent handlePlayerEvent = new HandlePlayerEvent();

        // 单例
        public static ServNet instance;

        public ServNet()
        {
            instance = this;
        }

        // 获取连接池索引，返回负数表示失败
        public int NewIndex()
        {
            if (conns == null)
                return -1;
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null)
                {
                    conns[i] = new Conn();
                    return i;
                }
                else if (conns[i].isUse == false)
                {
                    return i;
                }
            }
            return -1;
        }

        // 开启服务器
        public void Start(string host, int port)
        {
            // 定时器
            timer.Elapsed += new System.Timers.ElapsedEventHandler(HandleMainTimer);
            timer.AutoReset = false;
            timer.Enabled = true;

            string connStr = "Database=msgboard;Data Source=127.0.0.1;";
            connStr += "Server=localhost;User=root; Password = 123456; port = 3306";
            sqlConn = new MySqlConnection(connStr);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    Console.WriteLine("Connecting to MySQL");
                    sqlConn.Open();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[数据库]连接失败 " + e.Message);
                    return;
                }
            }

            conns = new Conn[maxConn];
            for (int i = 0; i < maxConn; i++)
            {
                conns[i] = new Conn();
            }

            // Socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            listenfd.Bind(ipEp);

            // Listen
            listenfd.Listen(maxConn);

            // Accept
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("[服务器]启动成功");
        }

        // Accept 回调
        private void AcceptCb(IAsyncResult ar)
        {
            try
            {
                Socket socket = listenfd.EndAccept(ar);
                int index = NewIndex();

                if (index < 0)
                {
                    socket.Close();
                    Console.WriteLine("[warning] connect pool is full");
                }
                else
                {
                    Conn conn = conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAdress();
                    Console.WriteLine("Client connect [" + adr + "] conn pool ID : " + index);

                    // 异步接受客户端数据
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                }
                // 再次调用，实现循环
                listenfd.BeginAccept(AcceptCb, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AcceptCb 失败：" + ex.Message);
            }
        }

        // 关闭
        public void Close()
        {
            for (int i = 0; i < conns.Length; i++)
            {
                Conn conn = conns[i];
                if (conn != null) continue;
                if (!conn.isUse) continue;
                // 关闭服务端时，可能玩家尚在游戏中，调用conn.Close()保存玩家数据
                lock (conn)
                {
                    conn.Close();
                }
            }
        }

        // 接受回调
        // conn.buffCount指向缓冲区的数据长度，接受数据缓冲区数据增加时，需要给buffCount加上count
        // 回调只把接受到的数据添加到缓冲区，之后再交给ProcessData处理
        private void ReceiveCb(IAsyncResult ar)
        {
            Conn conn = (Conn)ar.AsyncState;
            // 同一时间，只有一个线程起作用
            lock (conn)
            {
                try
                {
                    Socket socket = listenfd.EndAccept(ar);
                    int count = conn.socket.EndReceive(ar);
                    if (count <= 0)
                    {
                        socket.Close();
                    }
                    else
                    {
                        conn.buffCount += count;
                        ProcessData(conn);
                    }

                    listenfd.BeginAccept(AcceptCb, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ReceiveCb 失败：" + e.Message);
                }
            }
        }

        // 处理收到的数据
        /*
            每条消息均包含消息长度和消息内容两项：
            其中消息长度为一个32位int类型，转换成bytes占用4个字节空间。
            所以当接受缓冲区数据长度小于4字节，一定不是一条完整的消息；
            如果消息长度大于4个字节，先通过BitConverter获取消息长度，然后再判断缓冲区长度是否满足要求；
            消息处理完毕后，如果缓冲区还有数据，再次判断缓冲区中数据是否能构成一条完整的信息。
        */
        private void ProcessData(Conn conn)
        { 
            // 规定更新心跳时间的协议
            //string str = Encoding.UTF8.GetString("...");
            //if (str == "HeartBeat")
            //    conn.lastTickTime = Sys.GetTimeStamp();

            // 小于长度4字节
            if (conn.buffCount < sizeof(Int32))
            {
                return;
            }
            // 消息长度
            Array.Copy(conn.readBuff, conn.lenBytes, sizeof(Int32));
            conn.msgLength = BitConverter.ToInt32(conn.lenBytes, 0);
            if (conn.buffCount < conn.msgLength + sizeof(Int32))
            {
                return;
            }
            // 处理消息
            //string str = Encoding.UTF8.GetString(conn.readBuff, sizeof(Int32), conn.msgLength);
            ProtocolBase protocol = proto.Decode(conn.readBuff, sizeof(Int32), conn.msgLength);
            HandleMsg(conn, protocol);
            // 清除已处理的消息
            int count = conn.buffCount - conn.msgLength - sizeof(Int32);
            Array.Copy(conn.readBuff, sizeof(Int32) + conn.msgLength, conn.readBuff, 0, count);
            conn.buffCount = count;

            if (conn.buffCount > 0)
            {
                ProcessData(conn);
            }
        }

        // 发送消息
        public void Send(Conn conn, ProtocolBase protocol)
        {
            byte[] bytes = protocol.Encode();
            byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] sendBuff = length.Concat(bytes).ToArray();
            try
            {
                conn.socket.BeginSend(sendBuff, 0, sendBuff.Length, SocketFlags.None, null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("[发送消息] " + conn.GetAdress() + ":" + e.Message);
            }
        }

        public void BroadCast(ProtocolBase protocol)
        {
            for (int i = 0; i < conns.Length ; i++)
            {
                if (!conns[i].isUse)
                    continue;
                if (conns[i].player == null)
                    continue;
                Send(conns[i], protocol);
            }

        }

        public void HandleMsg(Conn conn, ProtocolBase protoBase)
        {
            string name = protoBase.GetName();
            Console.WriteLine("[收到协议] " + name);
            string methodName = "Msg" + name;
            // 通过反射的方法，用协议名获取处理函数
            // 连接协议分发
            if (conn.player == null || name == "HeartBeat" || name == "Logout")
            {
                MethodInfo mm = handleConnMsg.GetType().GetMethod(methodName);
                if (mm == null)
                {
                    string str = "[警告]HandleMsg没有处理连接方法";
                    Console.WriteLine(str + methodName);
                    return;
                }
                Object[] obj = new object[]{conn, protoBase};
                Console.WriteLine("[处理连接消息]" + conn.GetAdress() + " :" + name);
                mm.Invoke(handleConnMsg, obj);
            }
            // 角色协议分发
            else
            {
                MethodInfo mm = handlePlayerMsg.GetType().GetMethod(methodName);
                if (mm == null)
                {
                    string str = "[警告]HandleMsg没有处理玩家方法";
                    Console.WriteLine(str + methodName);
                    return;
                }
                Object[] obj = new object[] { conn, protoBase };
                Console.WriteLine("[处理玩家消息]" + conn.GetAdress() + " :" + name);
                mm.Invoke(handlePlayerMsg, obj);
            }
        }

        // 主定时器
        public void HandleMainTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 处理心跳
            HeartBeat();
            timer.Start();
        }

        // 心跳
        public void HeartBeat()
        {
            Console.WriteLine("[主定时器执行]");
            long timeNow = Sys.GetTimeStamp();
            for (int i = 0; i < conns.Length; i++)
            {
                Conn conn = conns[i];
                if (conn == null) continue;
                if (!conn.isUse) continue;

                if (conn.lastTickTime < timeNow - heartBeatTime)
                {
                    Console.WriteLine("[心跳断开连接]" + conn.GetAdress());
                    lock (conn)
                    {
                        conn.Close();
                    }
                }
            }
        }
    }
}
