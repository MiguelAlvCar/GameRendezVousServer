using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using GameWebSite.Infrastructure;
using DTO_Models;
using System.Data.Entity;
using System.Net.Sockets;

namespace GameWebSite.Controllers.API
{
    public class GamesPool2Controller : ApiController
    {
        //[System.Web.Http.Authorize]
        [System.Web.Http.HttpPost]
        public long PostGame(OnlineGameDTO game)
        {
            game.ID = GamesPoolController.PoolID;
            game.Host.GlobalIP = HttpContext.Current.Request.UserHostAddress;
            game.CreationTime = DateTime.Now;

            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Interval = 13000;
            aTimer.Elapsed += (a, b) =>
            {
                GamesPoolController.DeleteGame(game.ID);
                GamesPoolController.GamesPool.Remove(game.ID);
            };
            aTimer.AutoReset = false;

            using (AppIdentityDbContext context = new AppIdentityDbContext())
            {
                IEnumerable<LocalIPDTO> localIPS = game.Host.LocalIPs;
                

                PlayerDTO host = context.Players.FirstOrDefault(x => x.ID == game.Host.ID) ;
                if (host != null)
                {
                    game.Host = host;
                }

                OnlineGameDTO onlineGame = context.OnlineGames.Add(game);
                context.SaveChanges();
            }
            GamesPoolController.GamesPool.Add(game.ID, aTimer);

            aTimer.Start();

            return game.ID;
        }
    }
}