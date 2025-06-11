using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballLeague.Models
{
    public class Player
    {
        [Key]
        public int IDzawodnika { get; set; } 

        [Required(ErrorMessage = "Imię zawodnika jest wymagane.")]
        [StringLength(50)]
        public string Imie { get; set; }

        [Required(ErrorMessage = "Nazwisko zawodnika jest wymagane.")]
        [StringLength(50)]
        public string Nazwisko { get; set; }

        [Required(ErrorMessage = "Pozycja jest wymagana.")]
        public int IDpozycji { get; set; } // FK

        [ForeignKey("IDpozycji")]
        public virtual Position? Pozycja { get; set; }


        [NotMapped] 
        public Club? AktualnyKlub { get; set; }

        [NotMapped]
        public string PelneNazwisko => $"{Imie} {Nazwisko}";

        public Player()
        {
            Imie = string.Empty;
            Nazwisko = string.Empty;
        }
    }
}