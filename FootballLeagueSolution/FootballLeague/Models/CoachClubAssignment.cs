using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballLeague.Models
{
    public class CoachClubAssignment
    {
        [Key]
        public int IDrelacji { get; set; }

        [Required]
        public int IDtrenera { get; set; }
        [ForeignKey("IDtrenera")]
        public virtual Coach? Trener { get; set; }

        [Required]
        public int IDklubu { get; set; }
        [ForeignKey("IDklubu")]
        public virtual Club? Klub { get; set; }

        [Required]
        public DateTime DataZatrudnienia { get; set; }

        public DateTime? DataZwolnienia { get; set; }

        public CoachClubAssignment()
        {
            DataZatrudnienia = DateTime.Today;
        }
    }
}