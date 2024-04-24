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

            MyGameAck myGameAck = GetMyGameAck();
            myGameAck.ConnAck.RegisterAck = ack;

            SendMsg(conn, myGameAck);
        }

        private MyGameAck GetMyGameAck()
        {
            MyGameAck ack = new MyGameAck();
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
