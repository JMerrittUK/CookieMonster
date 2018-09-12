using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookieMonster.Structs
{
    [Serializable]
    public struct Config
    {
        public string token;
        public ulong botID;
        public string botPictureURL;
        public bool displayAllGames;
        public int totalGamesToDisplay;
        public int cookieDropChance;
        public BotColor color;
    }

    public struct BotColor
    {
        public byte r;
        public byte g;
        public byte b;
    }
}
