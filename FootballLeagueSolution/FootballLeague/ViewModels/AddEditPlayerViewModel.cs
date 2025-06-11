using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FootballLeague.Models;
using FootballLeague.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;
using System.Linq; // Dla FirstOrDefault
using System.Diagnostics;

namespace FootballLeague.ViewModels
{
    [QueryProperty(nameof(PlayerId), "playerId")]
    public partial class AddEditPlayerViewModel : BaseViewModel
    {
        private readonly PlayerService _playerService;
        private readonly PositionService _positionService;
        private readonly ClubService _clubService; // Do wyboru początkowego klubu

        public ObservableCollection<Position> AvailablePositions { get; } = new();
        public ObservableCollection<Club> AvailableClubs { get; } = new(); // Dla początkowego klubu

        private int _playerId;
        public int PlayerId
        {
            get => _playerId;
            set
            {
                SetProperty(ref _playerId, value);
                LoadPlayerAsync(value);
            }
        }

        [ObservableProperty]
        string? _imie;

        [ObservableProperty]
        string? _nazwisko;

        [ObservableProperty]
        Position? _selectedPosition;

        [ObservableProperty]
        Club? _selectedInitialClub; // Dla klubu, do którego zawodnik jest dodawany

        private bool _isEditing = false;

        public AddEditPlayerViewModel(PlayerService playerService, PositionService positionService, ClubService clubService)
        {
            _playerService = playerService;
            _positionService = positionService;
            _clubService = clubService;
            Title = "Dodaj Zawodnika";
        }

        public async Task LoadInitialDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                if (AvailablePositions.Count == 0)
                {
                    var positions = await _positionService.GetPositionsAsync();
                    foreach (var pos in positions) AvailablePositions.Add(pos);
                }
                if (AvailableClubs.Count == 0)
                {
                    var clubs = await _clubService.GetClubsAsync();
                    AvailableClubs.Add(new Club { IdKlubu = 0, Nazwa = "Brak klubu (wolny agent)" }); // Opcja braku klubu
                    foreach (var club in clubs) AvailableClubs.Add(club);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd ładowania danych dla formularza zawodnika: {ex.ToString()}");
                await Shell.Current.DisplayAlert("Błąd", "Nie udało się załadować danych formularza.", "OK");
            }
            finally { IsBusy = false; }
        }


        private async void LoadPlayerAsync(int playerId)
        {
            await LoadInitialDataAsync(); // Upewnij się, że pozycje i kluby są załadowane

            if (playerId == 0) // Dodawanie nowego
            {
                Title = "Dodaj Zawodnika";
                Imie = string.Empty;
                Nazwisko = string.Empty;
                SelectedPosition = AvailablePositions.FirstOrDefault();
                SelectedInitialClub = AvailableClubs.FirstOrDefault(c => c.IdKlubu == 0); // Domyślnie "Brak klubu"
                _isEditing = false;
            }
            else // Edycja istniejącego
            {
                Title = "Edytuj Zawodnika";
                var player = await _playerService.GetPlayerByIdAsync(playerId);
                if (player != null)
                {
                    Imie = player.Imie;
                    Nazwisko = player.Nazwisko;
                    SelectedPosition = AvailablePositions.FirstOrDefault(p => p.IDpozycji == player.IDpozycji);
                    // Aktualny klub jest ładowany przez PlayerService, ale nie ma tu bezpośredniej edycji klubu
                    // (to będzie przez transfery). Możemy go wyświetlić, ale nie jako SelectedInitialClub.
                    // SelectedInitialClub pozostaje do wyboru klubu przy tworzeniu nowego zawodnika.
                    // Dla edycji, pole wyboru klubu początkowego może być ukryte lub nieaktywne.
                    SelectedInitialClub = player.AktualnyKlub ?? AvailableClubs.FirstOrDefault(c => c.IdKlubu == 0);

                }
                _isEditing = true;
            }
            // Odśwież CanExecute dla komend, jeśli takie masz
        }

        [RelayCommand]
        async Task SavePlayerAsync()
        {
            if (string.IsNullOrWhiteSpace(Imie) || string.IsNullOrWhiteSpace(Nazwisko) || SelectedPosition == null)
            {
                await Shell.Current.DisplayAlert("Błąd", "Imię, nazwisko i pozycja są wymagane.", "OK");
                return;
            }

            Player player = new Player
            {
                IDzawodnika = _playerId, // Będzie 0 dla nowego
                Imie = Imie,
                Nazwisko = Nazwisko,
                IDpozycji = SelectedPosition.IDpozycji
            };

            IsBusy = true;
            try
            {
                if (_isEditing)
                {
                    await _playerService.UpdatePlayerAsync(player);
                }
                else
                {
                    int? initialClubId = (SelectedInitialClub != null && SelectedInitialClub.IdKlubu != 0) ? SelectedInitialClub.IdKlubu : (int?)null;
                    await _playerService.AddPlayerAsync(player, initialClubId);
                }
                await Shell.Current.DisplayAlert("Sukces", _isEditing ? "Zawodnik zaktualizowany!" : "Zawodnik dodany!", "OK");
                // Powiadom listę o zmianie
                MessagingCenter.Send(this, "PlayersChanged");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd zapisywania zawodnika: {ex.ToString()}");
                await Shell.Current.DisplayAlert("Błąd", $"Nie udało się zapisać zawodnika: {ex.Message}", "OK");
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        async Task DeletePlayerAsync()
        {
            if (!_isEditing || _playerId == 0) return;

            bool confirmed = await Shell.Current.DisplayAlert("Potwierdzenie", $"Czy na pewno chcesz usunąć zawodnika {Imie} {Nazwisko}?", "Tak", "Nie");
            if (confirmed)
            {
                IsBusy = true;
                try
                {
                    await _playerService.DeletePlayerAsync(_playerId);
                    await Shell.Current.DisplayAlert("Sukces", "Zawodnik usunięty.", "OK");
                    MessagingCenter.Send(this, "PlayersChanged");
                    await Shell.Current.GoToAsync("..");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Błąd usuwania zawodnika: {ex.ToString()}");
                    await Shell.Current.DisplayAlert("Błąd", $"Nie udało się usunąć zawodnika: {ex.Message}", "OK");
                }
                finally { IsBusy = false; }
            }
        }

        public async Task OnAppearing()
        {
            // Jeśli nie jesteśmy w trybie edycji (playerId == 0), to jest dodawanie nowego, załaduj dane
            // Jeśli edytujemy, LoadPlayerAsync już wywoła LoadInitialDataAsync
            if (_playerId == 0 && (AvailablePositions.Count == 0 || AvailableClubs.Count == 0) && !IsBusy)
            {
                await LoadInitialDataAsync();
                // Ustawienie domyślnych wartości dla nowego zawodnika po załadowaniu danych
                SelectedPosition = AvailablePositions.FirstOrDefault();
                SelectedInitialClub = AvailableClubs.FirstOrDefault(c => c.IdKlubu == 0);
            }
        }
    }
}