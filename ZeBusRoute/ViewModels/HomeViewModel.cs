using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using ZeBusRoute.Models;
using ZeBusRoute.Services;

namespace ZeBusRoute.ViewModels;

public class HomeViewModel : INotifyPropertyChanged
{
    private string _trenutniEmail = string.Empty;
    private bool _hasFavorites;

    public ObservableCollection<Linija> FavoriteLines { get; } = new();
    public ObservableCollection<Linija> FilteredFavoriteLines { get; } = new();

    public bool HasFavorites
    {
        get => _hasFavorites;
        private set
        {
            if (_hasFavorites != value)
            {
                _hasFavorites = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNoFavorites));
            }
        }
    }

    public bool HasNoFavorites => !HasFavorites;

    public event PropertyChangedEventHandler? PropertyChanged;

    public HomeViewModel()
    {
        RefreshEmail();
        FavoritesNotifier.FavoritesChanged += LoadFavorites;
    }

    private void RefreshEmail()
    {
        var profil = UserAuthService.UcitajProfil();
        var email = profil?.Email;

        if (string.IsNullOrWhiteSpace(email))
            email = Preferences.Get("auth_email", string.Empty);
        if (string.IsNullOrWhiteSpace(email))
            email = Preferences.Get("profil_email", string.Empty);

        _trenutniEmail = email ?? string.Empty;
    }

    public void LoadFavorites()
    {
        RefreshEmail();

        FavoriteLines.Clear();
        FilteredFavoriteLines.Clear();

        if (string.IsNullOrWhiteSpace(_trenutniEmail) || !UserAuthService.JePrijavljen())
        {
            HasFavorites = false;
            return;
        }

        try
        {
            DataService.InitDb();
            var omiljene = DataService.DohvatiOmiljeneLinije(_trenutniEmail);

            foreach (var linija in omiljene)
            {
                linija.JeOmiljeno = true;
                FavoriteLines.Add(linija);
                FilteredFavoriteLines.Add(linija);
            }

            HasFavorites = FavoriteLines.Count > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Greška pri učitavanju omiljenih: {ex.Message}");
            HasFavorites = false;
        }
    }

    public void FilterFavorites(string query)
    {
        if (FavoriteLines.Count == 0)
            return;

        FilteredFavoriteLines.Clear();

        var normalized = (query ?? string.Empty).Trim();
        var results = string.IsNullOrWhiteSpace(normalized)
            ? FavoriteLines
            : FavoriteLines.Where(l =>
                l.Naziv.Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
                l.Smjer.Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
                l.Id.ToString().Contains(normalized, StringComparison.OrdinalIgnoreCase));

        foreach (var linija in results)
        {
            FilteredFavoriteLines.Add(linija);
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}