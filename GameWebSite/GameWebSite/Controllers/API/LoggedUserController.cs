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
    public class LoggedUserController : ApiController
    {
        [System.Web.Http.HttpPost]
        public async Task<LoginDTO> Login(LoginModel details)
        {
            GameUser user;
            try
            {
                user = await UserManager.FindAsync(details.Name, details.Password);
            }
            catch (Exception e)
            {
                throw e;
            }
            

            if (user != null)
            {
                ClaimsIdentity ident = await UserManager.CreateIdentityAsync(user,
                    DefaultAuthenticationTypes.ApplicationCookie);
                ident.AddClaims(BattlesClaimProvider.GetClaims(ident, user));
                AuthManager.SignOut();
                AuthManager.SignIn(new AuthenticationProperties
                {
                    IsPersistent = false
                }, ident);

                LoginDTO loginDTO = new LoginDTO(true);
                PlayerDTO player = new PlayerDTO(user.UserName, user.Ability, user.Id.ToString()) { Email = user.Email};
                player.GlobalIP = HttpContext.Current.Request.UserHostAddress;

                using (AppIdentityDbContext context = new AppIdentityDbContext())
                {
                    player.Battles = context.Battles.Where(x => x.Player1.Id == user.Id || x.Player2.Id == user.Id).Count();
                    player.WonBattles = context.Battles.Where(x => (x.Player1.Id == user.Id && x.Result > 0)
                                    || (x.Player2.Id == user.Id && x.Result < 0)).Count();
                }

                loginDTO.User = player;
                return loginDTO;
            }
            return new LoginDTO(false);
        }

        [System.Web.Http.HttpPut]
        public async Task<bool> Edit(EditUserDTO editUser)
        {
            GameUser user = await UserManager.FindByIdAsync(editUser.ID);
            if (!string.IsNullOrEmpty(editUser.NewPassword))
            {
                GameUser user1 = await UserManager.FindAsync(user.UserName, editUser.OldPassword);
                if (user1 != null)
                {
                    IdentityResult validPass = null;
                    if (editUser.NewPassword != string.Empty)
                    {
                        validPass = await UserManager.PasswordValidator.ValidateAsync(editUser.NewPassword);
                        if (validPass.Succeeded)
                        {
                            user.PasswordHash = UserManager.PasswordHasher.HashPassword(editUser.NewPassword);
                        }
                    }
                }
                else return false;
            }

            if (!string.IsNullOrEmpty(user.Email))
            {
                IdentityResult validEmail
                = await UserManager.UserValidator.ValidateAsync(user);
                if (validEmail.Succeeded)
                {
                    user.Email = editUser.NewEmail;
                }
            }

            if (!string.IsNullOrEmpty(editUser.NewName))
            {
                user.UserName = editUser.NewName;
            }
            UserManager.Update(user);
            return true;
        }

        private IAuthenticationManager AuthManager
        {
            get
            {
                return Request.GetOwinContext().Authentication;
            }
        }

        private AppUserManager UserManager
        {
            get
            {
                return Request.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }
    }
}
