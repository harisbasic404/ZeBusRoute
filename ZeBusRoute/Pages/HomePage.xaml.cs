using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;                    // MapSpan, Distance
using Microsoft.Maui.Devices.Sensors;        // Geolocation, GeolocationRequest
using Position = Microsoft.Maui.Devices.Sensors.Location;
using ZeBusRoute.ViewModels;
using ZeBusRoute.Models;

namespace ZeBusRoute.Pages;

public partial class HomePage : ContentPage
{
    private readonly List<Station> _stations;
    private readonly Dictionary<string, StationDetails> _stationDetails;
    private readonly MapSpan _zenicaRegion = MapSpan.FromCenterAndRadius(new Position(44.1994, 17.9066), Distance.FromKilometers(25));
    private Pin? _userPin;
    private bool _initialized;

    private HomeViewModel Vm => BindingContext as HomeViewModel;

    public HomePage()
    {
        InitializeComponent();
        BindingContext = new HomeViewModel();

        _stations = new List<Station>
        {
            new Station("Zenica AS",        "Linije: 1,2,3 | 06:00-22:00", new Position(44.1994, 17.9066)),
            new Station("Gornja Zenica",    "Linije: 1,5",                 new Position(44.2100, 17.9100)),
            new Station("Bilješnica",       "Linije: 2,4",                 new Position(44.2050, 17.9000)),
            new Station("Kakanj Centar",    "Linija: 3",                   new Position(44.1200, 18.1200)),
            new Station("Vareš Centar",     "Linija: 6",                   new Position(44.1800, 18.3400)),
        };

        _stationDetails = new Dictionary<string, StationDetails>(StringComparer.OrdinalIgnoreCase)
        {
            ["Zenica AS"] = new()
            {
                Name = "Zenica AS",
                Address = "Bulevar Kralja Tvrtka I bb",
                Lines = "1, 2, 3",
                WorkingHours = "05:30 - 23:30",
                UpcomingDepartures = new[] { "05:40", "06:10", "06:40" },
                Notes = "Glavna autobuska stanica, peroni 1-5 za gradske linije."
            },
            ["Gornja Zenica"] = new()
            {
                Name = "Gornja Zenica",
                Address = "Ulica Prve zeničke brigade bb",
                Lines = "1, 5",
                WorkingHours = "06:00 - 22:00",
                UpcomingDepartures = new[] { "06:20", "07:05", "07:50" },
                Notes = "Stanica pokriva sjeverni dio grada. Info iz baze još nije dostupna."
            },
            ["Bilješnica"] = new()
            {
                Name = "Bilješnica",
                Address = "Bilješnica bb",
                Lines = "2, 4",
                WorkingHours = "06:00 - 21:30",
                UpcomingDepartures = new[] { "06:35", "07:15", "07:55" },
                Notes = "Privremeni podaci: polasci su informativni."
            },
            ["Kakanj Centar"] = new()
            {
                Name = "Kakanj Centar",
                Address = "Alije Izetbegovića bb, Kakanj",
                Lines = "3",
                WorkingHours = "05:45 - 22:00",
                UpcomingDepartures = new[] { "06:05", "06:50", "07:30" },
                Notes = "Ulaz na peron 2. Detalji će biti ažurirani kada baza bude dostupna."
            },
            ["Vareš Centar"] = new()
            {
                Name = "Vareš Centar",
                Address = "Hikine Livade bb, Vareš",
                Lines = "6",
                WorkingHours = "06:15 - 21:00",
                UpcomingDepartures = new[] { "06:25", "07:10", "07:55" },
                Notes = "Trenutno prikazani podaci su orijentacioni."
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Vm?.LoadFavorites();

        if (_initialized)
            return;

        CenterMap();
        LoadPins(_stations);
        _initialized = true;
    }

    private void LoadPins(IEnumerable<Station> source)
    {
        BusMap.Pins.Clear();

        foreach (var station in source)
        {
            var pin = new Pin
            {
                Label = station.Name,
                Address = station.Address,
                Location = station.Location,
                Type = PinType.Place
            };

            pin.MarkerClicked += OnPinMarkerClicked;
            BusMap.Pins.Add(pin);
        }

        if (_userPin != null)
        {
            BusMap.Pins.Add(_userPin);
        }
    }

    private void CenterMap() =>
        BusMap.MoveToRegion(_zenicaRegion);

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var query = (e.NewTextValue ?? string.Empty).Trim();

        Vm?.FilterFavorites(query);

        if (string.IsNullOrWhiteSpace(query))
        {
            LoadPins(_stations);
            return;
        }

        var filtered = _stations
            .Where(s => s.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || s.Address.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        LoadPins(filtered);
    }

    private void OnCenterMapClicked(object sender, EventArgs e) =>
        CenterMap();

    private void OnCenterZenicaClicked(object sender, EventArgs e) =>
        CenterMap();

    private async void OnMyLocationClicked(object sender, EventArgs e)
    {
        if (!await EnsureLocationPermissionAsync())
        {
            await DisplayAlertAsync("Lokacija nedostupna", "Nije odobrena dozvola za pristup lokaciji.", "OK");
            return;
        }

        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location == null)
            {
                await DisplayAlertAsync("Lokacija nedostupna", "Trenutnu lokaciju nije moguće dobiti.", "OK");
                return;
            }

            _userPin = new Pin
            {
                Label = "Moja lokacija",
                Address = "Trenutna pozicija",
                Location = new Position(location.Latitude, location.Longitude),
                Type = PinType.SavedPin
            };
            _userPin.MarkerClicked += OnPinMarkerClicked;

            LoadPins(_stations);
            BusMap.MoveToRegion(MapSpan.FromCenterAndRadius(_userPin.Location, Distance.FromKilometers(2)));
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlertAsync("Lokacija nedostupna", "GPS nije podržan na ovom uređaju.", "OK");
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Lokacija nedostupna", "Trenutnu lokaciju nije moguće dohvatiti.", "OK");
        }
    }

    private async void OnPinMarkerClicked(object? sender, PinClickedEventArgs e)
    {
        e.HideInfoWindow = true;

        if (sender is Pin pin)
        {
            await ShowStationDetailsAsync(pin.Label, pin.Address ?? string.Empty);
        }
    }

    private async void OnFavoriteClicked(object sender, EventArgs e) =>
        await DisplayAlertAsync("Favoriti", "Dodavanje u favorite još nije implementirano.", "OK");

    private async void OnFavoriteLineClicked(object sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: Linija line })
            return;

        var departures = GetLineDepartures(line.Id);
        var message =
            $"Smjer: {line.Smjer}\n" +
            $"Status: {(line.JeOmiljeno ? "Omiljeno" : "Nije omiljeno")}\n" +
            $"Polasci: {departures}\n\n" +
            "Napomena: Detalji su privremeni dok baza ne bude dostupna.";

        await DisplayAlertAsync($"Linija {line.Naziv}", message, "OK");
    }

    private static async Task<bool> EnsureLocationPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status == PermissionStatus.Granted)
            return true;

        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        return status == PermissionStatus.Granted;
    }

    private Task ShowStationDetailsAsync(string stationName, string fallbackAddress)
    {
        if (!_stationDetails.TryGetValue(stationName, out var details))
        {
            details = new StationDetails
            {
                Name = stationName,
                Address = string.IsNullOrWhiteSpace(fallbackAddress) ? "Adresa nije dostupna" : fallbackAddress,
                Lines = "N/A",
                WorkingHours = "06:00 - 22:00",
                UpcomingDepartures = new[] { "06:30", "07:15", "08:00" },
                Notes = "Detalji trenutno nisu u bazi, prikazani su privremeni podaci."
            };
        }

        var departures = details.UpcomingDepartures?.Length > 0
            ? string.Join(", ", details.UpcomingDepartures)
            : "N/A";

        var message =
            $"{details.Address}\n" +
            $"Linije: {details.Lines}\n" +
            $"Radno vrijeme: {details.WorkingHours}\n" +
            $"Polasci: {departures}\n\n" +
            details.Notes;

        return DisplayAlertAsync(details.Name, message, "OK");
    }

    private string GetLineDepartures(int lineId)
    {
        return lineId switch
        {
            1 => "06:10, 06:45, 07:20",
            2 => "06:20, 07:00, 07:40",
            3 => "06:05, 06:50, 07:30",
            4 => "06:35, 07:15, 07:55",
            5 => "06:25, 07:05, 07:45",
            6 => "06:30, 07:10, 07:50",
            _ => "06:30, 07:15, 08:00"
        };
    }
}

public class Station
{
    public Station(string name, string address, Position location)
    {
        Name = name;
        Address = address;
        Location = location;
    }

    public string Name { get; }
    public string Address { get; }
    public Position Location { get; }
}

public class StationDetails
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Lines { get; set; } = "";
    public string WorkingHours { get; set; } = "";
    public string[] UpcomingDepartures { get; set; } = Array.Empty<string>();
    public string Notes { get; set; } = "";
}
