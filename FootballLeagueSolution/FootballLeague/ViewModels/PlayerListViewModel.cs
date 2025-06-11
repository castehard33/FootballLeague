using CommunityToolkit.Mvvm.Input;
using FootballLeague.Models;
using FootballLeague.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using FootballLeague.Views;

namespace FootballLeague.ViewModels
{
    public partial class PlayerListViewModel : BaseViewModel
    {
        private readonly PlayerService _playerService;
        public ObservableCollection<Player> Players { get; } = new();

        public PlayerListViewModel(PlayerService playerService)
        {
            _playerService = playerService;
            Title = "Zawodnicy";
            MessagingCenter.Subscribe<AddEditPlayerViewModel>(this, "PlayersChanged", async (sender) =>
            {
                await LoadPlayersAsync();
            });
        }

        [RelayCommand]
        async Task LoadPlayersAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Players.Clear();
                var players = await _playerService.GetPlayersAsync();
                foreach (var player in players)
                {
                    Players.Add(player);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd ładowania zawodników: {ex.ToString()}");
                await Shell.Current.DisplayAlert("Błąd", $"Nie udało się załadować listy zawodników: {ex.Message}", "OK");
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        async Task GoToAddPlayerAsync()
        {
            await Shell.Current.GoToAsync(nameof(AddEditPlayerPage));
        }

        [RelayCommand]
        async Task GoToEditPlayerAsync(Player player)
        {
            if (player == null) return;
            await Shell.Current.GoToAsync($"{nameof(AddEditPlayerPage)}?playerId={player.IDzawodnika}");
        }

        public async Task OnAppearing()
        {
            await LoadPlayersAsync();
        }
    }
}