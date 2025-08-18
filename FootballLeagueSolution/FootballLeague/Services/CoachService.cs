using FootballLeague.Data;
using FootballLeague.Models;
using Microsoft.EntityFrameworkCore;


namespace FootballLeague.Services
{
    public class CoachService
    {
        private readonly FootballLeagueDbContext _context;

        public CoachService(FootballLeagueDbContext context)
        {
            _context = context;
        }

        public async Task<List<Coach>> GetCoachesAsync()
        {
            var coaches = await _context.Trenerzy
                                      .OrderBy(c => c.Nazwisko).ThenBy(c => c.Imie)
                                      .ToListAsync();

            var coachIds = coaches.Select(c => c.IDtrenera).ToList();
            var activeAssignments = await _context.TrenerzyKlubow
                                                  .Where(a => coachIds.Contains(a.IDtrenera) && a.DataZwolnienia == null)
                                                  .Include(a => a.Klub)
                                                  .OrderByDescending(a => a.DataZatrudnienia)
                                                  .ToListAsync();

            var latestAssignmentsByCoachId = activeAssignments
                .GroupBy(a => a.IDtrenera)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var coach in coaches)
            {
                if (latestAssignmentsByCoachId.TryGetValue(coach.IDtrenera, out var assignment))
                {
                    coach.AktualnyKlub = assignment.Klub;
                }
                else
                {
                    coach.AktualnyKlub = null;
                }
            }
            return coaches;
        }

        public async Task<Coach?> GetCoachByIdAsync(int coachId)
        {
            var coach = await _context.Trenerzy.FindAsync(coachId);
            if (coach != null)
            {
                var assignment = await _context.TrenerzyKlubow
                                               .Where(a => a.IDtrenera == coachId && a.DataZwolnienia == null)
                                               .OrderByDescending(a => a.DataZatrudnienia)
                                               .Include(a => a.Klub)
                                               .FirstOrDefaultAsync();
                if (assignment != null)
                {
                    coach.AktualnyKlub = assignment.Klub;
                }
            }
            return coach;
        }

        public async Task AddCoachAsync(Coach coach, int? initialClubId)
        {
            _context.Trenerzy.Add(coach);
            await _context.SaveChangesAsync();

            if (initialClubId.HasValue && initialClubId.Value != 0)
            {
                var assignment = new CoachClubAssignment
                {
                    IDtrenera = coach.IDtrenera,
                    IDklubu = initialClubId.Value,
                    DataZatrudnienia = DateTime.Today,
                    DataZwolnienia = null
                };
                _context.TrenerzyKlubow.Add(assignment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateCoachAsync(Coach coach)
        {
            var existing = _context.Trenerzy.Local.FirstOrDefault(c => c.IDtrenera == coach.IDtrenera);
            if (existing != null)
            {
                _context.Entry(existing).State = EntityState.Detached;
            }
            _context.Trenerzy.Update(coach);
            await _context.SaveChangesAsync();
        }

        public async Task ReleaseCoachFromCurrentClubAsync(int coachId, DateTime releaseDate)
        {
            var activeAssignments = await _context.TrenerzyKlubow
                                                  .Where(a => a.IDtrenera == coachId && a.DataZwolnienia == null)
                                                  .ToListAsync();
            foreach (var assignment in activeAssignments)
            {
                assignment.DataZwolnienia = releaseDate;
            }
            if (activeAssignments.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task AssignCoachToClubAsync(int coachId, int clubId, DateTime joinDate)
        {
            await ReleaseCoachFromCurrentClubAsync(coachId, joinDate.AddDays(-1));

            var newAssignment = new CoachClubAssignment
            {
                IDtrenera = coachId,
                IDklubu = clubId,
                DataZatrudnienia = joinDate,
                DataZwolnienia = null
            };
            _context.TrenerzyKlubow.Add(newAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCoachAsync(int coachId)
        {
            var coach = await _context.Trenerzy.FindAsync(coachId);
            if (coach != null)
            {
                _context.Trenerzy.Remove(coach);
                await _context.SaveChangesAsync();
            }
        }
    }
}