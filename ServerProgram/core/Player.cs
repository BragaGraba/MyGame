using ServerProgram.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerProgram.core.protocol;
using ServerProgram.core.manager;
using ServerProgram.logic;

namespace ServerProgram.core
{
    public class Player
    {
        public string id;

        // 连接类
        public Conn conn;

        // 数据
        public PlayerData data;

        // 临时数据
        public PlayerTempData tempData;

        public Player(string id, Conn conn)
        {
            this.id = id;
            this.conn = conn;
            tempData = new PlayerTempData();
        }

        public void Send(ProtocolBase proto)
        {
            if (conn == null)
                return;
            ServNet.instance.Send(conn, proto);
        }

        public static bool KickOff(string id, ProtocolBase proto)
        {
            Conn[] conns = ServNet.instance.conns;
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null)
                    continue;
                if (!conns[i].isUse)
                    continue;
                if (conns[i].player == null)
                    continue;
                if (conns[i].player.id == id)
                {
                    if (proto != null)
                        conns[i].player.Send(proto);

                    return conns[i].player.Logout();
                }
            }
            return true;
        }

        public bool Logout()
        {
            // 事件处理

            // 保存
            if (!DataMgr.instance.SavePlayer(this))
                return false;

            // 下线
            conn.player = null;
            conn.Close();
            return true;
        }
    }
}
