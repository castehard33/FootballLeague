using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FootballLeague.Models;
using FootballLeague.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Diagnostics;

namespace FootballLeague.ViewModels
{
    [QueryProperty(nameof(CoachId), "coachId")]
    public partial class AddEditCoachViewModel : BaseViewModel
    {
        private readonly CoachService _coachService;
        private readonly ClubService _clubService;

        public ObservableCollection<Club> AvailableClubs { get; } = new();
        public ObservableCollection<string> AvailableLicenses { get; } = new();

        private int _coachId;
        public int CoachId
        {
            get => _coachId;
            set
            {
                SetProperty(ref _coachId, value);
                LoadCoachAsync(value);
            }
        }

        [ObservableProperty]
        string? _imie;
        [ObservableProperty]
        string? _nazwisko;
        [ObservableProperty]
        string? _selectedLicense;
        [ObservableProperty]
        Club? _selectedClub;

        private bool _isEditing = false;
        private Club? _clubBeforeEdit;

        public AddEditCoachViewModel(CoachService coachService, ClubService clubService)
        {
            _coachService = coachService;
            _clubService = clubService;
            Title = "Dodaj Trenera";

            AvailableLicenses.Add("UEFA Pro");
            AvailableLicenses.Add("UEFA A");
            AvailableLicenses.Add("UEFA B");
            AvailableLicenses.Add("UEFA C");
            AvailableLicenses.Add("Inna");
        }

        public async Task LoadInitialDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                if (AvailableClubs.Count == 0)
                {
                    AvailableClubs.Clear();
                    AvailableClubs.Add(new Club { IdKlubu = 0, Nazwa = "Brak klubu" });

                    var clubsWithoutCoach = await _clubService.GetClubsWithoutCoachAsync();
                    foreach (var club in clubsWithoutCoach)
                    {
                        AvailableClubs.Add(club);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd ładowania danych formularza trenera: {ex.ToString()}");
                await Shell.Current.DisplayAlert("Błąd", "Nie udało się załadować danych formularza.", "OK");
            }
            finally { IsBusy = false; }
        }

        private async void LoadCoachAsync(int coachId)
        {
            await LoadInitialDataAsync();

            if (coachId == 0)
            {
                Title = "Dodaj Trenera";
                Imie = string.Empty;
                Nazwisko = string.Empty;
                SelectedLicense = AvailableLicenses.FirstOrDefault();
                SelectedClub = AvailableClubs.FirstOrDefault(c => c.IdKlubu == 0);
                _clubBeforeEdit = null;
                _isEditing = false;
            }
            else
            {
                Title = "Edytuj Trenera";
                var coach = await _coachService.GetCoachByIdAsync(coachId);
                if (coach != null)
                {
                    Imie = coach.Imie;
                    Nazwisko = coach.Nazwisko;
                    SelectedLicense = AvailableLicenses.Contains(coach.Licencja) ? coach.Licencja : "Inna";

                    _clubBeforeEdit = coach.AktualnyKlub;
                    if (_clubBeforeEdit != null && !AvailableClubs.Any(c => c.IdKlubu == _clubBeforeEdit.IdKlubu))
                    {
                        AvailableClubs.Add(_clubBeforeEdit);
                    }

                    SelectedClub = coach.AktualnyKlub ?? AvailableClubs.FirstOrDefault(c => c.IdKlubu == 0);
                }
                _isEditing = true;
            }
        }

        [RelayCommand]
        async Task SaveCoachAsync()
        {
            if (string.IsNullOrWhiteSpace(Imie) || string.IsNullOrWhiteSpace(Nazwisko) || string.IsNullOrWhiteSpace(SelectedLicense))
            {
                await Shell.Current.DisplayAlert("Błąd", "Wszystkie pola są wymagane.", "OK");
                return;
            }

            Coach coachToSave = new Coach
            {
                IDtrenera = _coachId,
                Imie = Imie,
                Nazwisko = Nazwisko,
                Licencja = SelectedLicense
            };

            IsBusy = true;
            try
            {
                if (_isEditing)
                {
                    await _coachService.UpdateCoachAsync(coachToSave);

                    int? newClubId = SelectedClub?.IdKlubu;
                    int? oldClubId = _clubBeforeEdit?.IdKlubu;

                    if (newClubId == 0)
                    {
                        if (oldClubId.HasValue && oldClubId.Value != 0)
                        {
                            await _coachService.ReleaseCoachFromCurrentClubAsync(_coachId, DateTime.Today);
                        }
                    }
                    else if (newClubId.HasValue && newClubId != oldClubId)
                    {
                        await _coachService.AssignCoachToClubAsync(_coachId, newClubId.Value, DateTime.Today);
                    }
                }
                else
                {
                    int? initialClubId = (SelectedClub != null && SelectedClub.IdKlubu != 0) ? SelectedClub.IdKlubu : (int?)null;
                    await _coachService.AddCoachAsync(coachToSave, initialClubId);
                }

                await Shell.Current.DisplayAlert("Sukces", _isEditing ? "Trener zaktualizowany!" : "Trener dodany!", "OK");
                MessagingCenter.Send(this, "CoachesChanged");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {

                Debug.WriteLine("--------------------------------------------------");
                Debug.WriteLine($"BŁĄD PODCZAS ZAPISYWANIA TRENERA: {ex.GetType().FullName}");
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


                string errorMessage = "Nie udało się zapisać trenera.";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nSzczegóły błędu:\n{ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"\n{ex.InnerException.InnerException.Message}";
                    }
                }
                else
                {
                    errorMessage += $"\n\nSzczegóły błędu:\n{ex.Message}";
                }
                await Shell.Current.DisplayAlert("Błąd Krytyczny", errorMessage, "OK");
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        async Task DeleteCoachAsync()
        {
            if (!_isEditing || _coachId == 0) return;
            bool confirmed = await Shell.Current.DisplayAlert("Potwierdzenie", $"Czy na pewno chcesz usunąć trenera {Imie} {Nazwisko}?", "Tak", "Nie");
            if (confirmed)
            {
                IsBusy = true;
                try
                {
                    await _coachService.DeleteCoachAsync(_coachId);
                    await Shell.Current.DisplayAlert("Sukces", "Trener usunięty.", "OK");
                    MessagingCenter.Send(this, "CoachesChanged");
                    await Shell.Current.GoToAsync("..");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Błąd usuwania trenera: {ex.ToString()}");
                    await Shell.Current.DisplayAlert("Błąd", $"Nie udało się usunąć trenera: {ex.Message}", "OK");
                }
                finally { IsBusy = false; }
            }
        }

        public async Task OnAppearing()
        {
            if (_coachId == 0 && AvailableClubs.Count <= 1 && !IsBusy)
            {
                await LoadInitialDataAsync();
            }
            else if (_coachId != 0 && (Imie == null || Nazwisko == null) && !IsBusy)
            {
                LoadCoachAsync(_coachId);
            }
        }
    }
}