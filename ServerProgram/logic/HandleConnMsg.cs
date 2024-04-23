using ServerProgram.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerProgram.core;
using ServerProgram.core.protocol;
using MyGameProto;

namespace ServerProgram.logic
{
    public partial class HandleConnMsg
    {
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
        public void MsgRegister(Conn conn, ProtocolBase protoBase)
        {
            // 获取数值
            

        }
    }
}
