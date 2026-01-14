using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using ZeBusRoute.ViewModels;

namespace ZeBusRoute.Pages
{
    public partial class EditProfilePage : ContentPage
    {
        private readonly ProfileViewModel vm;
        public EditProfilePage(ProfileViewModel model)
        {
            InitializeComponent();
            vm = model;
            // Popuni postojeće vrijednosti
            unosIme.Text = vm.Korisnik.Ime;
            unosPrezime.Text = vm.Korisnik.Prezime;
            unosEmail.Text = vm.Korisnik.Email;
            unosTel.Text = vm.Korisnik.BrojTelefona;

            // Postavi sliku ako postoji
            if (!string.IsNullOrWhiteSpace(vm.Korisnik.SlikaPutanja))
            {
                slikaProfil.Source = ImageSource.FromFile(vm.Korisnik.SlikaPutanja);
            }
            else
            {
                slikaProfil.Source = "default_avatar.png"; // ensure this exists in Resources
            }
        }

        private async void OnSpasi(object sender, System.EventArgs e)
        {
            vm.PostaviKorisnika(new KorisnikModel
            {
                Ime = unosIme.Text?.Trim() ?? "",
                Prezime = unosPrezime.Text?.Trim() ?? "",
                Email = unosEmail.Text?.Trim() ?? "",
                BrojTelefona = unosTel.Text?.Trim() ?? "",
                SlikaPutanja = vm.Korisnik.SlikaPutanja // keep current path
            });
            await DisplayAlert("Uspjeh", "Profil ažuriran.", "OK");
            await Navigation.PopAsync();
        }

        private async void OnOtkazi(object sender, System.EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnPromijeniSliku(object sender, System.EventArgs e)
        {
            // Use MediaPicker for photo selection
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                // Fallback to file picker
                var file = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Odaberite sliku profila",
                    FileTypes = FilePickerFileType.Images
                });
                if (file != null)
                {
                    vm.Korisnik.SlikaPutanja = file.FullPath;
                    slikaProfil.Source = ImageSource.FromFile(file.FullPath);
                }
                return;
            }

            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Odaberite sliku profila"
            });

            if (photo != null)
            {
                vm.Korisnik.SlikaPutanja = photo.FullPath;
                slikaProfil.Source = ImageSource.FromFile(photo.FullPath);
            }
        }
    }
}