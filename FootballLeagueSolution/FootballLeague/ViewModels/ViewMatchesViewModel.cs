// Plik: ViewModels/ViewMatchesViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FootballLeague.Models;
using FootballLeague.Services; // Zakładamy, że MatchService ma metodę GetMatchesAsync
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Diagnostics;

namespace FootballLeague.ViewModels
{
    public partial class ViewMatchesViewModel : BaseViewModel
    {
        private readonly MatchService _matchService;
        private readonly ClubService _clubService; // Do pobrania listy klubów dla filtrów

        // Lista wszystkich meczów pobranych z serwisu
        private List<Match> _allMatches = new List<Match>();

        public ObservableCollection<Match> Matches { get; } = new ObservableCollection<Match>();
        public ObservableCollection<Club> FilterClubs { get; } = new ObservableCollection<Club>();

        [ObservableProperty]
        DateTime? _filterDate; // Nullable DateTime dla filtra daty

        [ObservableProperty]
        Club? _filterByClub; // Dla filtra po klubie (gospodarz lub gość)

        [ObservableProperty]
        bool _isFilterDateEnabled; // Do włączania/wyłączania DatePicker

        public ViewMatchesViewModel(MatchService matchService, ClubService clubService)
        {
            _matchService = matchService;
            _clubService = clubService;
            Title = "Lista Meczów";
            // Inicjalnie filtr daty jest wyłączony, żeby pokazać wszystkie mecze
            IsFilterDateEnabled = false;
        }

        public async Task LoadInitialDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                _allMatches = await _matchService.GetMatchesAsync(); // Pobierz WSZYSTKIE mecze
                ApplyFilters(); // Zastosuj filtry (na początku pokaże wszystkie)

                if (FilterClubs.Count == 0) // Załaduj kluby tylko raz
                {
                    var clubs = await _clubService.GetClubsAsync();
                    FilterClubs.Add(new Club { IdKlubu = 0, Nazwa = "Wszystkie Kluby" }); // Opcja "Wszystkie"
                    foreach (var club in clubs)
                    {
                        FilterClubs.Add(club);
                    }
                    // Ustawienie domyślne dla filtra klubów
                    FilterByClub = FilterClubs.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd podczas ładowania danych meczów: {ex.ToString()}");
                await Shell.Current.DisplayAlert("Błąd", $"Nie udało się załadować danych: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnFilterDateChanged(DateTime? value)
        {
            // Jeśli data jest ustawiona, automatycznie włącz filtr daty
            // Jeśli użytkownik wyczyści datę (co nie jest łatwe w standardowym DatePicker),
            // można by dodać przycisk "Wyczyść filtr daty"
            IsFilterDateEnabled = value.HasValue;
            ApplyFilters();
        }

        partial void OnFilterByClubChanged(Club? value)
        {
            ApplyFilters();
        }

        // Użyj tego, jeśli chcesz mieć checkbox do włączania/wyłączania filtra daty
        // partial void OnIsFilterDateEnabledChanged(bool value)
        // {
        //     if (!value) // Jeśli filtr daty jest wyłączony
        //     {
        //         FilterDate = null; // Zresetuj datę, co wywoła OnFilterDateChanged i odświeży
        //     }
        //     ApplyFilters();
        // }


        [RelayCommand]
        void ClearDateFilter() // Przycisk do czyszczenia filtra daty
        {
            FilterDate = null; // To wywoła OnFilterDateChanged -> IsFilterDateEnabled = false -> ApplyFilters
        }


        private void ApplyFilters()
        {
            if (_allMatches == null) return;

            IEnumerable<Match> filtered = _allMatches;

            if (IsFilterDateEnabled && FilterDate.HasValue)
            {
                filtered = filtered.Where(m => m.DataMeczu.Date == FilterDate.Value.Date);
            }

            if (FilterByClub != null && FilterByClub.IdKlubu != 0) // 0 to ID dla "Wszystkie Kluby"
            {
                filtered = filtered.Where(m => m.IdGospodarza == FilterByClub.IdKlubu || m.IdGoscia == FilterByClub.IdKlubu);
            }

            Matches.Clear();
            foreach (var match in filtered.OrderByDescending(m => m.DataMeczu)) // Sortuj od najnowszych
            {
                Matches.Add(match);
            }
        }

        public async Task OnAppearing()
        {
            await LoadInitialDataAsync();
        }
    }
}