using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Crmf;

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
            string connStr = "Database=game;DataSource=127.0.0.1";
            connStr += "user=root;password=123456;port=3306";

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
            string cmdStr = string.Format("select * from user where id='{0};", id);
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
            PlayerData playerData = new PlayerData();
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CreatePlayer 序列化 " + e.Message);
                return false;
            }

        }
    }
}
