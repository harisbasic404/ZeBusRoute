using System.Collections.ObjectModel;
using ZeBusRoute.Models;
using ZeBusRoute.Services;

namespace ZeBusRoute.Pages;

public partial class LineDetailsPage : ContentPage
{
    private readonly Linija _odabranaLinija;
    private ObservableCollection<Stanica> _stanice;
    private ObservableCollection<Polazak> _polasci;

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

    public LineDetailsPage(Linija linija)
    {
        InitializeComponent();
        _odabranaLinija = linija;
        _stanice = new ObservableCollection<Stanica>();
        _polasci = new ObservableCollection<Polazak>();
        BindingContext = this;

        Title = $"Linija {linija.Id}";
        UcitajDetaljeLinije();
    }

    private void UcitajDetaljeLinije()
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
            DisplayAlert("Greška", $"Nije moguće učitati detalje linije: {ex.Message}", "OK");
        }
    }
}
