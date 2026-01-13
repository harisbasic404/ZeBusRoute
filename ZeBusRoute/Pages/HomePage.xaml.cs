using System.Collections.ObjectModel;
using ZeBusRoute.Models;
using ZeBusRoute.Services;

namespace ZeBusRoute.Pages;

public partial class HomePage : ContentPage
{
    private List<Linija> _sveLinije;
    private ObservableCollection<RezultatPretrage> _filtriraniRezultati;

    public ObservableCollection<RezultatPretrage> FiltriraniRezultati
    {
        get => _filtriraniRezultati;
        set
        {
            _filtriraniRezultati = value;
            OnPropertyChanged();
        }
    }

    public HomePage()
    {
        InitializeComponent();
        _filtriraniRezultati = new ObservableCollection<RezultatPretrage>();
        _sveLinije = new List<Linija>();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UcitajLinije();
    }

    private void UcitajLinije()
    {
        try
        {
            // Inicijaliziraj bazu ako još nije
            DataService.InitDb();
            _sveLinije = DataService.GetLinije();
        }
        catch (Exception ex)
        {
            DisplayAlert("Greška", $"Nije moguće učitati linije: {ex.Message}", "OK");
        }
    }

    private void PriPromjeniTekstaPretrage(object sender, TextChangedEventArgs e)
    {
        var tekstPretrage = e.NewTextValue?.ToLower() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(tekstPretrage))
        {
            FiltriraniRezultati.Clear();
            return;
        }

        var rezultati = new List<RezultatPretrage>();

        foreach (var linija in _sveLinije)
        {
            // Pretraži po liniji
            if (linija.Id.ToString().Contains(tekstPretrage) ||
                linija.Naziv.ToLower().Contains(tekstPretrage) ||
                linija.Smjer.ToLower().Contains(tekstPretrage))
            {
                // Dodaj polaske za ovu liniju
                var polasci = DataService.GetPolasci(linija.Id);
                var dostupniRezimi = polasci.Select(d => d.RezimDisplay).Distinct().ToList();

                rezultati.Add(new RezultatPretrage
                {
                    Linija = linija,
                    DostupniRezimi = string.Join(", ", dostupniRezimi),
                    BrojPolazaka = polasci.Count
                });
            }
            else
            {
                // Pretraži po stanicama
                var stanice = DataService.GetStanice(linija.Id);
                if (stanice.Any(s => s.Naziv.ToLower().Contains(tekstPretrage)))
                {
                    var polasci = DataService.GetPolasci(linija.Id);
                    var dostupniRezimi = polasci.Select(d => d.RezimDisplay).Distinct().ToList();

                    rezultati.Add(new RezultatPretrage
                    {
                        Linija = linija,
                        DostupniRezimi = string.Join(", ", dostupniRezimi),
                        BrojPolazaka = polasci.Count,
                        PronadenaStanica = stanice.First(s => s.Naziv.ToLower().Contains(tekstPretrage)).Naziv
                    });
                }
            }
        }

        FiltriraniRezultati.Clear();
        foreach (var rezultat in rezultati.Take(10)) // Ograniči na 10 rezultata
        {
            FiltriraniRezultati.Add(rezultat);
        }
    }

    private async void PriOdabiruLinije(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is RezultatPretrage odabraniRezultat)
        {
            // Reset selekcije
            KolekcijaLinija.SelectedItem = null;
            
            // Navigiraj na detalje linije
            await Navigation.PushAsync(new LineDetailsPage(odabraniRezultat.Linija));
        }
    }
}

// Helper klasa za rezultate pretrage
public class RezultatPretrage
{
    public Linija Linija { get; set; } = new();
    public string DostupniRezimi { get; set; } = "";
    public int BrojPolazaka { get; set; }
    public string? PronadenaStanica { get; set; }
    
    public string InfoZaPrikaz => PronadenaStanica != null 
        ? $"Prolazi kroz: {PronadenaStanica}" 
        : $"{BrojPolazaka} polazaka";
}
