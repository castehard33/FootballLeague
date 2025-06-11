using System.ComponentModel.DataAnnotations;

namespace FootballLeague.Models
{
    public class Position
    {
        [Key]
        public int IDpozycji { get; set; } 

        [Required(ErrorMessage = "Nazwa pozycji jest wymagana.")]
        [StringLength(30, ErrorMessage = "Nazwa pozycji nie może przekraczać 30 znaków.")]
        public string NazwaPozycji { get; set; }

        public Position()
        {
            NazwaPozycji = string.Empty;
        }
    }
}