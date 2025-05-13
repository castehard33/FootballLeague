
using CommunityToolkit.Mvvm.Input;
using FootballLeague.Models;
using FootballLeague.Services;
using System.Collections.ObjectModel;


namespace FootballLeague.ViewModels
{
    public partial class LeagueTableViewModel : BaseViewModel
    {
        private readonly LeagueTableService _leagueTableService;
        public ObservableCollection<LeagueTableEntry> LeagueEntries { get; } = new();

        public LeagueTableViewModel(LeagueTableService leagueTableService)
        {
            _leagueTableService = leagueTableService;
            Title = "Tabela Ligowa";
            MessagingCenter.Subscribe<AddMatchViewModel>(this, "MatchAdded", async (sender) =>
            {
                await LoadLeagueTableAsync();
            });
        }

        [RelayCommand]
        async Task LoadLeagueTableAsync()
        {
            try
            {
                LeagueEntries.Clear();
                var entriesFromService = await _leagueTableService.GetLeagueTableAsync(); 
                for (int i = 0; i < entriesFromService.Count; i++)
                {
                    var entry = entriesFromService[i];
                    entry.IsOddRow = (i % 2 != 0); //dla zebry
                    LeagueEntries.Add(entry);
                }
            }

            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Błąd", $"Nie udało się załadować tabeli: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task OnAppearing()
        {
            await LoadLeagueTableAsync();
        }
    }
}