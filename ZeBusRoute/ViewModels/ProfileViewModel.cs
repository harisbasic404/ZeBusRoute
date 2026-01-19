using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using ZeBusRoute.Services;

namespace ZeBusRoute.ViewModels
{
    public class KorisnikModel
    {
        public string Ime { get; set; } = "";
        public string Prezime { get; set; } = "";
        public string Email { get; set; } = "";
        public string BrojTelefona { get; set; } = "";
        public string? SlikaPutanja { get; set; }
        public string ImePrezime => string.IsNullOrWhiteSpace(Prezime) ? Ime : $"{Ime} {Prezime}";
    }

    public class ProfileViewModel : BindableObject
    {
        private bool _jePrijavljen;
        public bool JePrijavljen
        {
            get => _jePrijavljen;
            set { _jePrijavljen = value; OnPropertyChanged(); }
        }

        private KorisnikModel _korisnik = new();
        public KorisnikModel Korisnik
        {
            get => _korisnik;
            set { _korisnik = value; OnPropertyChanged(); }
        }

        private bool _jeTamniRežim = Preferences.Get("postavka_tamni", false);
        public bool JeTamniRežim
        {
            get => _jeTamniRežim;
            set { _jeTamniRežim = value; OnPropertyChanged(); Preferences.Set("postavka_tamni", value); }
        }

        private bool _obavijestiUkljucene = Preferences.Get("postavka_notif", true);
        public bool ObavijestiUkljucene
        {
            get => _obavijestiUkljucene;
            set { _obavijestiUkljucene = value; OnPropertyChanged(); Preferences.Set("postavka_notif", value); }
        }

        public ICommand OtvoriPrijavuKomanda { get; }
        public ICommand OtvoriRegistracijuKomanda { get; }
        public ICommand OtvoriUrediProfilKomanda { get; }
        public ICommand OdjavaKomanda { get; }

        public ProfileViewModel()
        {
            // Učitavanje stanja prijave i profila preko servisa
            JePrijavljen = UserAuthService.JePrijavljen();
            var profil = UserAuthService.UcitajProfil();
            if (profil != null)
                Korisnik = profil;

            OtvoriPrijavuKomanda = new Command(async () =>
            {
                await Application.Current.MainPage.Navigation.PushAsync(new Pages.LoginPage(this));
            });

            OtvoriRegistracijuKomanda = new Command(async () =>
            {
                await Application.Current.MainPage.Navigation.PushAsync(new Pages.RegisterPage(this));
            });

            OtvoriUrediProfilKomanda = new Command(async () =>
            {
                await Application.Current.MainPage.Navigation.PushAsync(new Pages.EditProfilePage(this));
            });

            OdjavaKomanda = new Command(async () =>
            {
                UserAuthService.Odjava();
                JePrijavljen = false;
                Korisnik = new KorisnikModel();
                await Application.Current.MainPage.DisplayAlert("Odjava", "Odjavljeni ste.", "OK");
            });
        }

        // Poziva se nakon registracije, prijave ili uređivanja profila
        public void PostaviKorisnika(KorisnikModel k, string? novaLozinka = null)
        {
            Korisnik = k;
            JePrijavljen = !string.IsNullOrWhiteSpace(k.Email);
            UserAuthService.SpasiProfil(k, novaLozinka);
            Preferences.Set("auth_logged_in", JePrijavljen);
        }
    }
}