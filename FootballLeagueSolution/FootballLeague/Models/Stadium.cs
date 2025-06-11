using System.ComponentModel.DataAnnotations; 

namespace FootballLeague.Models
{
    public class Stadium
    {
       
        [Key] 
        public int IDstadionu { get; set; }

        
        [Required(ErrorMessage = "Nazwa stadionu jest wymagana.")]
        [StringLength(100, ErrorMessage = "Nazwa stadionu nie może przekraczać 100 znaków.")]
        public string Nazwa { get; set; }

        [Required(ErrorMessage = "Miasto jest wymagane.")]
        [StringLength(60, ErrorMessage = "Nazwa miasta nie może przekraczać 60 znaków.")]
        public string Miasto { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Pojemność nie może być ujemna.")]
        public int? Pojemnosc { get; set; }


        
        public Stadium()
        {
            Nazwa = string.Empty; 
            Miasto = string.Empty; 
        }
    }
}