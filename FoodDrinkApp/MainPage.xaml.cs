using FoodDrinkApp.Services;

namespace FoodDrinkApp;

/// <summary>
/// Primary landing page displaying the searchable food and drink list.
/// Binds a <see cref="CollectionView"/> to data from <see cref="FoodCatalogService"/>
/// and provides navigation to the detail and add-item pages via Shell routing.
/// </summary>
public partial class MainPage : BasePage
{
    public MainPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Refreshes the food list whenever the page becomes visible, ensuring
    /// newly added items appear immediately after navigating back.
    /// Base font scaling is handled by <see cref="BasePage"/>.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    private async Task LoadFoodItemsAsync(string? query = null)
    {
        FoodCollection.ItemsSource = await FoodCatalogService.SearchAsync(query);
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddItemPage));
    }

    private async void OnDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            await Shell.Current.GoToAsync($"{nameof(FoodDetailPage)}?id={Uri.EscapeDataString(id)}");
        }
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        await LoadFoodItemsAsync(e.NewTextValue);
    }

    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadFoodItemsAsync(SearchFoodBar.Text);
        FoodRefreshView.IsRefreshing = false;
        var source = FoodCatalogService.LastLoadUsedMockApi ? "mockapi.io" : "local fallback data";
        SemanticScreenReader.Announce($"Food and drink list refreshed. Current source: {source}.");
    }
}
