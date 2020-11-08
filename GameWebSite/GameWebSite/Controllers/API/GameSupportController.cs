using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using GameWebSite.Infrastructure;
using DTO_Models;
using System.Web;
using System.Threading.Tasks;
using GameWebSite.Models;

namespace GameWebSite.Controllers
{
    public class GamesPoolController : ApiController
    {
        private static int _poolID = 0;
        private static int PoolID
        {
            get
            {
                if (_poolID != int.MaxValue)
                {
                    return ++_poolID;
                }
                else
                {
                    _poolID = 1;
                    return _poolID;
                }
            }
        }

        //[System.Web.Http.Authorize]
        [System.Web.Http.HttpPost]
        public long PostGame(OnlineGameDTO game)
        {
            if (PoolGame.GamesPool.All(x => x.Value.OnlineGame.Host.ID != game.Host.ID))
            {
                game.ID = PoolID;
                game.Host.GlobalIP = HttpContext.Current.Request.UserHostAddress;
                game.CreationTime = DateTime.Now;

                System.Timers.Timer aTimer = new System.Timers.Timer();
                aTimer.Interval = 13000;
                aTimer.Elapsed += (a, b) =>
                {
                    PoolGame.GamesPool.Remove(game.ID);
                };
                aTimer.AutoReset = false;

                PoolGame poolGame = new PoolGame(game, aTimer);
                PoolGame.GamesPool.Add(game.ID, poolGame);
                
                aTimer.Start();

                return game.ID;
            }
            else
            {
                return 0;
            }
        }

        //[System.Web.Http.Authorize]
        [System.Web.Http.HttpGet]
        public IEnumerable<OnlineGameDTO> GetOnlineGamesPool(string id)
        {
            string clientAddress = HttpContext.Current.Request.UserHostAddress;
            IEnumerable<OnlineGameDTO> onlineGames = PoolGame.GamesPool.Where(x => x.Value.OnlineGame.Guest == null || (id != null && x.Value.OnlineGame.Guest.ID == id)).Select(x => x.Value.OnlineGame);
            if (clientAddress == "::1" || clientAddress.Substring(0, 7) == "192.168" || 
                clientAddress.Substring(0, 3) == "172" || clientAddress.Substring(0, 3) == "127")
            {
                onlineGames = onlineGames.Select(x => {
                    OnlineGameDTO newOnlineGame = (OnlineGameDTO)x.Clone();
                    if (newOnlineGame.Host.GlobalIP != "::1" || clientAddress.Substring(0, 3) != "127")
                        newOnlineGame.Host.GlobalIP = newOnlineGame.Host.LocalIP;
                    newOnlineGame.Host.LocalIP = null;
                    newOnlineGame.Guest = null;
                    return newOnlineGame;
                });
            }
            else
            {
                onlineGames = onlineGames.Select(x => {
                    OnlineGameDTO newOnlineGame = (OnlineGameDTO)x.Clone();
                    newOnlineGame.Host.LocalIP = null;
                    newOnlineGame.Guest = null;
                    return newOnlineGame;
                });
            }

            return onlineGames;
        }

        //[System.Web.Http.Authorize]
        [System.Web.Http.HttpDelete]
        public bool DeleteHostGame(int id)
        {
            try
            {
                PoolGame.GamesPool.Remove(PoolGame.GamesPool.First(x => x.Value.OnlineGame.ID == id).Key);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        //[System.Web.Http.Authorize]
        [System.Web.Http.HttpPut]
        public bool PutGame(OnlineGameDTO game)
        {
            OnlineGameDTO localGame = PoolGame.GamesPool.FirstOrDefault(x => x.Value.OnlineGame.Host.ID == game.Host.ID).Value.OnlineGame;
            if (localGame != null)
            {
                if (game.Description != null)
                    localGame.Description = game.Description;
                if (game.Guest != null)
                {
                    if (game.Guest.ID == "")
                        localGame.Guest = null;
                    else
                        localGame.Guest = game.Guest;
                }  
                return true;
            }
            else
                return false;
        }

        [System.Web.Http.HttpHead]
        public void MaintainGame(int id)
        {
            KeyValuePair<int, PoolGame> gamesPoolItem = PoolGame.GamesPool.FirstOrDefault(x => x.Value.OnlineGame.ID == id);
            if (gamesPoolItem.Value != null)
            {
                System.Timers.Timer timer  = PoolGame.GamesPool.FirstOrDefault(x => x.Value.OnlineGame.ID == id).Value.Timer;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Start();
                }
            }
            
        }
    }
}
