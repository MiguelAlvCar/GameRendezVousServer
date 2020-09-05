using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using GameWebSite.Models;

namespace GameWebSite.Infrastructure
{
    public class BattlesClaimProvider
    {
        public static IEnumerable<Claim> GetClaims(ClaimsIdentity identityUser, GameUser gameUser)
        {
            List<Claim> claims = new List<Claim>();
            AppIdentityDbContext context = new AppIdentityDbContext();
            if (context.Battles.Where(x => x.Player1.Id == gameUser.Id || x.Player2.Id == gameUser.Id).Count() > 0 )
            {
                claims.Add(new Claim("NumberOfBattles", "Min1"));
            }
            return claims;
        }

        private static Claim CreateClaim(string type, string value)
        {
            return new Claim(type, value, ClaimValueTypes.String);
        }
    }
}