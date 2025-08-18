using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballLeague.Models
{
    public class Coach
    {
        [Key]
        public int IDtrenera { get; set; }

        [Required]
        [StringLength(50)]
        public string Imie { get; set; }

        [Required]
        [StringLength(50)]
        public string Nazwisko { get; set; }

        [Required]
        [StringLength(30)]
        public string Licencja { get; set; }

        [NotMapped]
        public Club? AktualnyKlub { get; set; }

        [NotMapped]
        public string PelneNazwisko => $"{Imie} {Nazwisko}";

        public Coach()
        {
            Imie = string.Empty;
            Nazwisko = string.Empty;
            Licencja = string.Empty;
        }
    }
}
