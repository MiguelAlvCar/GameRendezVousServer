using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GameWebSite.Models
{
    public class RoleEditModel
    {
        public GameRole Role { get; set; }
        public IEnumerable<GameUser> Members { get; set; }
        public IEnumerable<GameUser> NonMembers { get; set; }
    }

    public class RoleModificationModel
    {
        [Required]
        public string RoleName { get; set; }
        public string[] IdsToAdd { get; set; }
        public string[] IdsToDelete { get; set; }
    }
}
