using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FootballLeague.Models;
using FootballLeague.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;

namespace FootballLeague.ViewModels
{
    public partial class AddMatchViewModel : BaseViewModel
    {
        private readonly MatchService _matchService;
        private readonly ClubService _clubService;

        public ObservableCollection<Club> AvailableClubs { get; } = new();

        [ObservableProperty]
        Club selectedHomeTeam;

        [ObservableProperty]
        Club selectedAwayTeam;

        [ObservableProperty]
        DateTime matchDate = DateTime.Today;

        [ObservableProperty]
        string homeScore;

        [ObservableProperty]
        string awayScore;


        public AddMatchViewModel(MatchService matchService, ClubService clubService)
        {
            _matchService = matchService;
            _clubService = clubService;
            Title = "Dodaj Mecz";
        }

        public async Task LoadClubsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                AvailableClubs.Clear();
                var clubs = await _clubService.GetClubsAsync();
                foreach (var club in clubs)
                {
                    AvailableClubs.Add(club);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Błąd", $"Nie udało się załadować klubów: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task SaveMatchAsync()
        {
            if (SelectedHomeTeam == null || SelectedAwayTeam == null)
            {
                await Shell.Current.DisplayAlert("Błąd", "Wybierz obie drużyny.", "OK");
                return;
            }

            if (SelectedHomeTeam.IdKlubu == SelectedAwayTeam.IdKlubu)
            {
                await Shell.Current.DisplayAlert("Błąd", "Gospodarz i gość nie mogą być tym samym klubem.", "OK");
                return;
            }

            byte? parsedHomeScore = null;
            if (!string.IsNullOrWhiteSpace(HomeScore))
            {
                if (byte.TryParse(HomeScore, out byte hs))
                    parsedHomeScore = hs;
                else
                {
                    await Shell.Current.DisplayAlert("Błąd", "Nieprawidłowy wynik gospodarzy.", "OK"); return;
                }
            }

            byte? parsedAwayScore = null;
            if (!string.IsNullOrWhiteSpace(AwayScore))
            {
                if (byte.TryParse(AwayScore, out byte as_))
                    parsedAwayScore = as_;
                else
                {
                    await Shell.Current.DisplayAlert("Błąd", "Nieprawidłowy wynik gości.", "OK"); return;
                }
            }

            if ((parsedHomeScore.HasValue && !parsedAwayScore.HasValue) || (!parsedHomeScore.HasValue && parsedAwayScore.HasValue))
            {
                await Shell.Current.DisplayAlert("Błąd", "Podaj oba wyniki lub żaden (dla meczu nierozegranego).", "OK"); return;
            }

            var newMatch = new Models.Match 
            {
                IdGospodarza = SelectedHomeTeam.IdKlubu,
                IdGoscia = SelectedAwayTeam.IdKlubu,
                DataMeczu = MatchDate,
                BramkiGospodarza = parsedHomeScore,
                BramkiGoscia = parsedAwayScore
            };

            IsBusy = true;
            try
            {
                await _matchService.AddMatchAsync(newMatch);
                await Shell.Current.DisplayAlert("Sukces", "Mecz dodany!", "OK");
                MessagingCenter.Send(this, "MatchAdded");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Błąd", $"Nie udało się dodać meczu: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        public async Task OnAppearing()
        {
            await LoadClubsAsync();
        }
    }
}