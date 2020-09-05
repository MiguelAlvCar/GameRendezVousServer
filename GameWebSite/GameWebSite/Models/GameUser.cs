using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GameWebSite.Models
{
    public class GameUser : IdentityUser
    {
        public double Ability { get; set; }
        public DateTime CreationTime { get; set; }
        public string ComputerID { get; set; }
    }
}
