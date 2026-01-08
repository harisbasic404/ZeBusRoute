using System.Collections.ObjectModel;
using System.Diagnostics;
using ZeBusRoute.Models;
using ZeBusRoute.Services;

namespace ZeBusRoute.Pages;

public partial class RoutesPage : ContentPage
{
    public ObservableCollection<Linija> Linije { get; set; } = new();

    public RoutesPage()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            DataService.InitDb(); // Kreira/puni bazu ako treba
            Linije = new ObservableCollection<Linija>(DataService.GetLinije());
            RoutesListView.ItemsSource = Linije;
            BindingContext = this;
        }
        catch (Exception ex)
        {
            DisplayAlert("Greška", $"Baza: {ex.Message}", "OK");
            Debug.WriteLine($"Error: {ex}");
        }
    }

    private async void OnRouteSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Linija linija)
        {
            var stanice = DataService.GetStanice(linija.Id);
            var polasci = DataService.GetPolasci(linija.Id);

            var detalji = $"🚌 {linija.Naziv}\n\n" +
                          $"📍 Stanica: {stanice.Count}\n" +
                          $"🕒 Polazaka: {polasci.Count}\n\n" +
                          $"Prva stanica: {stanice.FirstOrDefault()?.Naziv}\n" +
                          $"Zadnji polazak: {polasci.LastOrDefault()?.Vrijeme}";

            await DisplayAlert("Detalji rute", detalji, "OK");
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
