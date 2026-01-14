using Microsoft.Maui.Controls;
using System;
using ZeBusRoute.ViewModels;

namespace ZeBusRoute.Pages
{
    public partial class RegisterPage : ContentPage
    {
        private readonly ProfileViewModel vm;

        // Konstruktor bez parametara (postojeći)
        public RegisterPage()
        {
            InitializeComponent();
        }

        // Novi overload sa ViewModel-om (rješava CS1729)
        public RegisterPage(ProfileViewModel model) : this()
        {
            vm = model;
            BindingContext = vm;
        }

        private void OnOdabirEmail(object sender, EventArgs e)
        {
            formaEmail.IsVisible = true;
            formaTelefon.IsVisible = false;
            linijaEmail.BackgroundColor = Color.FromArgb("#66BB6A");
            linijaTelefon.BackgroundColor = Color.FromArgb("#DDDDDD");
        }

        private void OnOdabirTelefon(object sender, EventArgs e)
        {
            formaEmail.IsVisible = false;
            formaTelefon.IsVisible = true;
            linijaEmail.BackgroundColor = Color.FromArgb("#DDDDDD");
            linijaTelefon.BackgroundColor = Color.FromArgb("#66BB6A");
        }

        private async void OnRegistracija(object sender, EventArgs e)
        {
            if (formaEmail.IsVisible)
            {
                if (string.IsNullOrWhiteSpace(unosIme.Text)) { await DisplayAlert("Greška", "Unesite ime.", "OK"); return; }
                if (string.IsNullOrWhiteSpace(unosPrezime.Text)) { await DisplayAlert("Greška", "Unesite prezime.", "OK"); return; }
                var email = unosEmail.Text?.Trim() ?? "";
                var lozinka = unosLozinka.Text ?? "";
                var potvrda = unosPotvrda.Text ?? "";
                if (string.IsNullOrWhiteSpace(email) || !email.Contains("@")) { await DisplayAlert("Greška", "Unesite ispravan email.", "OK"); return; }
                if (string.IsNullOrWhiteSpace(lozinka)) { await DisplayAlert("Greška", "Lozinka je obavezna.", "OK"); return; }
                if (lozinka != potvrda) { await DisplayAlert("Greška", "Lozinke se ne podudaraju.", "OK"); return; }

                // Spoji sa VM ako je proslijeđena
                vm?.PostaviKorisnika(new KorisnikModel { Ime = unosIme.Text?.Trim() ?? "", Prezime = unosPrezime.Text?.Trim() ?? "", Email = email });
            }
            else
            {
                var telefon = unosTelefon.Text?.Trim() ?? "";
                var kod = unosKod.Text?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(telefon)) { await DisplayAlert("Greška", "Unesite broj telefona.", "OK"); return; }
                if (string.IsNullOrWhiteSpace(kod)) { await DisplayAlert("Greška", "Unesite verifikacijski kod.", "OK"); return; }
                // Po želji: vm?.PostaviKorisnika(...) i spremi telefon
            }

            await DisplayAlert("Uspjeh", "Registracija uspješna.", "OK");
            await Navigation.PopAsync();
        }

        private async void OnOtkazi(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            OnOdabirEmail(this, EventArgs.Empty);
        }
    }
}