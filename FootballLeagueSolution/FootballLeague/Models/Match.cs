using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballLeague.Models
{
    public class Match 
    {
        public int IdMeczu { get; set; }
        public DateTime DataMeczu { get; set; }

        public int IdGospodarza { get; set; }
        
        [ForeignKey("IdGospodarza")]
        public virtual Club? Gospodarz { get; set; } 

        public int IdGoscia { get; set; }
        [ForeignKey("IdGoscia")]
        public virtual Club? Gosc { get; set; }    

        public byte? BramkiGospodarza { get; set; }
        public byte? BramkiGoscia { get; set; }

        [NotMapped]
        public bool CzyRozegrany => BramkiGospodarza.HasValue && BramkiGoscia.HasValue;
    }
}