using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using MySql.Data.MySqlClient;
using System.Data;

namespace ServerProgram.Net
{
    public class Serv
    {
        // 监听套接字
        public Socket listenfd;

        // 客户端连接
        public Conn[] conns;

        // 最大连接数
        public int maxConn = 50;

        // 数据库连接
        MySqlConnection sqlConn;

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
            string connStr = "Database=msgboard;Data Source=127.0.0.1;";
            connStr += "Server=localhost;User=root; Password = 123456; port = 3306";
            sqlConn = new MySqlConnection(connStr);
            using(MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    Console.WriteLine("Connecting to MySQL");
                    sqlConn.Open();
                }
                catch   (Exception e)
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

        // Receive 回调
        private void ReceiveCb(IAsyncResult ar)
        {
            Conn conn = (Conn) ar.AsyncState;
            try
            {
                int count = conn.socket.EndReceive(ar);

                // 关闭信号
                if (count <= 0)
                {
                    Console.WriteLine("收到 [" + conn.GetAdress() + "] 断开连接");
                    conn.Close();
                    return;
                }

                // 数据处理
                string str = Encoding.UTF8.GetString(conn.readBuff, 0, count);
                Console.WriteLine("收到 [" + conn.GetAdress() + "] 数据：" + str);

                HandleMsg(conn, str);

                str = conn.GetAdress() + ":" + str;
                byte[] bytes = Encoding.Default.GetBytes(str);

                // 广播
                for (int i = 0; i < conns.Length; i++) 
                {
                    if (conns[i] == null)
                        continue;
                    if (!conns[i].isUse)
                        continue;

                    Console.WriteLine("将消息传播给 " + conns[i].GetAdress());
                    conns[i].socket.Send(bytes);
                }

                // 继续接受，实现循环
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
            }
            catch(Exception ex)
            {
                Console.WriteLine("收到 [" + conn.GetAdress() + "] 断开连接" + ex.Message);
            }
        }

        public void HandleMsg(Conn conn, string str)
        {
            // 获取数据
            if(str == "_GET")
            {
                string cmdStr = "select * from msg order by id desc limit 10;";
                MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
                try
                {
                   MySqlDataReader dataReader = cmd.ExecuteReader();
                    str = "";
                    while (dataReader.Read())
                    {
                        str += dataReader["name"] + ":" + dataReader["msg"] + "\n\r";
                    }
                    dataReader.Close();
                    byte[] bytes = Encoding.Default.GetBytes(str);
                    conn.socket.Send(bytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[数据库]查询失败 " + ex.Message);
                }
            }
            else
            {
                string cmdStrFormat = "insert into msg (name, msg) values(\"{0}\", \"{1}\");";
                string cmdStr = string.Format(cmdStrFormat, conn.GetAdress(), str);
                MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[数据库]插入失败 " + e.Message);
                }
            }
        }
    }
}
