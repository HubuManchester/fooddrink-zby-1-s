using FoodDrinkApp.Services;

namespace FoodDrinkApp;

/// <summary>
/// Primary landing page displaying the searchable food and drink list.
/// Features a horizontal category filter bar, floating search,
/// and a FAB for adding items — inspired by real-world food tracking apps.
/// </summary>
public partial class MainPage : BasePage
{
    /// <summary>Currently selected category filter. An empty string means "All".</summary>
    private string selectedCategory = string.Empty;

    /// <summary>All distinct categories from the loaded data, used to build filter chips.</summary>
    private readonly List<string> allCategories = ["All"];

    public MainPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Refreshes the food list and rebuilds category chips whenever the page becomes visible,
    /// ensuring newly added items and categories appear immediately.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    /// <summary>Loads items via the service and rebuilds both list and category chips.</summary>
    private async Task LoadFoodItemsAsync(string? query = null)
    {
        var items = await FoodCatalogService.SearchAsync(query);

        // Rebuild category list from all items (not filtered)
        var unfiltered = await FoodCatalogService.SearchAsync(null);
        RebuildCategories(unfiltered);

        // Apply category filter on top of search
        if (!string.IsNullOrWhiteSpace(selectedCategory) && selectedCategory != "All")
        {
            items = items
                .Where(i => i.Category == selectedCategory)
                .OrderBy(i => i.Name)
                .ToList();
        }

        FoodCollection.ItemsSource = items;
    }

    /// <summary>
    /// Builds the horizontal category chip list from the available categories.
    /// Chips use a pill/tag style — the active chip is filled; others are outlined.
    /// </summary>
    private void RebuildCategories(IReadOnlyList<Models.FoodItem> items)
    {
        var distinct = items
            .Select(i => i.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        // Always keep "All" at position 0
        var categories = new List<string> { "All" };
        categories.AddRange(distinct.Where(c => c != "All"));

        // Only rebuild if categories actually changed
        if (categories.SequenceEqual(allCategories))
        {
            return;
        }

        allCategories.Clear();
        allCategories.AddRange(categories);

        CategoryChips.Children.Clear();
        foreach (var cat in categories)
        {
            var chip = CreateCategoryChip(cat);
            CategoryChips.Children.Add(chip);
        }
    }

    /// <summary>Creates a single category filter chip as a tappable Border+Label.</summary>
    private Border CreateCategoryChip(string category)
    {
        var isActive = category == selectedCategory ||
                       (category == "All" && string.IsNullOrWhiteSpace(selectedCategory));

        var label = new Label
        {
            Text = category,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = isActive
                ? Colors.White
                : Color.FromArgb("#B87A4A"),
            Padding = new Thickness(14, 6)
        };

        var border = new Border
        {
            Content = label,
            StrokeThickness = isActive ? 0 : 1,
            Stroke = Color.FromArgb("#F0D6B8"),
            BackgroundColor = isActive
                ? Color.FromArgb("#D9472B")
                : Colors.Transparent,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(0)
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => OnCategoryTapped(category);
        border.GestureRecognizers.Add(tap);

        return border;
    }

    /// <summary>Handles category chip taps — sets the filter and refreshes the list.</summary>
    private async void OnCategoryTapped(string category)
    {
        selectedCategory = category == "All" ? string.Empty : category;

        // Refresh chip visuals
        RebuildCategories([.. allCategories.Select(_ => new Models.FoodItem { Category = _ })]);

        // Actually rebuild from real data
        var unfiltered = await FoodCatalogService.SearchAsync(null);
        RebuildCategories(unfiltered);

        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    /// <summary>Card tap opens the detail page (same as the old Details button).</summary>
    private async void OnFoodCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string id)
        {
            await Shell.Current.GoToAsync(
                $"{nameof(FoodDetailPage)}?id={Uri.EscapeDataString(id)}");
        }
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddItemPage));
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
