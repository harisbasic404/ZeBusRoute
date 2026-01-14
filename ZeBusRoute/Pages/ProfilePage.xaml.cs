using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage; // Preferences

namespace ZeBusRoute.Pages
{
    public partial class ProfilePage : ContentPage
    {
        // Stanje korisnika
        private bool jePrijavljen = false;

        // Postavke
        private string trenutniJezik = "English";
        private bool jeTamniMod = false;
        private bool dozvoliNotifikacije = true;

        // UI reference preko FindByName (negenerička + cast)
        private Label oznakaJezikRef => (Label)FindByName("oznakaJezik");
        private Switch prekidacTamniModRef => (Switch)FindByName("prekidacTamniMod");
        private Switch prekidacNotifikacijeRef => (Switch)FindByName("prekidacNotifikacije");
        private Button dugmePrijavaRef => (Button)FindByName("dugmePrijava");
        private Button dugmeKreirajNalogRef => (Button)FindByName("dugmeKreirajNalog");
        private Button dugmePosaljiPrijavuRef => (Button)FindByName("dugmePosaljiPrijavu");
        private Button dugmeRegistracijaRef => (Button)FindByName("dugmeRegistracija");
        private Entry unosEmailRef => (Entry)FindByName("unosEmail");
        private Entry unosLozinkaRef => (Entry)FindByName("unosLozinka");
        private Label oznakaStatusRef => (Label)FindByName("oznakaStatus");

        // Ključevi za Preferences
        private const string KLJUC_EMAIL = "profil_email";
        private const string KLJUC_TOKEN = "profil_token";
        private const string KLJUC_JEZIK = "postavka_jezik";
        private const string KLJUC_TAMNI = "postavka_tamni";
        private const string KLJUC_NOTIF = "postavka_notif";

        public ProfilePage()
        {
            InitializeComponent();

            // Učitati lokalno sačuvane postavke/profil
            trenutniJezik = Preferences.Get(KLJUC_JEZIK, "English");
            jeTamniMod = Preferences.Get(KLJUC_TAMNI, false);
            dozvoliNotifikacije = Preferences.Get(KLJUC_NOTIF, true);

            var sacuvaniEmail = Preferences.Get(KLJUC_EMAIL, string.Empty);
            var sacuvaniToken = Preferences.Get(KLJUC_TOKEN, string.Empty);
            jePrijavljen = !string.IsNullOrEmpty(sacuvaniEmail) && !string.IsNullOrEmpty(sacuvaniToken);

            // Inicijalni prikaz
            if (oznakaJezikRef != null) oznakaJezikRef.Text = trenutniJezik;
            if (prekidacTamniModRef != null) prekidacTamniModRef.IsToggled = jeTamniMod;
            if (prekidacNotifikacijeRef != null) prekidacNotifikacijeRef.IsToggled = dozvoliNotifikacije;

            AzurirajUIZaPrijavu();
        }

        private void AzurirajUIZaPrijavu()
        {
            if (dugmePrijavaRef != null) dugmePrijavaRef.IsVisible = !jePrijavljen;
            if (dugmeKreirajNalogRef != null) dugmeKreirajNalogRef.IsVisible = !jePrijavljen;

            if (oznakaStatusRef != null)
                oznakaStatusRef.Text = jePrijavljen ? "Prijavljeni ste." : "Niste prijavljeni.";

            if (dugmePosaljiPrijavuRef != null)
                dugmePosaljiPrijavuRef.Text = jePrijavljen ? "Odjava" : "Prijava";

            if (dugmeRegistracijaRef != null)
                dugmeRegistracijaRef.IsEnabled = !jePrijavljen;
        }

        // Dugme na vrhu - SIGN IN info
        private async void OnKlikPrijava(object sender, EventArgs e)
        {
            await DisplayAlert("Prijava", "Unesite email i lozinku u polja ispod, pa kliknite Prijava.", "OK");
        }

        // Dugme - KREIRAJ NALOG (popup objašnjenje)
        private async void OnKreirajNalog(object sender, EventArgs e)
        {
            await DisplayAlert("Kreiranje naloga", "Unesite email i lozinku u polja ispod, pa kliknite Registracija.", "OK");
        }

        // Toggling Dark Mode
        private void OnPromjenaTamniMod(object sender, ToggledEventArgs e)
        {
            jeTamniMod = e.Value;
            Preferences.Set(KLJUC_TAMNI, jeTamniMod);
            Application.Current.UserAppTheme = jeTamniMod ? AppTheme.Dark : AppTheme.Light;
        }

        // Toggling Notifications
        private void OnPromjenaNotifikacije(object sender, ToggledEventArgs e)
        {
            dozvoliNotifikacije = e.Value;
            Preferences.Set(KLJUC_NOTIF, dozvoliNotifikacije);
        }

        // Prijava ili Odjava
        private async void OnPosaljiPrijavu(object sender, EventArgs e)
        {
            if (!jePrijavljen)
            {
                var email = unosEmailRef?.Text?.Trim() ?? string.Empty;
                var lozinka = unosLozinkaRef?.Text ?? string.Empty;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(lozinka))
                {
                    await DisplayAlert("Greška", "Unesite email i lozinku.", "OK");
                    return;
                }

                // Jednostavna lokalna provjera
                if (email.Contains("@") && lozinka.Length >= 4)
                {
                    // Simulacija tokena, u pravoj aplikaciji dobijen od API-a
                    var token = Guid.NewGuid().ToString("N");

                    // Sačuvati profil lokalno
                    Preferences.Set(KLJUC_EMAIL, email);
                    Preferences.Set(KLJUC_TOKEN, token);

                    jePrijavljen = true;
                    await DisplayAlert("Uspjeh", "Uspješno ste se prijavili.", "OK");
                }
                else
                {
                    await DisplayAlert("Greška", "Neispravni podaci za prijavu.", "OK");
                }
            }
            else
            {
                // Odjava i brisanje lokalnih podataka
                jePrijavljen = false;
                Preferences.Remove(KLJUC_EMAIL);
                Preferences.Remove(KLJUC_TOKEN);
                if (unosLozinkaRef != null) unosLozinkaRef.Text = string.Empty;
                await DisplayAlert("Odjava", "Odjavljeni ste.", "OK");
            }

            AzurirajUIZaPrijavu();
        }

        // Registracija (kreiranje naloga) i spremanje profila
        private async void OnPosaljiRegistraciju(object sender, EventArgs e)
        {
            var email = unosEmailRef?.Text?.Trim() ?? string.Empty;
            var lozinka = unosLozinkaRef?.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(lozinka))
            {
                await DisplayAlert("Greška", "Unesite email i lozinku.", "OK");
                return;
            }

            if (!email.Contains("@"))
            {
                await DisplayAlert("Greška", "Email nije ispravan.", "OK");
                return;
            }

            if (lozinka.Length < 4)
            {
                await DisplayAlert("Greška", "Lozinka mora imati najmanje 4 znaka.", "OK");
                return;
            }

            // Simuliraj uspješnu registraciju i odmah spremi profil
            var token = Guid.NewGuid().ToString("N");
            Preferences.Set(KLJUC_EMAIL, email);
            Preferences.Set(KLJUC_TOKEN, token);

            jePrijavljen = true;
            await DisplayAlert("Uspjeh", "Registracija uspješna. Prijavljeni ste.", "OK");

            AzurirajUIZaPrijavu();
        }
    }
}