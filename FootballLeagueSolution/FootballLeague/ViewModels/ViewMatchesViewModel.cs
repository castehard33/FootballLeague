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
    public partial class ViewMatchesViewModel : BaseViewModel 
    {
        private readonly MatchService _matchService;
        private readonly ClubService _clubService;

        private List<Match> _allMatches = [];

        public ObservableCollection<Match> Matches { get; } = [];
        public ObservableCollection<Club> FilterClubs { get; } = [];

        [ObservableProperty]
        DateTime? _filterDate;

        [ObservableProperty]
        Club? _filterByClub; 

        [ObservableProperty]
        bool _isFilterDateEnabled; 
        public ViewMatchesViewModel(MatchService matchService, ClubService clubService)
        {
            _matchService = matchService;
            _clubService = clubService;
            Title = "Lista Meczów";
            IsFilterDateEnabled = false; 
        }

        public async Task LoadInitialDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                _allMatches = await _matchService.GetMatchesAsync();
                ApplyFilters();

                if (FilterClubs.Count == 0)
                {
                    var clubs = await _clubService.GetClubsAsync();
                    FilterClubs.Add(new Club { IdKlubu = 0, Nazwa = "Wszystkie Kluby" });
                    foreach (var club in clubs)
                    {
                        FilterClubs.Add(club);
                    }
                    FilterByClub = FilterClubs.FirstOrDefault(); 
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd podczas ładowania danych meczów: {ex}");
                await Shell.Current.DisplayAlert("Błąd", $"Nie udało się załadować danych: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }


        partial void OnFilterDateChanged(DateTime? value)
        {
            IsFilterDateEnabled = value.HasValue; 
            ApplyFilters();
        }

        partial void OnFilterByClubChanged(Club? value)
        {
            ApplyFilters();
        }

        partial void OnIsFilterDateEnabledChanged(bool value)
        {
            if (!value)
            {
                FilterDate = null; 
            }
            ApplyFilters();
        }

        [RelayCommand]
        void ClearDateFilter()
        {
            FilterDate = null; 
        }

        private void ApplyFilters()
        {
            if (_allMatches == null) return;

            IEnumerable<Match> filtered = _allMatches;


            if (IsFilterDateEnabled && FilterDate.HasValue)
            {
                filtered = filtered.Where(m => m.DataMeczu.Date == FilterDate.Value.Date);
            }

            if (FilterByClub != null && FilterByClub.IdKlubu != 0)
            {
                filtered = filtered.Where(m => m.IdGospodarza == FilterByClub.IdKlubu || m.IdGoscia == FilterByClub.IdKlubu);
            }

            Matches.Clear();
            foreach (var match in filtered.OrderByDescending(m => m.DataMeczu))
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