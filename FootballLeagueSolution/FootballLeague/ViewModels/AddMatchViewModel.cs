using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FootballLeague.Models;
using FootballLeague.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;

namespace FootballLeague.ViewModels
{
    public partial class AddMatchViewModel : BaseViewModel
    {
        private readonly MatchService _matchService;
        private readonly ClubService _clubService;

        public ObservableCollection<Club> AvailableClubs { get; } = new();

        [ObservableProperty]
        Club? _selectedHomeTeam;

        [ObservableProperty]
        Club? _selectedAwayTeam;

        [ObservableProperty]
        DateTime _matchDate = DateTime.Today;

        [ObservableProperty]
        string? _homeScore;

        [ObservableProperty]
        string? _awayScore;


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
                Debug.WriteLine($"Błąd podczas ładowania klubów: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner Exception (ładowanie klubów): {ex.InnerException.Message}");
                }
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
            if (_selectedHomeTeam == null || _selectedAwayTeam == null)
            {
                await Shell.Current.DisplayAlert("Błąd", "Wybierz obie drużyny.", "OK");
                return;
            }

            if (_selectedHomeTeam.IdKlubu == _selectedAwayTeam.IdKlubu)
            {
                await Shell.Current.DisplayAlert("Błąd", "Gospodarz i gość nie mogą być tym samym klubem.", "OK");
                return;
            }

            byte? parsedHomeScore = null;
            if (!string.IsNullOrWhiteSpace(_homeScore))
            {
                if (byte.TryParse(_homeScore, out byte hs))
                    parsedHomeScore = hs;
                else
                {
                    await Shell.Current.DisplayAlert("Błąd", "Nieprawidłowy wynik gospodarzy.", "OK"); return;
                }
            }

            byte? parsedAwayScore = null;
            if (!string.IsNullOrWhiteSpace(_awayScore))
            {
                if (byte.TryParse(_awayScore, out byte as_))
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

                IdGospodarza = SelectedHomeTeam!.IdKlubu,
                IdGoscia = SelectedAwayTeam!.IdKlubu,
                DataMeczu = MatchDate,
                BramkiGospodarza = parsedHomeScore,
                BramkiGoscia = parsedAwayScore,
                IdStadionu = SelectedHomeTeam.IdStadionu
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
                Debug.WriteLine("--------------------------------------------------");
                Debug.WriteLine($"BŁĄD PODCZAS ZAPISYWANIA MECZU: {ex.GetType().FullName}");
                Debug.WriteLine($"Message: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                Exception? innerEx = ex.InnerException;
                int depth = 1;
                while (innerEx != null)
                {
                    Debug.WriteLine($"--- Inner Exception (Poziom {depth}) ---");
                    Debug.WriteLine($"Inner Exception Type: {innerEx.GetType().FullName}");
                    Debug.WriteLine($"Inner Exception Message: {innerEx.Message}");
                    Debug.WriteLine($"Inner Exception StackTrace: {innerEx.StackTrace}");
                    innerEx = innerEx.InnerException;
                    depth++;
                }
                Debug.WriteLine("--------------------------------------------------");


                string errorMessage = "Nie udało się dodać meczu.";
                if (ex.InnerException != null)
                {

                    errorMessage += $"\n\nSzczegóły błędu (dla dewelopera):\n{ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"\n{ex.InnerException.InnerException.Message}";
                    }
                }
                else
                {
                    errorMessage += $"\n\nSzczegóły błędu (dla dewelopera):\n{ex.Message}";
                }
                await Shell.Current.DisplayAlert("Błąd Krytyczny", errorMessage, "OK");
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
            if (AvailableClubs.Count == 0 && !IsBusy)
            {
                await LoadClubsAsync();
            }
        }
    }
}