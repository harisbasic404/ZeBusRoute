using Microsoft.Data.Sqlite;
using ZeBusRoute.Models;
using System.Collections.ObjectModel;

namespace ZeBusRoute.Services;

public static class DataService
{
    private static string DbPath => Path.Combine(FileSystem.AppDataDirectory, "zebus.db");

    public static void InitDb()
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        conn.Open();

        // Kreiraj tabele
        var createCmd = conn.CreateCommand();
        createCmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Linije (
                Id INTEGER PRIMARY KEY,
                Naziv TEXT NOT NULL,
                Smjer TEXT
            );
            CREATE TABLE IF NOT EXISTS Stanice (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                LinijaId INTEGER,
                Naziv TEXT NOT NULL,
                Km REAL,
                FOREIGN KEY(LinijaId) REFERENCES Linije(Id)
            );
            CREATE TABLE IF NOT EXISTS Polasci (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                LinijaId INTEGER,
                Vrijeme TEXT NOT NULL,
                Rezim TEXT DEFAULT 'SD',
                FOREIGN KEY(LinijaId) REFERENCES Linije(Id)
            );
            """;
        createCmd.ExecuteNonQuery();

        // Provjeri da li je baza popunjena
        using var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Linije";
        if (checkCmd.ExecuteScalar() is long count && count == 0)
        {
            SeedData(conn);
        }
        
        // Inicijalizuj tabelu omiljenih
        InicijalizujTabeluOmiljenih();
    }

    private static void SeedData(SqliteConnection conn)
    {
        // 1. Zenica "AS" - Gornja Zenica (gradska)
        InsertLinija(conn, 1, "Zenica \"AS\" - Gornja Zenica", "AS -> Gornja");
        var stanice1 = new[] { ("Zenica AS", 0.0), ("Stadion", 1.2), ("Općina", 1.8), ("Carina", 2.5), ("Lovacki dom", 3.0), ("Mogućnice", 3.5), ("Prodavnica", 4.0), ("Turbe", 4.5), ("Luke", 5.0), ("Voljevac", 5.5), ("Gornja Zenica", 7.1) };
        foreach (var (naziv, km) in stanice1) InsertStanica(conn, naziv, 1, km);
        var polasci1 = new[] { "06:30", "07:15", "08:00", "09:00", "10:30", "12:00", "14:00", "16:00", "17:30", "19:00" };
        foreach (var vrijeme in polasci1) InsertPolazak(conn, 1, vrijeme);

        // 19. Zenica "AS" - Vjetrenice (gradska)
        InsertLinija(conn, 19, "Zenica \"AS\" - Vjetrenice", "AS -> Vjetrenice");
        var stanice19 = new[] { ("Zenica AS", 0.0), ("Stadion", 1.0), ("Općina", 1.5), ("Carina", 2.2), ("Vjetrenice", 4.5) };
        foreach (var (naziv, km) in stanice19) InsertStanica(conn, naziv, 19, km);
        var polasci19 = new[] { "06:00", "07:30", "09:00", "11:00", "13:00", "15:00", "17:00", "19:00" };
        foreach (var vrijeme in polasci19) InsertPolazak(conn, 19, vrijeme);

        // 20. Zenica "AS" - Mošćanica (gradska)
        InsertLinija(conn, 20, "Zenica \"AS\" - Mošćanica", "AS -> Mošćanica");
        var stanice20 = new[] { ("Zenica AS", 0.0), ("Bolnica", 1.5), ("Mošćanica", 3.8) };
        foreach (var (naziv, km) in stanice20) InsertStanica(conn, naziv, 20, km);
        var polasci20 = new[] { "05:45", "07:00", "08:30", "10:00", "12:30", "14:30", "16:30", "18:30" };
        foreach (var vrijeme in polasci20) InsertPolazak(conn, 20, vrijeme);

        // 21. Zenica "AS" - Gorica (gradska)
        InsertLinija(conn, 21, "Zenica \"AS\" - Gorica", "AS -> Gorica");
        var stanice21 = new[] { ("Zenica AS", 0.0), ("Gorica", 2.8) };
        foreach (var (naziv, km) in stanice21) InsertStanica(conn, naziv, 21, km);
        var polasci21 = new[] { "06:15", "07:45", "09:30", "11:30", "13:30", "15:45", "17:45" };
        foreach (var vrijeme in polasci21) InsertPolazak(conn, 21, vrijeme);

        // 22. Zenica "AS" – Bolnica - Tišina (gradska)
        InsertLinija(conn, 22, "Zenica \"AS\" – Bolnica - Tišina", "AS -> Tišina");
        var stanice22 = new[] { ("Zenica AS", 0.0), ("Bolnica", 1.0), ("Tišina", 2.5) };
        foreach (var (naziv, km) in stanice22) InsertStanica(conn, naziv, 22, km);
        var polasci22 = new[] { "06:20", "08:00", "10:00", "12:00", "14:00", "16:00", "18:00", "20:00" };
        foreach (var vrijeme in polasci22) InsertPolazak(conn, 22, vrijeme);

        // 33. Zenica “AS” - Klopče (gradska)
        InsertLinija(conn, 33, "Zenica \"AS\" - Klopče", "AS -> Klopče");
        var stanice33 = new[] { ("Zenica AS", 0.0), ("Klopče", 4.2) };
        foreach (var (naziv, km) in stanice33) InsertStanica(conn, naziv, 33, km);
        var polasci33 = new[] { "07:00", "09:00", "11:00", "13:00", "15:00", "17:00" };
        foreach (var vrijeme in polasci33) InsertPolazak(conn, 33, vrijeme);

        // 2. Zenica “AS” – Varošište - Nemila (Naselje) - Kahrimani (prigradska)
        InsertLinija(conn, 2, "Zenica “AS” – Varošište - Nemila (Naselje) - Kahrimani", "AS -> Kahrimani");
        var stanice2 = new[] { ("Zenica AS", 0.0), ("Varošište", 2.0), ("Nemila Naselje", 8.5), ("Kahrimani", 12.0) };
        foreach (var (naziv, km) in stanice2) InsertStanica(conn, naziv, 2, km);
        var polasci2 = new[] { "05:30", "06:45", "08:15", "10:15", "12:45", "15:00", "17:15", "19:30" };
        foreach (var vrijeme in polasci2) InsertPolazak(conn, 2, vrijeme);

        // 3. Zenica “AS” – Koprivna - Nemila (Naselje) (prigradska)
        InsertLinija(conn, 3, "Zenica “AS” – Koprivna - Nemila (Naselje)", "AS -> Nemila");
        var stanice3 = new[] { ("Zenica AS", 0.0), ("Koprivna", 6.0), ("Nemila Naselje", 10.5) };
        foreach (var (naziv, km) in stanice3) InsertStanica(conn, naziv, 3, km);
        var polasci3 = new[] { "06:00", "07:30", "09:30", "11:45", "14:00", "16:15", "18:30" };
        foreach (var vrijeme in polasci3) InsertPolazak(conn, 3, vrijeme);

        // 6. Zenica „AS“ – Nemila - Jastrebac – Vukotići - Šerići (prigradska)
        InsertLinija(conn, 6, "Zenica „AS“ – Nemila - Jastrebac – Vukotići - Šerići", "AS -> Šerići");
        var stanice6 = new[] { ("Zenica AS", 0.0), ("Nemila", 7.0), ("Jastrebac", 9.5), ("Vukotići", 12.0), ("Šerići", 15.2) };
        foreach (var (naziv, km) in stanice6) InsertStanica(conn, naziv, 6, km);
        var polasci6 = new[] { "05:45", "07:00", "09:00", "11:30", "14:00", "16:30", "19:00" };
        foreach (var vrijeme in polasci6) InsertPolazak(conn, 6, vrijeme);

        // 7. Zenica “AS” – Orahovica – Babići (škola) (prigradska)
        InsertLinija(conn, 7, "Zenica “AS” – Orahovica – Babići (škola)", "AS -> Babići");
        var stanice7 = new[] { ("Zenica AS", 0.0), ("Orahovica", 5.5), ("Babići Škola", 9.0) };
        foreach (var (naziv, km) in stanice7) InsertStanica(conn, naziv, 7, km);
        var polasci7 = new[] { "06:30", "08:00", "10:30", "13:00", "15:30", "17:30" };
        foreach (var vrijeme in polasci7) InsertPolazak(conn, 7, vrijeme);

        // 35. Zenica “AS" - Banlozi (prigradska)
        InsertLinija(conn, 35, "Zenica “AS\" - Banlozi", "AS -> Banlozi");
        var stanice35 = new[] { ("Zenica AS", 0.0), ("Banlozi", 11.0) };
        foreach (var (naziv, km) in stanice35) InsertStanica(conn, naziv, 35, km);
        var polasci35 = new[] { "06:15", "07:45", "09:45", "12:00", "14:30", "16:45", "19:15" };
        foreach (var vrijeme in polasci35) InsertPolazak(conn, 35, vrijeme);

        // 36. Zenica “AS” – Banlozi – Vranduk (Stari) (prigradska)
        InsertLinija(conn, 36, "Zenica “AS” – Banlozi – Vranduk (Stari)", "AS -> Vranduk");
        var stanice36 = new[] { ("Zenica AS", 0.0), ("Banlozi", 11.0), ("Vranduk Stari", 14.6) };
        foreach (var (naziv, km) in stanice36) InsertStanica(conn, naziv, 36, km);
        var polasci36 = new[] { "05:50", "07:20", "09:20", "11:50", "14:20", "16:50", "19:20" };
        foreach (var vrijeme in polasci36) InsertPolazak(conn, 36, vrijeme);
    }

    private static void InsertLinija(SqliteConnection conn, int id, string naziv, string smjer)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT OR IGNORE INTO Linije (Id, Naziv, Smjer) VALUES (@id, @naziv, @smjer)";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@naziv", naziv);
        cmd.Parameters.AddWithValue("@smjer", smjer);
        cmd.ExecuteNonQuery();
    }

    private static void InsertStanica(SqliteConnection conn, string naziv, int linijaId, double km)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Stanice (LinijaId, Naziv, Km) VALUES (@linijaId, @naziv, @km)";
        cmd.Parameters.AddWithValue("@linijaId", linijaId);
        cmd.Parameters.AddWithValue("@naziv", naziv);
        cmd.Parameters.AddWithValue("@km", km);
        cmd.ExecuteNonQuery();
    }

    private static void InsertPolazak(SqliteConnection conn, int linijaId, string vrijeme)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Polasci (LinijaId, Vrijeme) VALUES (@linijaId, @vrijeme)";
        cmd.Parameters.AddWithValue("@linijaId", linijaId);
        cmd.Parameters.AddWithValue("@vrijeme", vrijeme);
        cmd.ExecuteNonQuery();
    }

    public static List<Linija> GetLinije()
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Naziv, Smjer FROM Linije ORDER BY Id";
        using var reader = cmd.ExecuteReader();
        var linije = new List<Linija>();
        while (reader.Read())
        {
            linije.Add(new Linija { Id = reader.GetInt32(0), Naziv = reader.GetString(1), Smjer = reader.IsDBNull(2) ? "" : reader.GetString(2) });
        }
        return linije;
    }

    public static List<Stanica> GetStanice(int linijaId)
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Naziv, Km FROM Stanice WHERE LinijaId = @id ORDER BY Km";
        cmd.Parameters.AddWithValue("@id", linijaId);
        using var reader = cmd.ExecuteReader();
        var stanice = new List<Stanica>();
        while (reader.Read())
        {
            stanice.Add(new Stanica { Id = reader.GetInt32(0), Naziv = reader.GetString(1), Km = reader.GetDouble(2) });
        }
        return stanice;
    }

    public static List<Polazak> GetPolasci(int linijaId)
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Vrijeme, Rezim FROM Polasci WHERE LinijaId = @id ORDER BY Vrijeme";
        cmd.Parameters.AddWithValue("@id", linijaId);
        using var reader = cmd.ExecuteReader();
        var polasci = new List<Polazak>();
        while (reader.Read())
        {
            polasci.Add(new Polazak { Vrijeme = TimeOnly.Parse(reader.GetString(0)), Rezim = reader.IsDBNull(1) ? "SD" : reader.GetString(1) });
        }
        return polasci;
    }

    public static void InicijalizujTabeluOmiljenih()
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        conn.Open();
        
        var createCmd = conn.CreateCommand();
        createCmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Omiljeni (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Email TEXT NOT NULL,
                LinijaId INTEGER NOT NULL,
                DatumDodavanja TEXT NOT NULL,
                UNIQUE(Email, LinijaId),
                FOREIGN KEY(LinijaId) REFERENCES Linije(Id)
            );
            """;
        createCmd.ExecuteNonQuery();
    }

    public static bool JeLinijaOmiljena(string email, int linijaId)
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Omiljeni WHERE Email = @email AND LinijaId = @linijaId";
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@linijaId", linijaId);
        
        var result = cmd.ExecuteScalar();
        var brojac = result is not null ? (long)result : 0;
        return brojac > 0;
    }

    public static void DodajOmiljeno(string email, int linijaId)
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT OR IGNORE INTO Omiljeni (Email, LinijaId, DatumDodavanja) VALUES (@email, @linijaId, @datum)";
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@linijaId", linijaId);
        cmd.Parameters.AddWithValue("@datum", DateTime.Now.ToString("o"));
        cmd.ExecuteNonQuery();
    }

    public static void UkloniOmiljeno(string email, int linijaId)
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Omiljeni WHERE Email = @email AND LinijaId = @linijaId";
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@linijaId", linijaId);
        cmd.ExecuteNonQuery();
    }

    public static List<Linija> DohvatiOmiljeneLinije(string email)
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT L.Id, L.Naziv, L.Smjer 
            FROM Linije L
            INNER JOIN Omiljeni O ON L.Id = O.LinijaId
            WHERE O.Email = @email
            ORDER BY O.DatumDodavanja DESC
            """;
        cmd.Parameters.AddWithValue("@email", email);
        
        using var reader = cmd.ExecuteReader();
        var linije = new List<Linija>();
        while (reader.Read())
        {
            linije.Add(new Linija 
            { 
                Id = reader.GetInt32(0), 
                Naziv = reader.GetString(1), 
                Smjer = reader.IsDBNull(2) ? "" : reader.GetString(2) 
            });
        }
        return linije;
    }
}
