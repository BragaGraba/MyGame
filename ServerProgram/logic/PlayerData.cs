using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace ServerProgram.logic
{
    [ProtoContract]
    public class PlayerData
    {
        [ProtoMember(1)]
        public int score = 0;
        public PlayerData()
        {
            score = 100;
        }
    }
}
