using ServerProgram.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerProgram.core;
using ServerProgram.core.protocol;
using MyGameProto;
using ServerProgram.core.manager;

namespace ServerProgram.logic
{
    public partial class HandleConnMsg
    {
        public void HandleMsg(Conn conn, ConnReq req) 
        {
            if (req.RegisterReq != null)
            {
                HandleRegisterReq(conn, req.RegisterReq);
            }
            else if (req.HeartBeatReq != null)
            {
                HandleHeartBeatReq(conn);
            }
            else if (req.LoginReq != null)
            {
                HandleLoginReq(conn, req.LoginReq);
            }
            else if (req.LogoutReq != null)
            {
                HandleLogoutReq(conn, req.LogoutReq);
            }
        }
        public void HandleHeartBeatReq(Conn conn)
        {
            conn.lastTickTime = Sys.GetTimeStamp();
            Console.WriteLine("[更新心跳时间] " + conn.GetAdress());

            HeartBeatAck heartBeatAck = new HeartBeatAck();
            MyGameAck myGameAck = GetConnAck();
            myGameAck.ConnAck.HeartBeatAck = heartBeatAck;

            SendMsg(conn, myGameAck);
        }

        ///<Summary>
        /// 注册
        /// 协议参数：str 用户名，str密码
        /// 返回协议：-1 表示失败，0表示成功
        ///</Summary>
        public void HandleRegisterReq(Conn conn, RegisterReq req)
        {
            // 获取数值
            string id = req.Id;
            string pw = req.Pw;
            Console.WriteLine("[收到注册协议]" + conn.GetAdress() + "用户名：" + id + "密码：" + pw);

            // 构建返回协议
            RegisterAck ack = new RegisterAck();

            // 注册
            if (DataMgr.instance.Register(id, pw))
            {
                ack.Result = 0;
            }
            else
            {
                ack.Result = -1;
            }

            // 创建角色
            DataMgr.instance.CreatePlayer(id);

            MyGameAck myGameAck = GetConnAck();
            myGameAck.ConnAck.RegisterAck = ack;

            SendMsg(conn, myGameAck);
        }

        public void HandleLoginReq(Conn conn, LoginReq req)
        {
            string id = req.Id;
            string pw = req.Pw;

            Console.WriteLine("[收到登录协议]" + conn.GetAdress() + "用户名：" + id + "密码：" + pw);

            // 构建返回协议
            MyGameAck myGameAck = GetConnAck();

            // 验证
            if (!DataMgr.instance.CheckPassWord(id, pw))
            {
                LoginAck ack = new LoginAck();
                myGameAck.ConnAck.LoginAck = ack;
                ack.Result = -1;
               
                SendMsg(conn, myGameAck);
                return;
            }

            // 是否已经登录 如果玩家已经登录，则踢下线，发送Logout协议
            MyGameAck kickOffAck = GetConnAck();
            LogoutAck logoutAck = new LogoutAck();
            kickOffAck.ConnAck.LogoutAck = logoutAck;
            logoutAck.Result = -1;
            if (Player.KickOff(id, kickOffAck))
            {
                return;
            }

            // 获取玩家数据
            PlayerData playerData = DataMgr.instance.GetPlayerData(id);
            if (playerData == null)
            {
                LoginAck ack = new LoginAck();
                myGameAck.ConnAck.LoginAck = ack;
                ack.Result = -1;

                SendMsg(conn, myGameAck);
                return;
            }
            conn.player = new Player(id, conn);
            conn.player.data = playerData;

            // 事件触发
            ServNet.instance.handlePlayerEvent.OnLogin(conn.player);

            LoginAck loginAck = new LoginAck();
            myGameAck.ConnAck.LoginAck = loginAck;
            loginAck.Result = 0;

            SendMsg(conn, myGameAck);
            return;
        }

        public void HandleLogoutReq(Conn conn, LogoutReq req)
        {
            Console.WriteLine("[收到登出协议]" + conn.GetAdress());
            MyGameAck myGameAck = GetConnAck();
            LogoutAck logoutAck = new LogoutAck();
            logoutAck.Result = 0;
            myGameAck.ConnAck.LogoutAck = logoutAck;
            if (conn.player == null)
            {
                SendMsg(conn, myGameAck);
                conn.Close();
            }
            else
            {
                SendMsg(conn, myGameAck);
                conn.player.Logout();
            }
        }

        private MyGameAck GetConnAck()
        {
            MyGameAck ack = new MyGameAck();
            ConnAck connAck = new ConnAck();
            ack.ConnAck = connAck;
            return ack;
        }

        public void SendMsg(Conn conn, MyGameAck ack) 
        {
            if (conn == null)
                return;
            ServNet.instance.Send(conn, ack);
        }
    }
}
