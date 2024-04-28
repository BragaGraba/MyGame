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
        }
        public void MsgHeartBeat(Conn conn, ProtocolBase protoBase)
        {
            conn.lastTickTime = Sys.GetTimeStamp();
            Console.WriteLine("[更新心跳时间] " + conn.GetAdress());
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

            // 是否已经登录
            if (!Player.KickOff(id, myGameAck))
            {
                LogoutAck logoutAck = new LogoutAck();
                myGameAck.ConnAck.LogoutAck = logoutAck;
                logoutAck.Result = -1;
            }

            // 获取玩家数据
            PlayerData playerData = DataMgr.instance.GetPlayerData(id);
            if (playerData == null)
            {

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
