using System.Collections.ObjectModel;
using ZeBusRoute.Models;
using ZeBusRoute.Services;
using Microsoft.Maui.Storage;

namespace ZeBusRoute.Pages;

public partial class LineDetailsPage : ContentPage
{
    private readonly Linija _odabranaLinija;
    private ObservableCollection<Stanica> _stanice;
    private ObservableCollection<Polazak> _polasci;
    private bool _jeOmiljeno;
    private string _ikonicaOmiljenog = string.Empty;

    public Linija OdabranaLinija => _odabranaLinija;

    public ObservableCollection<Stanica> Stanice
    {
        get => _stanice;
        set
        {
            _stanice = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Polazak> Polasci
    {
        get => _polasci;
        set
        {
            _polasci = value;
            OnPropertyChanged();
        }
    }

    public bool JeOmiljeno
    {
        get => _jeOmiljeno;
        set
        {
            _jeOmiljeno = value;
            OnPropertyChanged();
            AzurirajIkonicuOmiljenog();
        }
    }

    public string IkonicaOmiljenog
    {
        get => _ikonicaOmiljenog;
        set
        {
            _ikonicaOmiljenog = value;
            OnPropertyChanged();
        }
    }

    public LineDetailsPage(Linija linija)
    {
        InitializeComponent();
        _odabranaLinija = linija;
        _stanice = new ObservableCollection<Stanica>();
        _polasci = new ObservableCollection<Polazak>();
        BindingContext = this;

        Title = $"Linija {linija.Id}";
        _ = UcitajDetaljeLinije();
        ProvjeriStatusOmiljenog();
    }

    private async Task UcitajDetaljeLinije()
    {
        try
        {
            // Učitaj stanice
            var stanice = DataService.GetStanice(_odabranaLinija.Id);
            Stanice = new ObservableCollection<Stanica>(stanice);

            // Učitaj polaske
            var polasci = DataService.GetPolasci(_odabranaLinija.Id);
            Polasci = new ObservableCollection<Polazak>(polasci);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Nije moguće učitati detalje linije: {ex.Message}", "U redu");
        }
    }

    private void ProvjeriStatusOmiljenog()
    {
        var email = Preferences.Get("profil_email", string.Empty);
        if (!string.IsNullOrEmpty(email))
        {
            JeOmiljeno = DataService.JeLinijaOmiljena(email, _odabranaLinija.Id);
        }
        else
        {
            JeOmiljeno = false;
        }
    }

    private void AzurirajIkonicuOmiljenog()
    {
        if (_jeOmiljeno)
        {
            IkonicaOmiljenog = "srce_puno.png";
        }
        else
        {
            IkonicaOmiljenog = "srce_prazno.png";
        }
    }

    private async void OnKlikOmiljeno(object sender, EventArgs e)
    {
        var email = Preferences.Get("profil_email", string.Empty);
        
        if (string.IsNullOrEmpty(email))
        {
            await DisplayAlert("Prijava potrebna", "Morate biti prijavljeni da biste dodali linije u omiljene.", "U redu");
            return;
        }

        try
        {
            if (JeOmiljeno)
            {
                DataService.UkloniOmiljeno(email, _odabranaLinija.Id);
                JeOmiljeno = false;
                await DisplayAlert("Uklonjeno", "Linija je uklonjena iz omiljenih.", "U redu");
            }
            else
            {
                DataService.DodajOmiljeno(email, _odabranaLinija.Id);
                JeOmiljeno = true;
                await DisplayAlert("Dodano", "Linija je dodana u omiljene.", "U redu");
            }
        }
                catch (Exception ex)
                {
                    await DisplayAlert("Greška", $"Nije moguće ažurirati omiljene: {ex.Message}", "U redu");
                }
            }
        }
