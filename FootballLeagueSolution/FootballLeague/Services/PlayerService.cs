using FootballLeague.Data;
using FootballLeague.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace FootballLeague.Services
{
    public class PlayerService
    {
        private readonly FootballLeagueDbContext _context;

        public PlayerService(FootballLeagueDbContext context) => _context = context;

        public async Task<List<Player>> GetPlayersAsync()
        {
            var players = await _context.Zawodnicy
                                      .Include(p => p.Pozycja) // dane pozycji
                                      .OrderBy(p => p.Nazwisko).ThenBy(p => p.Imie)
                                      .ToListAsync();

            var playerIds = players.Select(p => p.IDzawodnika).ToList();

            var activeTransfers = await _context.Transfery
                                              .Where(t => playerIds.Contains(t.IDzawodnika) && t.DataOdejscia == null)
                                              .Include(t => t.Klub) // dane klubu z transferu
                                              .OrderByDescending(t => t.DataDolaczenia)
                                              .ToListAsync();

            var latestActiveTransfersByPlayerId = activeTransfers
                                                .GroupBy(t => t.IDzawodnika)
                                                .ToDictionary(
                                                    g => g.Key,
                                                    g => g.First()
                                                );

            // znajdź aktualny klub zawodnika
            foreach (var player in players)
            {
                if (latestActiveTransfersByPlayerId.TryGetValue(player.IDzawodnika, out var currentTransfer))
                {
                    player.AktualnyKlub = currentTransfer.Klub;
                }
                else
                {
                    player.AktualnyKlub = null;
                }
            }
            return players;
        }

        public async Task<Player?> GetPlayerByIdAsync(int playerId)
        {
            var player = await _context.Zawodnicy
                                 .Include(p => p.Pozycja)
                                 .FirstOrDefaultAsync(p => p.IDzawodnika == playerId);

            if (player != null)
            {
                var currentTransfer = await _context.Transfery
                                                   .Where(t => t.IDzawodnika == player.IDzawodnika && t.DataOdejscia == null)
                                                   .Include(t => t.Klub)
                                                   .OrderByDescending(t => t.DataDolaczenia)
                                                   .FirstOrDefaultAsync();
                if (currentTransfer != null)
                {
                    player.AktualnyKlub = currentTransfer.Klub;
                }
                else
                {
                    player.AktualnyKlub = null;
                }
            }
            return player;
        }

        public async Task AddPlayerAsync(Player player, int? initialClubId)
        {
            // dodaj zawodnika, aby uzyskać ID
            _context.Zawodnicy.Add(player);
            await _context.SaveChangesAsync();


            if (initialClubId.HasValue && initialClubId.Value != 0)
            {
                var initialTransfer = new Transfer
                {
                    IDzawodnika = player.IDzawodnika,
                    IDklubu = initialClubId.Value,
                    DataDolaczenia = DateTime.Today,
                    DataOdejscia = null
                };
                _context.Transfery.Add(initialTransfer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdatePlayerAsync(Player player)
        {
            var existingPlayer = _context.Zawodnicy.Local.FirstOrDefault(p => p.IDzawodnika == player.IDzawodnika);
            if (existingPlayer != null)
            {
                _context.Entry(existingPlayer).State = EntityState.Detached;
            }
            _context.Zawodnicy.Update(player);
            await _context.SaveChangesAsync();
        }

        public async Task ReleasePlayerFromCurrentClubAsync(int playerId, DateTime releaseDate)
        {
            var activeTransfers = await _context.Transfery
                                              .Where(t => t.IDzawodnika == playerId && t.DataOdejscia == null)
                                              .ToListAsync();

            if (activeTransfers.Any())
            {
                foreach (var transfer in activeTransfers)
                {
                    transfer.DataOdejscia = releaseDate;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task AssignPlayerToClubAsync(int playerId, int clubId, DateTime joinDate)
        {

            var activeTransfers = await _context.Transfery
                                              .Where(t => t.IDzawodnika == playerId && t.DataOdejscia == null)
                                              .ToListAsync();
            foreach (var activeTransfer in activeTransfers)
            {

                if (joinDate < activeTransfer.DataDolaczenia)
                {

                    activeTransfer.DataOdejscia = joinDate.AddDays(-1) < activeTransfer.DataDolaczenia ? activeTransfer.DataDolaczenia : joinDate.AddDays(-1);
                }
                else
                {
                    activeTransfer.DataOdejscia = joinDate.AddDays(-1); 
                }
            }

            if (activeTransfers.Any())
            {
                await _context.SaveChangesAsync();
            }



            var newTransfer = new Transfer
            {
                IDzawodnika = playerId,
                IDklubu = clubId,
                DataDolaczenia = joinDate,
                DataOdejscia = null
            };
            _context.Transfery.Add(newTransfer);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePlayerAsync(int playerId)
        {
            var player = await _context.Zawodnicy.FindAsync(playerId);
            if (player != null)
            {
                _context.Zawodnicy.Remove(player);
                await _context.SaveChangesAsync();
            }
        }




    }
}