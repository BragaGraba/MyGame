using System;
using System.Net;
using System.Net.Sockets;
using ServerProgram.core;
using ServerProgram.core.manager;
using ServerProgram.Net;

class MainClass
{
    public static void Main(string[] args)
    {
        AsyncServ();
    }

    private void SyncServ()
    {
        // Socket
        Socket listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Bind
        IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
        IPEndPoint ipEp = new IPEndPoint(ipAdr, 1234);
        listenfd.Bind(ipEp);

        //Listen
        listenfd.Listen(0);
        Console.WriteLine("[服务器]启动成功");

        while (true)
        {
            // Accept
            Socket connfd = listenfd.Accept(); // connection file descriptor 连接文件描述符
            Console.WriteLine("[服务器]Accept");

            // Recv
            byte[] readBuff = new byte[1024];
            int count = connfd.Receive(readBuff);
            string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
            Console.WriteLine("[服务器接收]" + str + " ");

            // Send
            str = str + System.DateTime.Now.ToString();
            byte[] bytes = System.Text.Encoding.Default.GetBytes("serv echo " + str);
            connfd.Send(bytes);
        }
    }

    private static void AsyncServ()
    {
        Serv serv = new Serv();
        serv.Start("127.0.0.1", 1234);

        while(true)
        {
            string str = Console.ReadLine();
            switch (str)
            {
                case "quit":
                    return;
            }
        }
    }

    private static void TestDataMgr()
    {
        DataMgr dataMgr = new DataMgr();
        bool ret = dataMgr.Register("Wyy", "123");
        if (ret)
            Console.WriteLine("注册成功");
        else
            Console.WriteLine("注册失败");

        ret = dataMgr.CreatePlayer("Wyy");
        if (ret)
            Console.WriteLine("创建玩家成功");
        else
            Console.WriteLine("创建玩家失败");

        PlayerData pd = dataMgr.GetPlayerData("Wyy");
        if (pd != null)
            Console.WriteLine("获取玩家成功 " + pd.score);
        else
            Console.WriteLine("获取玩家数据失败");

        pd.score += 10;
        Player p = new Player();
        p.id = "Wyy";
        p.data = pd;
        dataMgr.SavePlayer(p);

        pd = dataMgr.GetPlayerData("Wyy");
        if (pd != null)
            Console.WriteLine("获取玩家成功 " + pd.score);
        else
            Console.WriteLine("获取玩家数据失败");
    }
}