using System.Globalization;
using System.Resources;
using FoodDrinkApp.Services;
using Microsoft.Extensions.Logging;

namespace FoodDrinkApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Set up runtime localization from RESX files.
        // A single ResourceManager resolves strings in any registered culture
        // by consulting the appropriate satellite assembly at runtime.
        var resourceManager = new ResourceManager(
            "FoodDrinkApp.Resources.Localization.AppStrings",
            typeof(MauiProgram).Assembly);

        LocalizationService.Instance.SetResourceManager(resourceManager);
        LocalizationService.Instance.SetLanguage("en");

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
