using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

/// <summary>
/// Recommendation-style home page with a featured horizontal carousel,
/// category filter chips, searchable food list, and a FAB for adding items.
/// All user-facing text is localised via <see cref="LocalizationService"/>.
/// </summary>
public partial class MainPage : BasePage
{
    private string selectedCategory = string.Empty;
    private readonly List<string> allCategories = ["All"];

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAllAsync(SearchFoodBar.Text);
    }

    private async Task LoadAllAsync(string? query = null)
    {
        var items = await FoodCatalogService.SearchAsync(query);
        var unfiltered = await FoodCatalogService.SearchAsync(null);
        BuildRecommended(unfiltered);
        RebuildCategories(unfiltered);

        if (!string.IsNullOrWhiteSpace(selectedCategory) && selectedCategory != "All")
            items = items.Where(i => i.Category == selectedCategory)
                         .OrderBy(i => i.Name).ToList();

        FoodCollection.ItemsSource = items;
    }

    /// <summary>
    /// Builds a horizontal strip of up to 4 featured items as compact cards.
    /// These act as "recommended" entries and are tappable to open the detail page.
    /// </summary>
    private void BuildRecommended(IReadOnlyList<FoodItem> items)
    {
        RecommendedItems.Children.Clear();
        var featured = items.Take(4).ToList();
        foreach (var item in featured)
        {
            RecommendedItems.Children.Add(CreateRecommendedCard(item));
        }
    }

    private Border CreateRecommendedCard(FoodItem item)
    {
        var card = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#FFFFFF"),
            Padding = new Thickness(12),
            WidthRequest = 150,
            HeightRequest = 110,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 }
        };
        card.Shadow = new Shadow
        {
            Brush = new SolidColorBrush(Color.FromArgb("#18000000")),
            Offset = new Point(0, 2),
            Radius = 8,
            Opacity = 0.10f
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            },
            RowSpacing = 4
        };

        grid.Add(new Label
        {
            Text = item.Name,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#1E1510"),
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 2
        }, 0, 0);

        grid.Add(new Label
        {
            Text = item.CaloriesLabel,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#D9472B")
        }, 0, 1);

        grid.Add(new Label
        {
            Text = item.Category,
            FontSize = 11,
            TextColor = Color.FromArgb("#B87A4A"),
            VerticalOptions = LayoutOptions.End
        }, 0, 2);

        card.Content = grid;

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => _ = Shell.Current.GoToAsync(
            $"{nameof(FoodDetailPage)}?id={Uri.EscapeDataString(item.Id)}");
        card.GestureRecognizers.Add(tap);

        return card;
    }

    private void RebuildCategories(IReadOnlyList<FoodItem> items)
    {
        var distinct = items
            .Select(i => i.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        var categories = new List<string> { LocalizationService.Get("MainAllCategories") };
        categories.AddRange(distinct.Where(c => c != "All" && c != LocalizationService.Get("MainAllCategories")));

        if (categories.SequenceEqual(allCategories)) return;

        allCategories.Clear();
        allCategories.AddRange(categories);
        CategoryChips.Children.Clear();

        foreach (var cat in categories)
            CategoryChips.Children.Add(CreateCategoryChip(cat));
    }

    private Border CreateCategoryChip(string category)
    {
        var isActive = category == selectedCategory ||
                       (category == LocalizationService.Get("MainAllCategories") && string.IsNullOrWhiteSpace(selectedCategory));

        var label = new Label
        {
            Text = category,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = isActive ? Colors.White : Color.FromArgb("#B87A4A"),
            Padding = new Thickness(14, 6)
        };

        var border = new Border
        {
            Content = label,
            StrokeThickness = isActive ? 0 : 1,
            Stroke = Color.FromArgb("#F0D6B8"),
            BackgroundColor = isActive ? Color.FromArgb("#D9472B") : Colors.Transparent,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(0)
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await OnCategoryTapped(category);
        border.GestureRecognizers.Add(tap);
        return border;
    }

    private async Task OnCategoryTapped(string category)
    {
        selectedCategory = category == LocalizationService.Get("MainAllCategories") ? string.Empty : category;

        var unfiltered = await FoodCatalogService.SearchAsync(null);
        RebuildCategories(unfiltered);
        await LoadAllAsync(SearchFoodBar.Text);
    }

    private async void OnFoodCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string id)
            await Shell.Current.GoToAsync(
                $"{nameof(FoodDetailPage)}?id={Uri.EscapeDataString(id)}");
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddItemPage));
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        await LoadAllAsync(e.NewTextValue);
    }

    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        await LoadAllAsync(SearchFoodBar.Text);
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadAllAsync(SearchFoodBar.Text);
        FoodRefreshView.IsRefreshing = false;
        var source = FoodCatalogService.LastLoadUsedMockApi ? "mockapi.io" : "local fallback data";
        SemanticScreenReader.Announce(
            LocalizationService.Get("MainRefreshSource", source));
    }
}
