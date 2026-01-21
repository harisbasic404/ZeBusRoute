using Microsoft.Maui.Storage;
using ZeBusRoute.ViewModels;

namespace ZeBusRoute.Services
{
    public static class UserAuthService
    {
        private const string KEY_EMAIL = "auth_email";
        private const string KEY_PASSWORD = "auth_password";
        private const string KEY_LOGGED_IN = "auth_logged_in";

        private const string KEY_PROFIL_IME = "profil_ime";
        private const string KEY_PROFIL_PREZIME = "profil_prezime";
        private const string KEY_PROFIL_TEL = "profil_tel";
        private const string KEY_PROFIL_SLIKA = "profil_slika";

        private static string NormalizeEmail(string email) => (email ?? string.Empty).Trim().ToLowerInvariant();
        private static string NormalizePassword(string password) => (password ?? string.Empty).Trim();

        public static bool DaLiJeRegistrovan(string email)
        {
            var emailNorm = NormalizeEmail(email);
            var sacuvaniEmail = Preferences.Get(KEY_EMAIL, "");
            return !string.IsNullOrWhiteSpace(sacuvaniEmail) &&
                   NormalizeEmail(sacuvaniEmail) == emailNorm;
        }

        public static bool Registruj(string ime, string prezime, string email, string lozinka)
        {
            var emailNorm = NormalizeEmail(email);
            var passNorm = NormalizePassword(lozinka);

            if (DaLiJeRegistrovan(emailNorm))
                return false;

            Preferences.Set(KEY_EMAIL, emailNorm);
            Preferences.Set(KEY_PASSWORD, passNorm);
            Preferences.Set(KEY_LOGGED_IN, true);

            Preferences.Set(KEY_PROFIL_IME, ime);
            Preferences.Set(KEY_PROFIL_PREZIME, prezime);
            Preferences.Set(KEY_PROFIL_TEL, "");
            Preferences.Set(KEY_PROFIL_SLIKA, "");

            return true;
        }

        public static bool Prijava(string email, string lozinka)
        {
            var emailNorm = NormalizeEmail(email);
            var lozinkaNorm = NormalizePassword(lozinka);

            var sacuvaniEmail = Preferences.Get(KEY_EMAIL, "");
            var sacuvanaLozinka = Preferences.Get(KEY_PASSWORD, "");

            if (string.IsNullOrWhiteSpace(sacuvaniEmail))
                return false;

            if (NormalizeEmail(sacuvaniEmail) != emailNorm)
                return false;

            if (NormalizePassword(sacuvanaLozinka) != lozinkaNorm)
                return false;

            Preferences.Set(KEY_LOGGED_IN, true);
            return true;
        }

        public static void Odjava()
        {
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
                Preferences.Set(KEY_EMAIL, NormalizeEmail(korisnik.Email));

            Preferences.Set(KEY_PROFIL_IME, korisnik.Ime ?? "");
            Preferences.Set(KEY_PROFIL_PREZIME, korisnik.Prezime ?? "");
            Preferences.Set(KEY_PROFIL_TEL, korisnik.BrojTelefona ?? "");
            Preferences.Set(KEY_PROFIL_SLIKA, korisnik.SlikaPutanja ?? "");

            if (!string.IsNullOrWhiteSpace(novaLozinka))
                Preferences.Set(KEY_PASSWORD, NormalizePassword(novaLozinka));
        }
    }
}