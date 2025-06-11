using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FootballLeague.Models;
using FootballLeague.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FootballLeague.ViewModels
{
    [QueryProperty(nameof(PlayerId), "playerId")]
    public partial class AddEditPlayerViewModel : BaseViewModel
    {
        private readonly PlayerService _playerService;
        private readonly PositionService _positionService;
        private readonly ClubService _clubService;

        public ObservableCollection<Position> AvailablePositions { get; } = new();
        public ObservableCollection<Club> AvailableClubs { get; } = new();

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
        Club? _selectedInitialClub;

        private bool _isEditing = false;
        private Player? _originalPlayer;
        private Club? _clubBeforeEdit;

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
                    AvailableClubs.Clear();
                    var clubs = await _clubService.GetClubsAsync();
                    AvailableClubs.Add(new Club { IdKlubu = 0, Nazwa = "Brak klubu (wolny agent)" });
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
            await LoadInitialDataAsync();

            if (playerId == 0)
            {
                Title = "Dodaj Zawodnika";
                _originalPlayer = new Player();
                Imie = string.Empty;
                Nazwisko = string.Empty;
                SelectedPosition = AvailablePositions.FirstOrDefault();
                SelectedInitialClub = AvailableClubs.FirstOrDefault(c => c.IdKlubu == 0);
                _clubBeforeEdit = null;
                _isEditing = false;
            }
            else
            {
                Title = "Edytuj Zawodnika";
                _originalPlayer = await _playerService.GetPlayerByIdAsync(playerId);
                if (_originalPlayer != null)
                {
                    Imie = _originalPlayer.Imie;
                    Nazwisko = _originalPlayer.Nazwisko;
                    SelectedPosition = AvailablePositions.FirstOrDefault(p => p.IDpozycji == _originalPlayer.IDpozycji);
                    SelectedInitialClub = _originalPlayer.AktualnyKlub ?? AvailableClubs.FirstOrDefault(c => c.IdKlubu == 0);
                    _clubBeforeEdit = _originalPlayer.AktualnyKlub;
                }
                _isEditing = true;
            }
        }

        [RelayCommand]
        async Task SavePlayerAsync()
        {
            if (string.IsNullOrWhiteSpace(Imie) || string.IsNullOrWhiteSpace(Nazwisko) || SelectedPosition == null)
            {
                await Shell.Current.DisplayAlert("Błąd", "Imię, nazwisko i pozycja są wymagane.", "OK");
                return;
            }

            Player playerToSave = new Player
            {
                IDzawodnika = _playerId,
                Imie = Imie,
                Nazwisko = Nazwisko,
                IDpozycji = SelectedPosition.IDpozycji
            };

            IsBusy = true;
            try
            {
                if (_isEditing)
                {
                    await _playerService.UpdatePlayerAsync(playerToSave);

                    int? newClubId = SelectedInitialClub?.IdKlubu;
                    int? oldClubId = _clubBeforeEdit?.IdKlubu;

                    if (newClubId == 0)
                    {
                        if (oldClubId.HasValue && oldClubId.Value != 0)
                        {
                            await _playerService.ReleasePlayerFromCurrentClubAsync(_playerId, DateTime.Today);
                        }
                    }
                    else if (newClubId.HasValue && newClubId != oldClubId)
                    {
                        await _playerService.AssignPlayerToClubAsync(_playerId, newClubId.Value, DateTime.Today);
                    }
                }
                else
                {
                    int? initialClubId = (SelectedInitialClub != null && SelectedInitialClub.IdKlubu != 0) ? SelectedInitialClub.IdKlubu : (int?)null;
                    await _playerService.AddPlayerAsync(playerToSave, initialClubId);
                }

                await Shell.Current.DisplayAlert("Sukces", _isEditing ? "Zawodnik zaktualizowany!" : "Zawodnik dodany!", "OK");
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
            if (_playerId == 0)
            {
                if ((AvailablePositions.Count == 0 || AvailableClubs.Count <= 1) && !IsBusy)
                {
                    await LoadInitialDataAsync();
                    if (_playerId == 0)
                    {
                        SelectedPosition = AvailablePositions.FirstOrDefault();
                        SelectedInitialClub = AvailableClubs.FirstOrDefault(c => c.IdKlubu == 0);
                    }
                }
            }
            else if (_originalPlayer == null && !IsBusy)
            {
                LoadPlayerAsync(_playerId);
            }
        }
    }
}