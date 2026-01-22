using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Data.Sqlite;
using Microsoft.Maui.Storage;
using ZeBusRoute.Models;
using ZeBusRoute.Services;

namespace ZeBusRoute.ViewModels;

public interface IAuthService
{
    bool IsLoggedIn();
    string? CurrentUserEmail { get; }
}

public interface ITicketService
{
    Task AddTicketAsync(TicketRecord ticket);
    Task<List<TicketRecord>> GetTicketsAsync(string userEmail);
    Task UpdateTicketAsync(TicketRecord ticket);
}

public class TicketsViewModel : INotifyPropertyChanged
{
    private readonly IAuthService _authService;
    private readonly ITicketService _ticketService;
    private int _selectedTabIndex;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<TicketType> DostupneKarte { get; set; }
    public ObservableCollection<UserTicket> MojeKarte { get; set; }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (_selectedTabIndex != value)
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BuyTicketsTabColor));
                OnPropertyChanged(nameof(MyTicketsTabColor));
                OnPropertyChanged(nameof(IsBuyTicketsTabVisible));
                OnPropertyChanged(nameof(IsMyTicketsTabVisible));
                OnPropertyChanged(nameof(BuyTicketsUnderlineVisible));
                OnPropertyChanged(nameof(MyTicketsUnderlineVisible));

                if (_selectedTabIndex == 1)
                {
                    _ = LoadTicketsAsync();
                }
            }
        }
    }

    public Color BuyTicketsTabColor => SelectedTabIndex == 0 ? Color.FromArgb("#8BC34A") : Color.FromArgb("#9E9E9E");
    public Color MyTicketsTabColor => SelectedTabIndex == 1 ? Color.FromArgb("#8BC34A") : Color.FromArgb("#9E9E9E");

    public bool IsBuyTicketsTabVisible => SelectedTabIndex == 0;
    public bool IsMyTicketsTabVisible => SelectedTabIndex == 1;

    public bool BuyTicketsUnderlineVisible => SelectedTabIndex == 0;
    public bool MyTicketsUnderlineVisible => SelectedTabIndex == 1;

    public ICommand PurchaseTicketCommand { get; }
    public ICommand ChangeTabCommand { get; }
    public ICommand RefreshTicketsCommand { get; }

    public TicketsViewModel()
        : this(new PreferencesAuthService(), new SqliteTicketService())
    {
    }

    public TicketsViewModel(IAuthService authService, ITicketService ticketService)
    {
        _authService = authService;
        _ticketService = ticketService;

        DostupneKarte = new ObservableCollection<TicketType>
        {
            new TicketType { Ime = "Jednokratna karta", Opis = "Vazi 30 minuta", Cijena = 2.00m },
            new TicketType { Ime = "Dnevna karta", Opis = "Beskonacan broj voznji na 12 sati", Cijena = 8.00m },
            new TicketType { Ime = "Mjesecna karta", Opis = "Beskonacan broj voznji na 30 dana", Cijena = 50.00m }
        };

        MojeKarte = new ObservableCollection<UserTicket>();

        PurchaseTicketCommand = new Command<TicketType>(async t => await OnPurchaseTicketAsync(t));
        ChangeTabCommand = new Command<string>(OnChangeTab);
        RefreshTicketsCommand = new Command(async () => await LoadTicketsAsync());

        SelectedTabIndex = 0;
        _ = LoadTicketsAsync();
    }

    private void OnChangeTab(string tabIndex)
    {
        if (int.TryParse(tabIndex, out int index))
        {
            SelectedTabIndex = index;
        }
    }

    private async Task OnPurchaseTicketAsync(TicketType? ticket)
    {
        if (ticket == null) return;

        if (!_authService.IsLoggedIn())
        {
            await ShowAlert("Potrebna prijava", "Moraš biti ulogovan da kupiš kartu. Idi na Profil tab i prijavi se.", "OK");
            return;
        }

        var email = _authService.CurrentUserEmail;
        if (string.IsNullOrWhiteSpace(email))
        {
            await ShowAlert("Greška", "Nije moguće učitati korisnika. Pokušaj ponovo nakon prijave.", "OK");
            return;
        }

        var purchaseDate = DateTime.Now;
        var validUntil = ticket.Ime switch
        {
            "Jednokratna karta" => purchaseDate.AddMinutes(30),
            "Dnevna karta" => purchaseDate.AddHours(12),
            "Mjesecna karta" => purchaseDate.AddDays(30),
            _ => purchaseDate.AddHours(1)
        };

        var record = new TicketRecord
        {
            TicketId = Guid.NewGuid().ToString("N"),
            Type = ticket.Ime,
            PurchaseDate = purchaseDate,
            ValidUntil = validUntil,
            UserEmail = email,
            IsActive = true,
            Price = ticket.Cijena,
            QrCodeData = $"TICKET_{purchaseDate:yyyyMMddHHmmss}_{ticket.Ime.Replace(" ", "").ToUpperInvariant()}"
        };

        await _ticketService.AddTicketAsync(record);
        await LoadTicketsAsync();

        await ShowAlert("Karta kupljena uspješno!", "Karta kupljena uspješno! Sada je aktivna u 'Moje karte'.", "OK");
    }

    private async Task LoadTicketsAsync()
    {
        MojeKarte.Clear();

        if (!_authService.IsLoggedIn())
            return;

        var email = _authService.CurrentUserEmail;
        if (string.IsNullOrWhiteSpace(email))
            return;

        var tickets = await _ticketService.GetTicketsAsync(email);

        foreach (var record in tickets.OrderByDescending(t => t.PurchaseDate))
        {
            // označi kao istekla ako je prošao rok
            if (record.IsActive && record.ValidUntil <= DateTime.Now)
            {
                record.IsActive = false;
                await _ticketService.UpdateTicketAsync(record);
            }

            MojeKarte.Add(new TicketDisplayModel(record));
        }

        if (MojeKarte.Count == 0)
        {
            MojeKarte.Add(TicketDisplayModel.CreatePlaceholder());
        }
    }

    private static async Task ShowAlert(string title, string message, string cancel)
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class TicketDisplayModel : UserTicket
{
    private readonly bool _persistedActive;
    private readonly string _statusOverride;
    private readonly Color? _colorOverride;

    public TicketDisplayModel()
    {
        _statusOverride = "";
        _persistedActive = false;
    }

    public TicketDisplayModel(TicketRecord record)
    {
        Id = record.TicketId;
        Type = record.Type;
        ValidUntil = record.ValidUntil;
        QrCodeData = string.IsNullOrWhiteSpace(record.QrCodeData) ? record.TicketId : record.QrCodeData;
        _persistedActive = record.IsActive;
        _statusOverride = "";
    }

    private TicketDisplayModel(string text, Color color)
    {
        Id = "—";
        Type = text;
        ValidUntil = DateTime.Now;
        QrCodeData = string.Empty;
        _persistedActive = false;
        _statusOverride = text;
        _colorOverride = color;
    }

    public static TicketDisplayModel CreatePlaceholder()
        => new("Nema aktivnih karata. Kupi novu!", Color.FromArgb("#9E9E9E"));

    public new bool IsActive => _persistedActive && DateTime.Now <= ValidUntil;
    public new string StatusDisplay => string.IsNullOrWhiteSpace(_statusOverride)
        ? (IsActive ? "Aktivna" : "Istekla")
        : _statusOverride;

    public new Color StatusColor => _colorOverride ?? (IsActive ? Color.FromArgb("#8BC34A") : Color.FromArgb("#FF5252"));
    public new string ValidUntilDisplay => $"Valid until: {ValidUntil:d/M/yyyy HH:mm}";
    public new string TicketId => string.IsNullOrWhiteSpace(Id) ? "" : $"Ticket ID: {Id}";
}

public class TicketRecord
{
    public string TicketId { get; set; } = "";
    public string Type { get; set; } = "";
    public DateTime PurchaseDate { get; set; }
    public DateTime ValidUntil { get; set; }
    public string UserEmail { get; set; } = "";
    public bool IsActive { get; set; }
    public decimal Price { get; set; }
    public string? QrCodeData { get; set; }
}

public class PreferencesAuthService : IAuthService
{
    public bool IsLoggedIn() => UserAuthService.JePrijavljen();
    public string? CurrentUserEmail => UserAuthService.UcitajProfil()?.Email;
}

public class SqliteTicketService : ITicketService
{
    private readonly string _dbPath = Path.Combine(FileSystem.AppDataDirectory, "zebus.db");
    private const string TableSql = """
        CREATE TABLE IF NOT EXISTS Tickets (
            TicketId TEXT PRIMARY KEY,
            Type TEXT NOT NULL,
            PurchaseDate TEXT NOT NULL,
            ValidUntil TEXT NOT NULL,
            UserEmail TEXT NOT NULL,
            IsActive INTEGER NOT NULL,
            Price REAL,
            QrCodeData TEXT
        );
        """;

    public SqliteTicketService()
    {
        EnsureTable();
    }

    public Task AddTicketAsync(TicketRecord ticket)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT OR REPLACE INTO Tickets (TicketId, Type, PurchaseDate, ValidUntil, UserEmail, IsActive, Price, QrCodeData)
            VALUES (@id, @type, @pdate, @vdate, @email, @active, @price, @qr);
            """;
        cmd.Parameters.AddWithValue("@id", ticket.TicketId);
        cmd.Parameters.AddWithValue("@type", ticket.Type);
        cmd.Parameters.AddWithValue("@pdate", ticket.PurchaseDate.ToString("o"));
        cmd.Parameters.AddWithValue("@vdate", ticket.ValidUntil.ToString("o"));
        cmd.Parameters.AddWithValue("@email", ticket.UserEmail);
        cmd.Parameters.AddWithValue("@active", ticket.IsActive ? 1 : 0);
        cmd.Parameters.AddWithValue("@price", ticket.Price);
        cmd.Parameters.AddWithValue("@qr", ticket.QrCodeData ?? "");
        cmd.ExecuteNonQuery();
        return Task.CompletedTask;
    }

    public Task<List<TicketRecord>> GetTicketsAsync(string userEmail)
    {
        var result = new List<TicketRecord>();
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT TicketId, Type, PurchaseDate, ValidUntil, UserEmail, IsActive, Price, QrCodeData
            FROM Tickets
            WHERE UserEmail = @email
            ORDER BY PurchaseDate DESC;
            """;
        cmd.Parameters.AddWithValue("@email", userEmail);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new TicketRecord
            {
                TicketId = reader.GetString(0),
                Type = reader.GetString(1),
                PurchaseDate = DateTime.Parse(reader.GetString(2)),
                ValidUntil = DateTime.Parse(reader.GetString(3)),
                UserEmail = reader.GetString(4),
                IsActive = reader.GetInt32(5) == 1,
                Price = reader.IsDBNull(6) ? 0m : (decimal)reader.GetDouble(6),
                QrCodeData = reader.IsDBNull(7) ? null : reader.GetString(7)
            });
        }
        return Task.FromResult(result);
    }

    public Task UpdateTicketAsync(TicketRecord ticket)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            UPDATE Tickets
            SET Type = @type,
                PurchaseDate = @pdate,
                ValidUntil = @vdate,
                UserEmail = @email,
                IsActive = @active,
                Price = @price,
                QrCodeData = @qr
            WHERE TicketId = @id;
            """;
        cmd.Parameters.AddWithValue("@type", ticket.Type);
        cmd.Parameters.AddWithValue("@pdate", ticket.PurchaseDate.ToString("o"));
        cmd.Parameters.AddWithValue("@vdate", ticket.ValidUntil.ToString("o"));
        cmd.Parameters.AddWithValue("@email", ticket.UserEmail);
        cmd.Parameters.AddWithValue("@active", ticket.IsActive ? 1 : 0);
        cmd.Parameters.AddWithValue("@price", ticket.Price);
        cmd.Parameters.AddWithValue("@qr", ticket.QrCodeData ?? "");
        cmd.Parameters.AddWithValue("@id", ticket.TicketId);
        cmd.ExecuteNonQuery();
        return Task.CompletedTask;
    }

    private void EnsureTable()
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = TableSql;
        cmd.ExecuteNonQuery();
    }
}