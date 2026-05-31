using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Expose the localization singleton to all XAML pages via StaticResource.
        Resources["Loc"] = LocalizationService.Instance;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
