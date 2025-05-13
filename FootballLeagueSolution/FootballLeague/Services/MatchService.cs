using FootballLeague.Data;
using FootballLeague.Models;


namespace FootballLeague.Services
{
    public class MatchService
    {
        private readonly FootballLeagueDbContext _context;

        public MatchService(FootballLeagueDbContext context)
        {
            _context = context;
        }

        public async Task AddMatchAsync(Match match)
        {
            if (match.IdGospodarza == match.IdGoscia)
            {
                throw new ArgumentException("Gospodarz i Gość nie mogą być tym samym klubem.");
            }
            _context.Mecze.Add(match);
            await _context.SaveChangesAsync();
        }
    }
}