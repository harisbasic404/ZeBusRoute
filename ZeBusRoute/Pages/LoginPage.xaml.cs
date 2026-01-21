using Microsoft.Maui.Controls;
using System;
using ZeBusRoute.Services;
using ZeBusRoute.ViewModels;

namespace ZeBusRoute.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly ProfileViewModel vm;

        public LoginPage(ProfileViewModel? viewModel = null)
        {
            InitializeComponent();
            vm = viewModel ?? new ProfileViewModel();
        }

        private async void OnPrijava(object sender, EventArgs e)
        {
            var email = (unosEmail.Text ?? "").Trim();
            var lozinka = (unosLozinka.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Greška", "Unesite email.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(lozinka))
            {
                await DisplayAlert("Greška", "Unesite lozinku.", "OK");
                return;
            }

            var uspjeh = UserAuthService.Prijava(email, lozinka);
            if (!uspjeh)
            {
                await DisplayAlert("Greška", "Email ili lozinka nisu ispravni.", "OK");
                return;
            }

            var profil = UserAuthService.UcitajProfil();
            if (profil != null)
            {
                vm.PostaviKorisnika(profil);
            }

            await DisplayAlert("Uspjeh", "Prijava uspješna.", "OK");
            await Navigation.PopAsync();
        }

        private async void OnOtkazi(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}