using Microsoft.EntityFrameworkCore;
using FootballLeague.Models;
using System.Reflection.Emit;

namespace FootballLeague.Data
{
    public class FootballLeagueDbContext : DbContext
    {
        public DbSet<Club> Kluby { get; set; }
        public DbSet<Match> Mecze { get; set; }
        public DbSet<Stadium> Stadiony { get; set; }
        public FootballLeagueDbContext(DbContextOptions<FootballLeagueDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Club>().ToTable("Kluby");
            modelBuilder.Entity<Club>().HasKey(k => k.IdKlubu);

            modelBuilder.Entity<Match>().ToTable("Mecze");
            modelBuilder.Entity<Match>().HasKey(m => m.IdMeczu);
            modelBuilder.Entity<Match>()
                .Property(m => m.IdMeczu)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Gospodarz)
                .WithMany()
                .HasForeignKey(m => m.IdGospodarza)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Gosc)
                .WithMany()
                .HasForeignKey(m => m.IdGoscia)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}