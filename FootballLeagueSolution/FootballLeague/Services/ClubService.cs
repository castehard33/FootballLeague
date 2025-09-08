using Microsoft.EntityFrameworkCore;
using FootballLeague.Data;
using FootballLeague.Models;


namespace FootballLeague.Services
{
    public class ClubService
    {
        private readonly FootballLeagueDbContext _context;

        public ClubService(FootballLeagueDbContext context)
        {
            _context = context;
        }

        public async Task<List<Club>> GetClubsAsync() => await _context.Kluby.OrderBy(c => c.Nazwa).ToListAsync();

        public async Task<List<Club>> GetClubsWithoutCoachAsync()
        {
   
            var clubsWithCoachIds = await _context.TrenerzyKlubow
                                                 .Where(a => a.DataZwolnienia == null)
                                                 .Select(a => a.IDklubu)
                                                 .Distinct()
                                                 .ToListAsync();

            var clubsWithoutCoach = await _context.Kluby
                                                 .Where(c => !clubsWithCoachIds.Contains(c.IdKlubu))
                                                 .OrderBy(c => c.Nazwa)
                                                 .ToListAsync();

            return clubsWithoutCoach;
        }
    }
}