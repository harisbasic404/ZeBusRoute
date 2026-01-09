namespace ZeBusRoute.Models;

public class Linija
{
    public int Id { get; set; }
    public string Naziv { get; set; } = "";
    public string Smjer { get; set; } = ""; // "AS -> Kraj" ili "Povratak"
}

public class Stanica
{
    public int Id { get; set; }
    public int LinijaId { get; set; }
    public string Naziv { get; set; } = "";
    public double Km { get; set; } // Iz PDF-a za ETA procjene
}

public class Polazak
{
    public int Id { get; set; }
    public int LinijaId { get; set; }
    public TimeOnly Vrijeme { get; set; }
    public string Rezim { get; set; } = ""; // "SD", "CG", "ŠN"
}

public class TicketType
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string PriceDisplay => $"{Price:F2} KM";
}

public class UserTicket
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public DateTime ValidUntil { get; set; }
    public string ValidUntilDisplay => $"Valid until: {ValidUntil:d/M/yyyy}";
    public string TicketId => $"Ticket ID: {Id}";
    public bool IsActive => DateTime.Now < ValidUntil;
    public string StatusDisplay => IsActive ? "Active" : "Expired";
    public Color StatusColor => IsActive ? Color.FromArgb("#8BC34A") : Color.FromArgb("#9E9E9E");
    public string QrCodeData { get; set; } = "";
}
