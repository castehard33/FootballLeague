using Microsoft.EntityFrameworkCore;
using FootballLeague.Data;
using FootballLeague.Models;


namespace FootballLeague.Services 
{
    public class LeagueTableService
    {
        private readonly FootballLeagueDbContext _context;

        public LeagueTableService(FootballLeagueDbContext context)
        {
            _context = context;
        }

        public async Task<List<LeagueTableEntry>> GetLeagueTableAsync()
        {
            var kluby = await _context.Kluby.ToListAsync();
            
            var mecze = await _context.Mecze
                                      .Where(m => m.BramkiGospodarza.HasValue && m.BramkiGoscia.HasValue)
                                      .ToListAsync();

            var tableEntries = new List<LeagueTableEntry>();

            foreach (var klub in kluby)
            {
                var entry = new LeagueTableEntry
                {
                    KlubId = klub.IdKlubu,
                    NazwaKlubu = klub.Nazwa

                };

               
                var meczeKlubu = mecze.Where(m => m.IdGospodarza == klub.IdKlubu || m.IdGoscia == klub.IdKlubu);

                foreach (var mecz in meczeKlubu)
                {
                    entry.MeczeRozegrane++;
                    bool isGospodarz = mecz.IdGospodarza == klub.IdKlubu;

                    
                    if (mecz.BramkiGospodarza.HasValue && mecz.BramkiGoscia.HasValue)
                    {
                        if (isGospodarz)
                        {
                            entry.BramkiZdobyte += mecz.BramkiGospodarza.Value;
                            entry.BramkiStracone += mecz.BramkiGoscia.Value;
                            if (mecz.BramkiGospodarza > mecz.BramkiGoscia) entry.Zwyciestwa++;
                            else if (mecz.BramkiGospodarza < mecz.BramkiGoscia) entry.Porazki++;
                            else entry.Remisy++;
                        }
                        else //gość
                        {
                            entry.BramkiZdobyte += mecz.BramkiGoscia.Value;
                            entry.BramkiStracone += mecz.BramkiGospodarza.Value;
                            if (mecz.BramkiGoscia > mecz.BramkiGospodarza) entry.Zwyciestwa++;
                            else if (mecz.BramkiGoscia < mecz.BramkiGospodarza) entry.Porazki++;
                            else entry.Remisy++;
                        }
                    }
                }
                tableEntries.Add(entry);
            }

            return tableEntries
                .OrderByDescending(e => e.Punkty)
                .ThenByDescending(e => e.RoznicaBramek)
                .ThenByDescending(e => e.BramkiZdobyte)
                .ThenBy(e => e.NazwaKlubu)
                .ToList();
        }
    }
}