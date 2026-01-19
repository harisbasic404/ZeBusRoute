using Microsoft.Maui.Controls;
using System;
using ZeBusRoute.ViewModels;
using ZeBusRoute.Services;

namespace ZeBusRoute.Pages
{
    public partial class RegisterPage : ContentPage
    {
        private readonly ProfileViewModel vm;

        public RegisterPage()
        {
            InitializeComponent();
        }

        public RegisterPage(ProfileViewModel model) : this()
        {
            vm = model;
        }

        private async void OnRegistracija(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(unosIme.Text))
            {
                await DisplayAlert("Greška", "Unesite ime.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(unosPrezime.Text))
            {
                await DisplayAlert("Greška", "Unesite prezime.", "OK");
                return;
            }

            var email = unosEmail.Text?.Trim() ?? "";
            var lozinka = unosLozinka.Text ?? "";
            var potvrda = unosPotvrda.Text ?? "";

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                await DisplayAlert("Greška", "Unesite ispravan email.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(lozinka))
            {
                await DisplayAlert("Greška", "Lozinka je obavezna.", "OK");
                return;
            }
            if (lozinka != potvrda)
            {
                await DisplayAlert("Greška", "Lozinke se ne podudaraju.", "OK");
                return;
            }

            if (UserAuthService.DaLiJeRegistrovan(email))
            {
                await DisplayAlert("Greška", "Korisnik sa ovim emailom je već registrovan.", "OK");
                return;
            }

            var uspjeh = UserAuthService.Registruj(
                unosIme.Text!.Trim(),
                unosPrezime.Text!.Trim(),
                email,
                lozinka
            );

            if (!uspjeh)
            {
                await DisplayAlert("Greška", "Došlo je do greške pri registraciji.", "OK");
                return;
            }

            var profil = UserAuthService.UcitajProfil();
            if (profil != null)
            {
                vm?.PostaviKorisnika(profil, lozinka);
            }

            await DisplayAlert("Uspjeh", "Registracija uspješna.", "OK");
            await Navigation.PopAsync();
        }

        private async void OnOtkazi(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}