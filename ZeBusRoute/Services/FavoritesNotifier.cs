using System;

namespace ZeBusRoute.Services;

public static class FavoritesNotifier
{
    public static event Action? FavoritesChanged;

    public static void NotifyFavoritesChanged() => FavoritesChanged?.Invoke();
}