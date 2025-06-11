using FootballLeague.Data;
using FootballLeague.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootballLeague.Services
{
    public class PositionService(FootballLeagueDbContext context)
    {
        private readonly FootballLeagueDbContext _context = context;

        public async Task<List<Position>> GetPositionsAsync() => await _context.Pozycje.OrderBy(p => p.NazwaPozycji).ToListAsync();

    }
}