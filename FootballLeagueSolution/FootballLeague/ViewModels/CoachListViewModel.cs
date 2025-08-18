using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FootballLeague.Models;
using FootballLeague.Services;
using FootballLeague.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System;

namespace FootballLeague.ViewModels
{
    public partial class CoachListViewModel : BaseViewModel
    {
        private readonly CoachService _coachService;
        public ObservableCollection<Coach> Coaches { get; } = new();

        public CoachListViewModel(CoachService coachService)
        {
            _coachService = coachService;
            Title = "Trenerzy";
            MessagingCenter.Subscribe<AddEditCoachViewModel>(this, "CoachesChanged", async (sender) =>
            {
                await LoadCoachesAsync();
            });
        }

        [RelayCommand]
        async Task LoadCoachesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Coaches.Clear();
                var coaches = await _coachService.GetCoachesAsync();
                foreach (var coach in coaches)
                {
                    Coaches.Add(coach);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd ładowania trenerów: {ex.ToString()}");
                await Shell.Current.DisplayAlert("Błąd", $"Nie udało się załadować listy trenerów: {ex.Message}", "OK");
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        async Task GoToAddCoachAsync()
        {
            await Shell.Current.GoToAsync($"{nameof(AddEditCoachPage)}?coachId=0");
        }

        [RelayCommand]
        async Task GoToEditCoachAsync(Coach coach)
        {
            if (coach == null) return;
            await Shell.Current.GoToAsync($"{nameof(AddEditCoachPage)}?coachId={coach.IDtrenera}");
        }

        public async Task OnAppearing()
        {
            await LoadCoachesAsync();
        }
    }
}