using Microsoft.Extensions.DependencyInjection;
using ZeBusRoute.Pages;

namespace ZeBusRoute
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new SplashPage());
        }
    }
}