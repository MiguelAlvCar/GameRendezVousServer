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
        static List<OnlineGameDTO> GamesPool = new List<OnlineGameDTO>();
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
            if (GamesPool.All(x => x.Host.ID != game.Host.ID))
            {
                game.ID = PoolID;
                GamesPool.Add(game);
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
            return GamesPool.Where(x => x.Guest == null || (id != null && x.Guest.ID == id));
        }

        //[System.Web.Http.Authorize]
        [System.Web.Http.HttpDelete]
        public bool DeleteHostGame(int id)
        {
            try
            {
                GamesPool.Remove(GamesPool.First(x => x.ID == id));
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
            OnlineGameDTO localGame = GamesPool.FirstOrDefault(x => x.Host.ID == game.Host.ID);
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

        
    }
}
