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
using System.Data.Entity;
using System.Net.Sockets;

namespace GameWebSite.Controllers
{
    public class GamesPoolController : ApiController
    {

        // It is imposible to use RAM-space for information for all requests.
        private static Dictionary<int, System.Timers.Timer> _GamesPool;
        public static Dictionary<int, System.Timers.Timer> GamesPool
        {
            set
            {
                _GamesPool = value;
            }
            get
            {
                if (_GamesPool == null)
                {
                    _GamesPool = new Dictionary<int, System.Timers.Timer>();
                }
                return _GamesPool;
            }
        }

        private static int _PoolID;
        public static int PoolID
        {
            get
            {
                if (_PoolID != int.MaxValue)
                {
                    return ++_PoolID;
                }
                else
                {
                    _PoolID = 1;
                    return _PoolID;
                }
            }
        }

        //[System.Web.Http.Authorize]
        [System.Web.Http.HttpPost]
        public IEnumerable<OnlineGameDTO> OnlineGamesPool(PlayerDTO player)
        {

            string clientAddress = HttpContext.Current.Request.UserHostAddress;

            IEnumerable<OnlineGameDTO> onlineGames;
            using (AppIdentityDbContext context = new AppIdentityDbContext())
            {
                onlineGames = context.OnlineGames.Where(x => x.Guest == null || (player.ID != null && x.Guest.ID == player.ID))
                    .Include(x => x.Guest).Include(x => x.Host).Include(c => c.Host.LocalIPs).ToList();
            }

            if (clientAddress == "::1" || clientAddress.Substring(0, 7) == "192.168" ||
                (clientAddress.Substring(0, 3) == "172" &&
                    Convert.ToInt32(clientAddress.Split('.')[1]) >= 16 && Convert.ToInt32(clientAddress.Split('.')[1]) <= 31)
                || clientAddress.Substring(0, 3) == "10."
                || clientAddress.Substring(0, 3) == "127" 
                || clientAddress.Substring(0, 4) == "fe80")
            {
                onlineGames = onlineGames.Select(x =>
                {
                    OnlineGameDTO newOnlineGame = (OnlineGameDTO)x.Clone();
                    SetIPAddressGlobal(newOnlineGame, player);
                    newOnlineGame.Host.LocalIPs = null;
                    newOnlineGame.Guest = null;
                    return newOnlineGame;
                }).ToList();
            }
            else
            {
                //throw new Exception("Request not from local net: " + clientAddress);
                onlineGames = onlineGames.Select(x =>
                {
                    OnlineGameDTO newOnlineGame = (OnlineGameDTO)x.Clone();
                    if (clientAddress == newOnlineGame.Host.GlobalIP)
                    {
                        SetIPAddressGlobal(newOnlineGame, player);
                    }
                    newOnlineGame.Host.LocalIPs = null;
                    newOnlineGame.Guest = null;
                    return newOnlineGame;
                }).ToList();
            }

            return onlineGames;
        }

        private static void SetIPAddressGlobal(OnlineGameDTO newOnlineGame, PlayerDTO player)
        {
            using (AppIdentityDbContext context = new AppIdentityDbContext())
            {
                newOnlineGame.Host.GlobalIP = newOnlineGame.Host.LocalIPs.FirstOrDefault(IsInSameNet).IP;

                // To debug purposes
                //newOnlineGame.Description = newOnlineGame.Host.GlobalIP;

            }

            bool IsInSameNet (LocalIPDTO IPDto)
            {
                if (IPDto.IP.Substring(0, 7) == "192.168")
                {
                    return player.LocalIPs.Any(x => x.IP.Substring(0, 11) == IPDto.IP.Substring(0, 11));
                }
                else if ((IPDto.IP.Substring(0, 3) == "172" &&
                    Convert.ToInt32(IPDto.IP.Split('.')[1]) >= 16 && Convert.ToInt32(IPDto.IP.Split('.')[1]) <= 31))
                {
                    return player.LocalIPs.Any(x => x.IP.Substring(0, 7) == IPDto.IP.Substring(0, 7));
                }
                else if (IPDto.IP.Substring(0, 3) == "10.")
                {
                    return player.LocalIPs.Any(x => x.IP.Substring(0, 3) == "10");
                }

                return false;
            }
        }

        //[System.Web.Http.Authorize]
        [System.Web.Http.HttpDelete]
        public bool DeleteHostGame(int id)
        {
            try
            {
                DeleteGame(id);
                GamesPool.Remove(GamesPool.FirstOrDefault(x => x.Key == id).Key);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        internal static void DeleteGame(int id)
        {
            using (AppIdentityDbContext context = new AppIdentityDbContext())
            {
                OnlineGameDTO onlineGame = context.OnlineGames.
                    Include(c => c.Host).Include(c => c.Host.LocalIPs).Include(c => c.Guest).
                    FirstOrDefault(x => x.ID == id);
                if (onlineGame != null)
                {
                    //context.Entry(onlineGame).Reference(x => x.Host).Load();
                    //context.Entry(onlineGame).Reference(x => x.Host).Load();
                    //context.Entry(onlineGame).Reference(x => x.Guest).Load();
                    //context.Entry(onlineGame).Reference(x => x.Guest.LocalIPs).Load();
                    if (onlineGame != null)
                    {
                        if (onlineGame.Guest != null)
                        {
                            if (!context.OnlineGames.Any(x => x.ID != onlineGame.ID &&
                                (onlineGame.Guest.ID == x.Host.ID || onlineGame.Guest.ID == x.Guest.ID)))
                            {
                                context.LocalIP.RemoveRange(onlineGame.Guest.LocalIPs);
                                context.Players.Remove(onlineGame.Guest);
                            }
                        }
                        if (!context.OnlineGames.Any(x => x.ID != onlineGame.ID &&
                                (onlineGame.Host.ID == x.Host.ID || onlineGame.Host.ID == x.Guest.ID)))
                        {
                            context.LocalIP.RemoveRange(onlineGame.Host.LocalIPs);
                            context.Players.Remove(onlineGame.Host);
                        }
                        context.OnlineGames.Remove(onlineGame);
                        context.SaveChanges();
                    }
                }
            }
        }

        //[System.Web.Http.Authorize]
        [System.Web.Http.HttpPut]
        public bool PutGame(OnlineGameDTO game)
        {
            using (AppIdentityDbContext context = new AppIdentityDbContext())
            {
                OnlineGameDTO localGame = context.OnlineGames.FirstOrDefault(x => x.ID == game.ID);

                if (localGame != null)
                {
                    if (game.Description != null)
                        localGame.Description = game.Description;
                    if (game.Guest != null)
                    {
                        if (game.Guest.ID == "")
                            localGame.Guest = null;
                        else
                        {
                            PlayerDTO guest = context.Players.FirstOrDefault(x => x.ID == game.Guest.ID);
                            if (guest != null)
                                localGame.Guest = guest;
                            else
                                localGame.Guest = game.Guest;
                        }
                    }
                    context.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
        }

        [System.Web.Http.HttpHead]
        public void MaintainGame(int id)
        {
            KeyValuePair<int, System.Timers.Timer> gamesPoolItem = GamesPool.FirstOrDefault(x => x.Key == id);
            if (gamesPoolItem.Value != null)
            {
                System.Timers.Timer timer = GamesPool.FirstOrDefault(x => x.Key == id).Value;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Start();
                }
            }
            
        }
    }
}
