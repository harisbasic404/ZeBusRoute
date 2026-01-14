using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace ZeBusRoute.ViewModels
{
    public class KorisnikModel
    {
        public string Ime { get; set; } = "";
        public string Prezime { get; set; } = "";
        public string Email { get; set; } = "";
        public string BrojTelefona { get; set; } = "";
        public string? SlikaPutanja { get; set; } // new
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
            var email = Preferences.Get("profil_email", "");
            var ime = Preferences.Get("profil_ime", "");
            var prezime = Preferences.Get("profil_prezime", "");
            var tel = Preferences.Get("profil_tel", "");
            var slika = Preferences.Get("profil_slika", null as string);

            JePrijavljen = !string.IsNullOrWhiteSpace(email);
            Korisnik = new KorisnikModel { Ime = ime, Prezime = prezime, Email = email, BrojTelefona = tel, SlikaPutanja = slika };

            OtvoriPrijavuKomanda = new Command(async () =>
            {
                await Application.Current.MainPage.Navigation.PushAsync(new Pages.LoginPage());
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
                JePrijavljen = false;
                Korisnik = new KorisnikModel();
                Preferences.Remove("profil_email");
                Preferences.Remove("profil_ime");
                Preferences.Remove("profil_prezime");
                Preferences.Remove("profil_tel");
                Preferences.Remove("profil_slika");
                await Application.Current.MainPage.DisplayAlert("Odjava", "Odjavljeni ste.", "OK");
            });
        }

        public void PostaviKorisnika(KorisnikModel k)
        {
            Korisnik = k;
            JePrijavljen = !string.IsNullOrWhiteSpace(k.Email);
            Preferences.Set("profil_email", k.Email ?? "");
            Preferences.Set("profil_ime", k.Ime ?? "");
            Preferences.Set("profil_prezime", k.Prezime ?? "");
            Preferences.Set("profil_tel", k.BrojTelefona ?? "");
            Preferences.Set("profil_slika", k.SlikaPutanja ?? "");
        }
    }
}