using Microsoft.Maui.Storage;
using ZeBusRoute.ViewModels;

namespace ZeBusRoute.Services
{
    // Jednostavan servis za rad sa korisnikom (lokalno čuvanje u Preferences)
    public static class UserAuthService
    {
        // Ključevi u Preferences
        private const string KEY_EMAIL = "auth_email";
        private const string KEY_PASSWORD = "auth_password";
        private const string KEY_LOGGED_IN = "auth_logged_in";

        private const string KEY_PROFIL_IME = "profil_ime";
        private const string KEY_PROFIL_PREZIME = "profil_prezime";
        private const string KEY_PROFIL_TEL = "profil_tel";
        private const string KEY_PROFIL_SLIKA = "profil_slika";

        public static bool DaLiJeRegistrovan(string email)
        {
            var sacuvaniEmail = Preferences.Get(KEY_EMAIL, "");
            return !string.IsNullOrWhiteSpace(sacuvaniEmail) &&
                   string.Equals(sacuvaniEmail, email, StringComparison.OrdinalIgnoreCase);
        }

        public static bool Registruj(string ime, string prezime, string email, string lozinka)
        {
            // Ako već postoji isti email – ne registruj
            if (DaLiJeRegistrovan(email))
                return false;

            Preferences.Set(KEY_EMAIL, email);
            Preferences.Set(KEY_PASSWORD, lozinka);
            Preferences.Set(KEY_LOGGED_IN, true);

            // Osnovni profil podaci
            Preferences.Set(KEY_PROFIL_IME, ime);
            Preferences.Set(KEY_PROFIL_PREZIME, prezime);
            Preferences.Set(KEY_PROFIL_TEL, "");          // prazan početno
            Preferences.Set(KEY_PROFIL_SLIKA, "");        // nema slike na početku

            return true;
        }

        public static bool Prijava(string email, string lozinka)
        {
            var sacuvaniEmail = Preferences.Get(KEY_EMAIL, "");
            var sacuvanaLozinka = Preferences.Get(KEY_PASSWORD, "");

            if (string.IsNullOrWhiteSpace(sacuvaniEmail))
                return false;

            if (!string.Equals(sacuvaniEmail, email, StringComparison.OrdinalIgnoreCase))
                return false;

            if (sacuvanaLozinka != lozinka)
                return false;

            Preferences.Set(KEY_LOGGED_IN, true);
            return true;
        }

        public static void Odjava()
        {
            // Ne brišemo kompletan nalog (email/lozinka), samo flag da je trenutno prijavljen
            Preferences.Set(KEY_LOGGED_IN, false);
        }

        public static bool JePrijavljen()
        {
            return Preferences.Get(KEY_LOGGED_IN, false);
        }

        public static KorisnikModel? UcitajProfil()
        {
            var email = Preferences.Get(KEY_EMAIL, "");
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var ime = Preferences.Get(KEY_PROFIL_IME, "");
            var prezime = Preferences.Get(KEY_PROFIL_PREZIME, "");
            var tel = Preferences.Get(KEY_PROFIL_TEL, "");
            var slika = Preferences.Get(KEY_PROFIL_SLIKA, "");

            return new KorisnikModel
            {
                Ime = ime,
                Prezime = prezime,
                Email = email,
                BrojTelefona = tel,
                SlikaPutanja = string.IsNullOrWhiteSpace(slika) ? null : slika
            };
        }

        public static void SpasiProfil(KorisnikModel korisnik, string? novaLozinka = null)
        {
            if (!string.IsNullOrWhiteSpace(korisnik.Email))
                Preferences.Set(KEY_EMAIL, korisnik.Email);

            Preferences.Set(KEY_PROFIL_IME, korisnik.Ime ?? "");
            Preferences.Set(KEY_PROFIL_PREZIME, korisnik.Prezime ?? "");
            Preferences.Set(KEY_PROFIL_TEL, korisnik.BrojTelefona ?? "");
            Preferences.Set(KEY_PROFIL_SLIKA, korisnik.SlikaPutanja ?? "");

            if (!string.IsNullOrWhiteSpace(novaLozinka))
                Preferences.Set(KEY_PASSWORD, novaLozinka);
        }
    }
}