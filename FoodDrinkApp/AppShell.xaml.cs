using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(AddItemPage), typeof(AddItemPage));
        Routing.RegisterRoute(nameof(FoodDetailPage), typeof(FoodDetailPage));

        UpdateTabs();
        LocalizationService.Instance.PropertyChanged += (_, _) => UpdateTabs();
    }

    private void UpdateTabs()
    {
        var items = MainTabs.Items;
        if (items.Count >= 1) items[0].Title = LocalizationService.Get("TabFoods");
        if (items.Count >= 2) items[1].Title = LocalizationService.Get("TabDevices");
        if (items.Count >= 3) items[2].Title = LocalizationService.Get("TabSettings");
    }
}
