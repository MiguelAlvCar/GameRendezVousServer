using Microsoft.AspNet.Identity.EntityFramework;

namespace GameWebSite.Models
{
    public class GameRole : IdentityRole
    {
        public GameRole() : base() { }

        public GameRole(string name) : base(name) { }
    }
}