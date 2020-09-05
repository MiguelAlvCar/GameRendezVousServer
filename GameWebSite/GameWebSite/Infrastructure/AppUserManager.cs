using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using DTO_Models;
using GameWebSite.Models;

namespace GameWebSite.Infrastructure
{
    public class AppUserManager : UserManager<GameUser>
    {
        public AppUserManager(IUserStore<GameUser> store)
            : base(store)
        { }

        public static AppUserManager Create(
                IdentityFactoryOptions<AppUserManager> options,
                IOwinContext context)
        {

            AppIdentityDbContext db = context.Get<AppIdentityDbContext>();
            AppUserManager manager = new AppUserManager(new UserStore<GameUser>(db));

            return manager;
        }
    }
}