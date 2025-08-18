using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FootballLeague.Data;
using FootballLeague.Services;
using FootballLeague.ViewModels;
using FootballLeague.Views;       
using System.Reflection;
using Microsoft.Extensions.Logging;


namespace FootballLeague 
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            var assembly = Assembly.GetExecutingAssembly();
           
            string resourceName = "FootballLeague.Resources.Raw.appsettings.json";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                var allResources = assembly.GetManifestResourceNames();
                System.Diagnostics.Debug.WriteLine("Dostępne zasoby osadzone:");
                foreach (var res in allResources) System.Diagnostics.Debug.WriteLine(res);
                throw new FileNotFoundException($"Nie można znaleźć zasobu osadzonego: '{resourceName}'. Sprawdź nazwę i czy plik jest ustawiony jako EmbeddedResource.");
            }


            var config = new ConfigurationBuilder()
                        .AddJsonStream(stream)
                        .Build();
            builder.Configuration.AddConfiguration(config);

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException("Connection string 'DefaultConnection' nie został znaleziony w appsettings.json.");

            builder.Services.AddDbContext<FootballLeagueDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddScoped<PositionService>();
            builder.Services.AddScoped<PlayerService>();

            builder.Services.AddTransient<AddEditPlayerViewModel>();
            builder.Services.AddTransient<PlayerListViewModel>();

            builder.Services.AddTransient<AddEditPlayerPage>();
            builder.Services.AddTransient<PlayerListPage>();

            builder.Services.AddScoped<ClubService>();
            builder.Services.AddScoped<MatchService>();
            builder.Services.AddScoped<LeagueTableService>();

            builder.Services.AddTransient<LeagueTableViewModel>();
            builder.Services.AddTransient<AddMatchViewModel>();
            builder.Services.AddTransient<ViewMatchesViewModel>();

            builder.Services.AddTransient<LeagueTablePage>();
            builder.Services.AddTransient<AddMatchPage>();
            builder.Services.AddTransient<ViewMatchesPage>();


            builder.Services.AddScoped<CoachService>();

            builder.Services.AddTransient<CoachListViewModel>();
            builder.Services.AddTransient<AddEditCoachViewModel>();

            builder.Services.AddTransient<CoachListPage>();
            builder.Services.AddTransient<AddEditCoachPage>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}