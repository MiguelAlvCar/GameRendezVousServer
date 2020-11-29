using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using DTO_Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using GameWebSite.Models;

namespace GameWebSite.Infrastructure
{
    public class AppIdentityDbContext : IdentityDbContext<GameUser>
    {
        public DbSet<Battle> Battles { get; set; }
        public DbSet<OnlineGameDTO> OnlineGames { get; set; }
        public DbSet<PlayerDTO> Players { get; set; }
        public DbSet<LocalIPDTO> LocalIP { get; set; }
        public AppIdentityDbContext() : base("IdentityDb") {}

        static AppIdentityDbContext()
        {
            Database.SetInitializer<AppIdentityDbContext>(new IdentityDbInit());
        }

        public static AppIdentityDbContext Create()
        {
            return new AppIdentityDbContext();
        }
    }

    public class IdentityDbInit
            : DropCreateDatabaseIfModelChanges<AppIdentityDbContext>
    {
        
        protected override void Seed(AppIdentityDbContext context)
        {
            PerformInitialSetup(context);
            base.Seed(context);
        }

        public void PerformInitialSetup(AppIdentityDbContext context)
        {
            AppUserManager userMgr = new AppUserManager(new UserStore<GameUser>(context));
            AppRoleManager roleMgr = new AppRoleManager(new RoleStore<GameRole>(context));

            string roleName = "Administrators";
            string userName = "Admin";
            string password = "aaaaaa";
            string email = "admin@example.com";

            if (!roleMgr.RoleExists(roleName))
            {
                roleMgr.Create(new GameRole(roleName));
            }

            GameUser user = userMgr.FindByName(userName);
            if (user == null)
            {
                userMgr.Create(new GameUser { UserName = userName, Email = email, CreationTime = DateTime.Now },
                    password);
                user = userMgr.FindByName(userName);
            }

            if (!userMgr.IsInRole(user.Id, roleName))
            {
                userMgr.AddToRole(user.Id, roleName);
            }
        }
    }
}