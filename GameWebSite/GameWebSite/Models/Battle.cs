using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace GameWebSite.Models
{
    public class Battle
    {
        public Battle(GameUser player1, GameUser player2, int army1, int army2, int points1, int points2, double result)
        {
            Player1 = player1;
            Player2 = player2;
            Army1 = army1;
            Army2 = army2;
            Points1 = points1;
            Points2 = points2;
            Result = result;
            CreationTime = DateTime.Now;
        }
        
        public int BattleID { get; set; }
        //[Required]
        public GameUser Player1 { get; set; }
        //[Required]
        public GameUser Player2 { get; set; }
        [Required]
        public int Army1 { get; set; }
        [Required]
        public int Army2 { get; set; }
        [Required]
        public int Points1 { get; set; }
        [Required]
        public int Points2 { get; set; }
        [Required]
        public int Width { get; set; }
        [Required]
        public int Length { get; set; }
        [Required]
        public double Result { get; set; }
        [Required]
        public DateTime CreationTime { get; set; }
    }
}
