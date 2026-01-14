using Microsoft.Maui.Controls;
using System;

namespace ZeBusRoute.Pages
{
    // This file should exist alongside LoginPage.xaml
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        // Tab switching like screenshot
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

        private async void OnPrijava(object sender, EventArgs e)
        {
            if (formaEmail.IsVisible)
            {
                var email = unosEmail.Text?.Trim() ?? "";
                var lozinka = unosLozinka.Text ?? "";
                if (string.IsNullOrWhiteSpace(email)) { await DisplayAlert("Greška", "Unesite email.", "OK"); return; }
                if (string.IsNullOrWhiteSpace(lozinka)) { await DisplayAlert("Greška", "Unesite lozinku.", "OK"); return; }
                await DisplayAlert("Uspjeh", "Prijava uspješna.", "OK");
            }
            else
            {
                var telefon = unosTelefon.Text?.Trim() ?? "";
                var kod = unosKod.Text?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(telefon)) { await DisplayAlert("Greška", "Unesite broj telefona.", "OK"); return; }
                if (string.IsNullOrWhiteSpace(kod)) { await DisplayAlert("Greška", "Unesite verifikacijski kod.", "OK"); return; }
                await DisplayAlert("Uspjeh", "Prijava uspješna.", "OK");
            }
            await Navigation.PopAsync();
        }

        private async void OnOtkazi(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // default: Email selected
            OnOdabirEmail(this, EventArgs.Empty);
        }
    }
}