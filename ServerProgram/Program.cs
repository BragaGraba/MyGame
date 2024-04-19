using System;
using System.Net;
using System.Net.Sockets;
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
}