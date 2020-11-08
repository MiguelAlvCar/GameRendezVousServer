using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using DTO_Models;
using GameWebSite.Infrastructure;
using GameWebSite.Models;

namespace GameWebSite.Controllers
{
    public class BattlesController : ApiController
    {
        [System.Web.Http.HttpGet]
        public IEnumerable<DebugGamesPool> GetPoolGames()
        {
            return PoolGame.GamesPool.Select(x =>
            {
                string player2 = "";
                if (x.Value.OnlineGame.Guest != null)
                    player2 = x.Value.OnlineGame.Guest.Name;
                return new DebugGamesPool() { GameID = x.Key, Player1 = x.Value.OnlineGame.Host.Name, Player2 = player2, IsThereATimer = x.Value.Timer != null };
            });
        }

        [System.Web.Http.HttpGet]
        public bool Restart(int id)
        {
            PoolGame.GamesPool = new Dictionary<int, PoolGame>();
            return true;
        }

        [System.Web.Http.HttpPost]
        public void Victory(BattleDTO battleDTO)
        {
            using (AppIdentityDbContext context = new AppIdentityDbContext())
            {
                IEnumerable<GameUser> users = context.Users.Where(x => x.Id == battleDTO.Player1ID || x.Id == battleDTO.Player2ID);

                GameUser player1 = users.First(x => x.Id == battleDTO.Player1ID);
                GameUser player2 = users.First(x => x.Id == battleDTO.Player2ID);
                Battle battle = new Battle(player1, player2, battleDTO.Army1, battleDTO.Army2, battleDTO.Points1, battleDTO.Points2, battleDTO.Result);
                battle.Width = battleDTO.Width;
                battle.Length = battleDTO.Length;
                battle.BattleID = context.Battles.Count();
                context.Battles.Add(battle);

                context.SaveChanges();

                IEnumerable<GameUser> users1 = context.Users.Where(x => x.Id == battleDTO.Player1ID || x.Id == battleDTO.Player2ID);
                foreach (GameUser user in users1)
                {
                    double numberOfBattles = context.Battles.Where(x => x.Player1.Id == user.Id || x.Player2.Id == user.Id).Count();
                    double wonBattles = context.Battles.Where(x => (x.Player1.Id == user.Id && x.Result > 0)
                                || (x.Player2.Id == user.Id && x.Result < 0)).Count();
                    double ratioWonLost = wonBattles / (numberOfBattles - wonBattles);

                    double horizontalAsymptote;
                    if (ratioWonLost < 1)
                    {
                        horizontalAsymptote = 4 * ratioWonLost - 1;
                        if (horizontalAsymptote <= 0)
                            horizontalAsymptote = 0.0001;
                    }
                    else
                    {
                        if (ratioWonLost > 3.5)
                        {
                            ratioWonLost = 3.5;
                            numberOfBattles = wonBattles * 2/7 + wonBattles;
                            horizontalAsymptote = 5;
                        }
                        else
                        {
                            horizontalAsymptote = 0.8 * ratioWonLost + 2.2;
                        }
                    }
                    double ability;
                    if (horizontalAsymptote < 1)
                        ability = ( (horizontalAsymptote - 2) / (1 + (1 - horizontalAsymptote) * Math.Pow(Math.E, -(numberOfBattles / 3))) ) + 2;
                    else
                        ability = horizontalAsymptote / ( 1 + (horizontalAsymptote - 1) * Math.Pow( Math.E, -(numberOfBattles / 3) ) );

                    user.Ability = ability;
                }

                context.SaveChanges();
            }
        }
    }
}