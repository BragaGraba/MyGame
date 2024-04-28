using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using System.Reflection;
using MyGameProto;

namespace ServerProgram.core.protocol
{
    public class ProtocolProtobuf<T> : ProtocolBase where T : IMessage, new()
    {
        private static readonly ConcurrentDictionary<Type, MessageParser> Parsers = new ConcurrentDictionary<Type, MessageParser>();
        private T _message;
        public ProtocolProtobuf()
        {
            _message = new T();

            RegisterMessageType();
        }

        public ProtocolProtobuf(T message)
        {
            _message = message;
        }

        // 注册消息类型，初始化解析器
        public static void RegisterMessageType()
        {
            var type = typeof(T);
            if (!Parsers.ContainsKey(type))
            {
                var parserPropertyInfo = type.GetProperty("Parser", BindingFlags.Public | BindingFlags.Static);
                if (parserPropertyInfo != null)
                {
                    var parser = (MessageParser)parserPropertyInfo.GetValue(null);
                    Parsers[type] = parser;
                }
                else
                {
                    throw new InvalidOperationException("The type " + type.FullName + "does not have a public static property named 'Parser' of type 'MessageParser'.");
                }
            }
        }

        // 解码器
        public override ProtocolBase Decode(byte[] readbuff, int start, int length)
        {
            try
            {
                _message = (T)Parsers[typeof(T)].ParseFrom(readbuff, start, length);
                return this;
            }
            catch (Exception e)
            {
                Console.WriteLine("Decode error " + typeof(T).Name + e.Message);
                return null;
            }
        }

        // 编码器
        public override byte[] Encode()
        {
            return _message.ToByteArray();
        }

        public T GetMessage()
        {
            return _message;
        }

        public override string GetName()
        {
            return typeof(T).Name + "Protocol";
        }

        public override string GetDesc()
        {
            return "Protocol for " + typeof(T).Name + " messages";
        }
    }
}
