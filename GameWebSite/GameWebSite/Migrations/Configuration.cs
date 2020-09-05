namespace GameWebSite.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using DTO_Models;
    using GameWebSite.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<GameWebSite.Infrastructure.AppIdentityDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "GameWebSite.Infrastructure.AppIdentityDbContext";
        }

        protected override void Seed(GameWebSite.Infrastructure.AppIdentityDbContext context)
        {
            //context.Battles.Add(new Models.Battle(context.Users.First(x => x.UserName == "Bob"), context.Users.First(x => x.UserName == "Alice")
                //,2 ,3,20,25,10,12,2.4));
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
