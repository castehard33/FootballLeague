using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FootballLeague.Models;
using FootballLeague.Services;
using System.Collections.ObjectModel;

using System.Diagnostics;

namespace FootballLeague.ViewModels
{
    public partial class AddMatchViewModel : BaseViewModel
    {
        private readonly MatchService _matchService;
        private readonly ClubService _clubService;

        public ObservableCollection<Club> AvailableClubs { get; } = [];

        [ObservableProperty]
        Club? _selectedHomeTeam;

        [ObservableProperty]
        Club? _selectedAwayTeam;

        [ObservableProperty]
        DateTime _matchDatePart = DateTime.Today; 

        [ObservableProperty]
        TimeSpan _matchTimePart = DateTime.Now.TimeOfDay; 

        [ObservableProperty]
        string? _homeScore;

        [ObservableProperty]
        string? _awayScore;

        public AddMatchViewModel(MatchService matchService, ClubService clubService )
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
                if (AvailableClubs.Count == 0) 
                {
                    AvailableClubs.Clear();
                    var clubs = await _clubService.GetClubsAsync();
                    foreach (var club in clubs)
                    {
                        AvailableClubs.Add(club);
                    }
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
        [Obsolete]
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

           
            if (SelectedHomeTeam.IdStadionu == 0) 
            {
                await Shell.Current.DisplayAlert("Błąd", "Wybrany gospodarz nie ma przypisanego ID stadionu.", "OK");
                return;
            }


            byte parsedHomeScore = 0; // Krok 1: Inicjalizuj na 0
            if (!string.IsNullOrWhiteSpace(HomeScore))
            {
                if (!byte.TryParse(HomeScore, out parsedHomeScore)) // Krok 2: Spróbuj sparsować, jeśli użytkownik coś wpisał
                {
                    await Shell.Current.DisplayAlert("Błąd", "Nieprawidłowy wynik gospodarzy.", "OK"); return;
                }
            }
            // Jeśli HomeScore jest puste, parsedHomeScore pozostaje 0.

            byte parsedAwayScore = 0; // Krok 1: Inicjalizuj na 0
            if (!string.IsNullOrWhiteSpace(AwayScore))
            {
                if (!byte.TryParse(AwayScore, out parsedAwayScore)) // Krok 2: Spróbuj sparsować, jeśli użytkownik coś wpisał
                {
                    await Shell.Current.DisplayAlert("Błąd", "Nieprawidłowy wynik gości.", "OK"); return;
                }
            }


            
            DateTime combinedDateTime = MatchDatePart.Date + MatchTimePart;

            var newMatch = new Models.Match
            {
                IdGospodarza = SelectedHomeTeam!.IdKlubu,
                IdGoscia = SelectedAwayTeam!.IdKlubu,
                DataMeczu = combinedDateTime, 
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
                Exception? innerEx = ex.InnerException; int depth = 1;
                while (innerEx != null) { Debug.WriteLine($"--- Inner Exception (Poziom {depth}) ---"); Debug.WriteLine($"Inner Exception Type: {innerEx.GetType().FullName}"); Debug.WriteLine($"Inner Exception Message: {innerEx.Message}"); Debug.WriteLine($"Inner Exception StackTrace: {innerEx.StackTrace}"); innerEx = innerEx.InnerException; depth++; }
                Debug.WriteLine("--------------------------------------------------");
                string errorMessage = "Nie udało się dodać meczu.";
                if (ex.InnerException != null) { errorMessage += $"\n\nSzczegóły błędu (dla dewelopera):\n{ex.InnerException.Message}"; if (ex.InnerException.InnerException != null) { errorMessage += $"\n{ex.InnerException.InnerException.Message}"; } } else { errorMessage += $"\n\nSzczegóły błędu (dla dewelopera):\n{ex.Message}"; }
                await Shell.Current.DisplayAlert("Błąd Krytyczny", errorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
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