using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameWebSite.Models
{
    public class DebugGamesPool
    {
        public int GameID { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public bool IsThereATimer { get; set; }
    }
}