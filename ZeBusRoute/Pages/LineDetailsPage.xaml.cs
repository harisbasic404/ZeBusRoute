using System.Collections.ObjectModel;
using System.Linq;
using ZeBusRoute.Models;
using ZeBusRoute.Services;
using Microsoft.Maui.Storage;

namespace ZeBusRoute.Pages;

public partial class LineDetailsPage : ContentPage
{
    private readonly Linija _odabranaLinija;
    private readonly string _odabranoVrijemeUDanu;
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

    public LineDetailsPage(Linija linija, string odabranoVrijemeUDanu)
    {
        InitializeComponent();
        _odabranaLinija = linija;
        _odabranoVrijemeUDanu = string.IsNullOrWhiteSpace(odabranoVrijemeUDanu) ? "Jutro" : odabranoVrijemeUDanu;
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
            var stanice = DataService.GetStanice(_odabranaLinija.Id);
            Stanice = new ObservableCollection<Stanica>(stanice);

            var polasci = DataService.GetPolasci(_odabranaLinija.Id);
            var filtrirani = polasci.Where(p => FiltrirajPoVremenuDana(p.Vrijeme)).ToList();
            Polasci = new ObservableCollection<Polazak>(filtrirani);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Nije moguće učitati detalje linije: {ex.Message}", "U redu");
        }
    }

    private bool FiltrirajPoVremenuDana(TimeOnly vrijeme)
    {
        int sat = vrijeme.Hour;
        return _odabranoVrijemeUDanu switch
        {
            "Jutro" => sat >= 5 && sat < 12,
            "Podne" => sat >= 12 && sat < 17,
            "Vecer" => sat >= 17 || sat < 5,
            _ => true
        };
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
