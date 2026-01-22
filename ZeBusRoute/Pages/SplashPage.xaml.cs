using ZeBusRoute.Services;

namespace ZeBusRoute.Pages;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var animacijaZadatak = AnimirajSplashScreen();
            var inicijalizacijaZadatak = InicijalizujAplikaciju();

            await Task.WhenAll(animacijaZadatak, inicijalizacijaZadatak);

            await Task.Delay(500);

            Application.Current!.MainPage = new AppShell();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška",
                $"Greška pri pokretanju aplikacije: {ex.Message}", 
                "U redu");
        }
    }

    private async Task AnimirajSplashScreen()
    {
        await Task.WhenAll(
            SlikaLogo.FadeToAsync(1, 1000, Easing.CubicOut),
            SlikaLogo.ScaleToAsync(1, 1000, Easing.SpringOut)
        );

        await NaslovLabela.FadeToAsync(1, 600, Easing.CubicOut);

        await PodnaslovLabela.FadeToAsync(1, 600, Easing.CubicOut);
    }

    private async Task InicijalizujAplikaciju()
    {
        await Task.Run(() =>
        {
            DataService.InitDb();
        });

        await Task.Delay(2000);
    }
}