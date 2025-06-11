using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballLeague.Models
{
    public class Transfer
    {
        [Key]
        public int IDtransferu { get; set; } 

        [Required]
        public int IDzawodnika { get; set; }
        [ForeignKey("IDzawodnika")]
        public virtual Player? Zawodnik { get; set; }

        [Required]
        public int IDklubu { get; set; }
        [ForeignKey("IDklubu")]
        public virtual Club? Klub { get; set; }

        [Required]
        public DateTime DataDolaczenia { get; set; }

        public DateTime? DataOdejscia { get; set; } 

        public Transfer()
        {
            DataDolaczenia = DateTime.Today;
        }
    }
}