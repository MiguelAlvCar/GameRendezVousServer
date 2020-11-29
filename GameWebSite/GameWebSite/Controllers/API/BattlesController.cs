using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using DTO_Models;
using GameWebSite.Infrastructure;
using GameWebSite.Models;
using System.Web;

namespace GameWebSite.Controllers
{
    public class BattlesController : ApiController
    {
        [System.Web.Http.HttpGet]
        public IEnumerable<DebugGamesPool> GetPoolGames()
        {
            using (AppIdentityDbContext context = new AppIdentityDbContext())
            {
                var onlineGames = context.OnlineGames.Include(x => x.Guest).Include(x => x.Host).ToList();
                List<DebugGamesPool> list = GamesPoolController.GamesPool.Select(x =>
                {
                    string player2 = "";

                    OnlineGameDTO onlineGame;

                    onlineGame = onlineGames.FirstOrDefault(y => y.ID == x.Key);

                    if (onlineGame.Guest != null)
                        player2 = onlineGame.Guest.Name;
                    return new DebugGamesPool() { GameID = x.Key, Player1 = onlineGame.Host.Name, Player2 = player2, IsThereATimer = x.Value != null };
                }).ToList();
                return list;
            }
        }

        [Authorize(Roles = "Administrators")]
        [System.Web.Http.HttpGet]
        public IEnumerable<DebugGamesPool> Restart(string id)
        {
            if (id == "res")
            {
                using (AppIdentityDbContext context = new AppIdentityDbContext())
                {
                    foreach (int onlineGameID in context.OnlineGames.Select(x => x.ID))
                        GamesPoolController.DeleteGame(onlineGameID);
                }
                GamesPoolController.GamesPool = new Dictionary<int, System.Timers.Timer>();                
            }
            else if (id == "ip")
            {
                List<DebugGamesPool> list = new List<DebugGamesPool>();
                list.Add(new DebugGamesPool() { Player1 = HttpContext.Current.Request.UserHostAddress });
                return list;
            }
            else if (id == "bat")
            {
                using (AppIdentityDbContext context = new AppIdentityDbContext())
                {
                    var onlineGames = context.OnlineGames.Include(x => x.Guest).Include(x => x.Host).ToList();
                    List<DebugGamesPool> list = onlineGames.Select(x =>
                    {
                        string player2 = "";

                        if (x.Guest != null)
                            player2 = x.Guest.Name;
                        return new DebugGamesPool() { Player1 = x.Host.Name, Player2 = player2};
                    }).ToList();
                    return list;
                }
            }
            return new List<DebugGamesPool>();
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