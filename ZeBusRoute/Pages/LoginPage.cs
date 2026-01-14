// Ensure that the partial class has a corresponding XAML file named LoginPage.xaml
// and that the XAML file's root element includes x:Class="ZeBusRoute.Pages.LoginPage"
// If you do not have a XAML file, you need to define the InitializeComponent method manually or remove the call.

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
#if !NETSTANDARD
        InitializeComponent();
#endif
    }

#if NETSTANDARD
    // If you do not use XAML, you can define InitializeComponent as an empty method to avoid the error.
    private void InitializeComponent()
    {
        // No XAML to initialize
    }
#else
    // Add this empty method to resolve CS0103 if the XAML file is missing or not used.
    private void InitializeComponent()
    {
        // No XAML to initialize
    }
#endif
}