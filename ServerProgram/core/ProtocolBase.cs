using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerProgram.core
{
    // 协议基类
    public class ProtocolBase
    {
        // 解码器，解码readBuff中从start开始length字节
        public virtual ProtocolBase Decode(byte[] readbuff, int start, int length)
        {
            return new ProtocolBase();
        }

        // 编码器，返回byte数组
        public virtual byte[] Encode()
        {
            return new byte[] { };
        }

        // 协议名称，用于消息分发
        // 会把不同协议名称的协议交给不同的函数处理
        public virtual string GetName()
        {
            return "";
        }

        // 描述
        public virtual string GetDesc()
        {
            return "";
        }
    }
}
