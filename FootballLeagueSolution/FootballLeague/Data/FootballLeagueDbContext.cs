using Microsoft.EntityFrameworkCore;
using FootballLeague.Models;


namespace FootballLeague.Data
{
    public class FootballLeagueDbContext : DbContext
    {
        public DbSet<Club> Kluby { get; set; }
        public DbSet<Match> Mecze { get; set; }
        public DbSet<Stadium> Stadiony { get; set; }
        public DbSet<Player> Zawodnicy { get; set; }
        public DbSet<Position> Pozycje { get; set; }
        public DbSet<Transfer> Transfery { get; set; }
        public DbSet<Coach> Trenerzy { get; set; }
        public DbSet<CoachClubAssignment> TrenerzyKlubow { get; set; }
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


            modelBuilder.Entity<Stadium>().ToTable("Stadiony").HasKey(s => s.IDstadionu);

            modelBuilder.Entity<Position>().ToTable("Pozycje").HasKey(p => p.IDpozycji);
            modelBuilder.Entity<Position>().Property(p => p.IDpozycji).ValueGeneratedOnAdd(); 
            modelBuilder.Entity<Position>().HasIndex(p => p.NazwaPozycji).IsUnique(); 



            modelBuilder.Entity<Player>().ToTable("Zawodnicy").HasKey(p => p.IDzawodnika);
            modelBuilder.Entity<Player>().Property(p => p.IDzawodnika).ValueGeneratedOnAdd();
            modelBuilder.Entity<Player>()
                .HasOne(p => p.Pozycja)
                .WithMany() 
                .HasForeignKey(p => p.IDpozycji)
                .OnDelete(DeleteBehavior.Restrict); 


            modelBuilder.Entity<Transfer>().ToTable("Transfery").HasKey(t => t.IDtransferu);
            modelBuilder.Entity<Transfer>().Property(t => t.IDtransferu).ValueGeneratedOnAdd();
            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.Zawodnik)
                .WithMany() 
                .HasForeignKey(t => t.IDzawodnika)
                .OnDelete(DeleteBehavior.Cascade); 
            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.Klub)
                .WithMany() 
                .HasForeignKey(t => t.IDklubu)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Coach>().ToTable("Trenerzy").HasKey(c => c.IDtrenera);
            modelBuilder.Entity<Coach>().Property(c => c.IDtrenera).ValueGeneratedOnAdd();


            modelBuilder.Entity<CoachClubAssignment>().ToTable("TrenerzyKlubow").HasKey(a => a.IDrelacji);
            modelBuilder.Entity<CoachClubAssignment>().Property(a => a.IDrelacji).ValueGeneratedOnAdd();
            modelBuilder.Entity<CoachClubAssignment>()
                .HasOne(a => a.Trener)
                .WithMany()
                .HasForeignKey(a => a.IDtrenera)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CoachClubAssignment>()
                .HasOne(a => a.Klub)
                .WithMany()
                .HasForeignKey(a => a.IDklubu)
                .OnDelete(DeleteBehavior.Restrict);
        
    }
    }
}