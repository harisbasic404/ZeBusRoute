using ZeBusRoute.ViewModels;

namespace ZeBusRoute.Pages;

public partial class TicketsPage : ContentPage
{
    public TicketsPage()
    {
        InitializeComponent();
    }

    private void OnBuyTicketsTabClicked(object sender, EventArgs e)
    {
        if (BindingContext is TicketsViewModel vm)
        {
            vm.SelectedTabIndex = 0;
        }
    }

    private void OnMyTicketsTabClicked(object sender, EventArgs e)
    {
        if (BindingContext is TicketsViewModel vm)
        {
            vm.SelectedTabIndex = 1;
        }
    }
}