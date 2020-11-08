using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DTO_Models;

namespace GameWebSite.Models
{
    public class PoolGame
    {
        public static Dictionary<int, PoolGame> GamesPool = new Dictionary<int, PoolGame>();

        internal PoolGame(OnlineGameDTO onlineGame, System.Timers.Timer timer)
        {
            OnlineGame = onlineGame;
            Timer = timer;
        }
        internal OnlineGameDTO OnlineGame { get; set; }
        internal System.Timers.Timer Timer { get; set; }
    }
}