using ServerProgram.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
