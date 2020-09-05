using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using DTO_Models;
using GameWebSite.Models;

namespace GameWebSite.Infrastructure
{

    public class AppRoleManager : RoleManager<GameRole>, IDisposable
    {

        public AppRoleManager(RoleStore<GameRole> store)
            : base(store)
        {
        }

        public static AppRoleManager Create(
                IdentityFactoryOptions<AppRoleManager> options,
                IOwinContext context)
        {
            return new AppRoleManager(new
                RoleStore<GameRole>(context.Get<AppIdentityDbContext>()));
        }
    }
}