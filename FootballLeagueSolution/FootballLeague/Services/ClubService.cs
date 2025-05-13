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
    }
}