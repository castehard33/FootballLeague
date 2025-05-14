namespace FootballLeague.Models
{
    public class LeagueTableEntry
    {
        public int KlubId { get; set; }
        public required string NazwaKlubu { get; set; }
        public int MeczeRozegrane { get; set; }
        public int Zwyciestwa { get; set; }
        public int Remisy { get; set; }
        public int Porazki { get; set; }
        public int BramkiZdobyte { get; set; }
        public int BramkiStracone { get; set; }
        public int RoznicaBramek => BramkiZdobyte - BramkiStracone;
        public int Punkty => (Zwyciestwa * 3) + Remisy;


        public bool IsOddRow { get; set; }

        public int Position { get; set; }
    }
}