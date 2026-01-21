using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;                    // MapSpan, Distance
using Microsoft.Maui.Devices.Sensors;        // Geolocation, GeolocationRequest
using Position = Microsoft.Maui.Devices.Sensors.Location;

namespace ZeBusRoute.Pages;

public partial class HomePage : ContentPage
{
    private readonly List<Station> _stations;
    private readonly MapSpan _zenicaRegion = MapSpan.FromCenterAndRadius(new Position(44.1994, 17.9066), Distance.FromKilometers(25));
    private Pin? _userPin;
    private bool _initialized;

    public HomePage()
    {
        InitializeComponent();

        _stations = new List<Station>
        {
            new Station("Zenica AS",        "Linije: 1,2,3 | 06:00-22:00", new Position(44.1994, 17.9066)),
            new Station("Gornja Zenica",    "Linije: 1,5",                 new Position(44.2100, 17.9100)),
            new Station("Bilješnica",       "Linije: 2,4",                 new Position(44.2050, 17.9000)),
            new Station("Kakanj Centar",    "Linija: 3",                   new Position(44.1200, 18.1200)),
            new Station("Vareš Centar",     "Linija: 6",                   new Position(44.1800, 18.3400)),
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

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
            await DisplayActionSheetAsync($"{pin.Label}\n{pin.Address}\n\nOdaberi:", "OK", null, "Raspored");
        }
    }

    private async void OnFavoriteClicked(object sender, EventArgs e) =>
        await DisplayAlertAsync("Favoriti", "Dodavanje u favorite još nije implementirano.", "OK");

    private static async Task<bool> EnsureLocationPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status == PermissionStatus.Granted)
            return true;

        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        return status == PermissionStatus.Granted;
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
