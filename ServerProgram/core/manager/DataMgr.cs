using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Protobuf;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Crmf;
using ProtoBuf;
using ServerProgram.logic;

namespace ServerProgram.core.manager
{
    public class DataMgr
    {
        MySqlConnection sqlConn;

        public static DataMgr instance;
        public DataMgr() 
        {
            instance = this;
            Connect();
        }

        // 连接
        public void Connect()
        {
            string connStr = "Database=game;Data Source=127.0.0.1;";
            connStr += "Server=localhost;User=root;Password=123456;port=3306";

            sqlConn = new MySqlConnection(connStr);
            try
            {
                sqlConn.Open();
            }
            catch (Exception ex) 
            {
                Console.WriteLine("[DataMgr]Connect " + ex.Message);
                return;
            }
        }

        public bool IsSafeStr(string str)
        {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
        }

        // 是否存在该用户
        private bool CanRegister(string id)
        {
            // 防sql注入
            if (!IsSafeStr(id))
                return false;
            // 查询id是否存在
            string cmdStr = string.Format("select * from user where id='{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                return !hasRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CanRegister fail " + e.Message);
                return false;
            }
        }

        // 注册
        public bool Register(string id, string pw)
        {
            // 防sql注入
            if (!IsSafeStr(id) || !IsSafeStr(pw))
            {
                Console.WriteLine("[DataMgr]Register 使用非法字符");
                return false;
            }

            // 能否注册
            if (!CanRegister(id))
            {
                Console.WriteLine("[DataMgr]Register !CanRegister");
                return false;
            }

            // 写入user数据表
            string cmdStr = string.Format("insert into user set id = '{0}', pw = '{1}';", id, pw);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]Register " + e.Message);
                return false;
            }
        }

        // 创建角色
        // 将默认的playerData对象序列化成二进制数据，并保存到player表的data栏下
        public bool CreatePlayer(string id)
        {
            // 防sql注入
            if (!IsSafeStr(id))
                return false;

            // 序列化
            MemoryStream memoryStream = new MemoryStream();
            PlayerData playerData = new PlayerData();
            try
            {
                // using 语句可以确保在代码执行完毕之后，某些实现了IDisposable接口的对象能够正确地释放其占用的资源（如文件句柄、数据库连接等）
                // using语句会自动调用对象的Dispose方法。
                // 考虑生命周期，此处暂不适用此方法。
                //using (MemoryStream memoryStream = new MemoryStream())
                //{
                //    Serializer.Serialize(memoryStream, playerData);
                //}
                
                Serializer.Serialize(memoryStream, playerData);
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CreatePlayer 序列化 " + e.Message);
                return false;
            }

            // 可以将memoryStream.ToArray() 或 memorySystem.GetBuffer() 用于存储或传输
            byte[] serializedData = memoryStream.ToArray();

            // 写入数据库
            string cmdStr = string.Format("insert into player set id = '{0}', data = @data;", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters[0].Value = serializedData;
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CreatePlayer 写入 " + e.Message);
                return false;
            }
        }

        // 登录校验
        public bool CheckPassWord(string id, string pw)
        {
            // 防sql注入
            if (!IsSafeStr(id))
                return false;
            // 查询
            string cmdStr = string.Format("select * from user where id = '{0}' and pw = '{1}';", id, pw);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);

            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                return hasRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CheckPassWord " + e.Message);
                return false;
            }
        }

        // 获取角色数据
        // 查询 & 反序列化
        public PlayerData GetPlayerData(string id)
        {
            PlayerData playerData = null;
            // 防sql注入
            if (!IsSafeStr(id))
                return playerData;
            // 查询
            string cmdStr = string.Format("select * from player where id = '{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            byte[] buffer = new byte[1];
            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (!dataReader.HasRows)
                {
                    return playerData;
                }
                dataReader.Read();

                // 将缓冲区设置为null，只为获取数据长度
                long len = dataReader.GetBytes(1, 0, null, 0, 0);
                buffer = new byte[len];
                dataReader.GetBytes(1, 0, buffer, 0, (int)len);
                dataReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]GetPlayerData 查询 " + e.Message);
                return playerData;
            }

            // 反序列化
            MemoryStream memoryStream = new MemoryStream(buffer);
            try
            {
                playerData = Serializer.Deserialize<PlayerData>(memoryStream);
                return playerData;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]GetPlayerData 反序列化 " + e.Message);
                return playerData;
            }
        }

        // 保存角色
        public bool SavePlayer(Player player)
        {
            string id = player.id;
            PlayerData playerData = player.data;

            // 序列化
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                Serializer.Serialize(memoryStream, playerData);
            }
            catch (Exception e ) 
            {
                Console.WriteLine("[DataMgr]SavePlayer 序列化 " + e.Message);
                return false;
            }

            byte[] byteArr = memoryStream.ToArray();

            string cmdStr = string.Format("update player set data = @data where id = '{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters[0].Value = byteArr;
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]SavePlayer 写入 " + e.Message);
                return false;
            }
        }
    }
}
