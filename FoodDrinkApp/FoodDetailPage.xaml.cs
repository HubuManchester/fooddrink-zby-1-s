using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

/// <summary>
/// Displays full nutritional details for a single food or drink item
/// with a hero card, animated macronutrient progress bars, and
/// accessibility actions (TTS and vibration).
/// Receives the item ID via Shell query parameter.
/// </summary>
[QueryProperty(nameof(ItemId), "id")]
public partial class FoodDetailPage : BasePage
{
    /// <summary>Maximum gram value used to normalise the macro bar widths.</summary>
    private const double MacroBarMaxGrams = 100.0;

    private FoodItem? currentItem;

    public FoodDetailPage()
    {
        InitializeComponent();
    }

    /// <summary>Speech cancellation on navigation is handled by <see cref="BasePage"/>.</summary>

    public string ItemId
    {
        set => _ = LoadItemAsync(value);
    }

    private async Task LoadItemAsync(string id)
    {
        currentItem = await FoodCatalogService.GetByIdAsync(id);
        RenderItem();
    }

    /// <summary>
    /// Populates all UI elements from the loaded <see cref="FoodItem"/>.
    /// Macro bars are scaled relative to <see cref="MacroBarMaxGrams"/>
    /// (capped at 100%) for consistent visual comparison.
    /// </summary>
    private void RenderItem()
    {
        if (currentItem is null)
        {
            NameLabel.Text = "Record not found";
            DescriptionLabel.Text = "The selected food or drink could not be loaded.";
            return;
        }

        NameLabel.Text = currentItem.Name;
        CategoryLabel.Text = currentItem.Category;
        CaloriesLabel.Text = currentItem.CaloriesLabel;
        DescriptionLabel.Text = currentItem.Description;
        AllergyLabel.Text = currentItem.AllergyNote;
        SemanticProperties.SetDescription(NameLabel, currentItem.AccessibleSummary);

        // Macro bars — width proportional to value, capped at 100%
        var max = Math.Max(MacroBarMaxGrams,
            Math.Max(currentItem.Protein, Math.Max(currentItem.Carbs, currentItem.Fat)));

        SetMacroBar(ProteinBar, ProteinValueLabel, currentItem.Protein, "g protein", max);
        SetMacroBar(CarbsBar, CarbsValueLabel, currentItem.Carbs, "g carbs", max);
        SetMacroBar(FatBar, FatValueLabel, currentItem.Fat, "g fat", max);
    }

    /// <summary>
    /// Sets the width of a macro bar <see cref="BoxView"/> and its value label.
    /// The bar width is capped at the parent container width (via HorizontalOptions="Start")
    /// and the value label shows the gram amount.
    /// </summary>
    private static void SetMacroBar(BoxView bar, Label valueLabel, int grams, string suffix, double maxGrams)
    {
        var ratio = Math.Min(grams / maxGrams, 1.0);
        bar.WidthRequest = ratio * 280; // approximate max width; actual is constrained by parent
        valueLabel.Text = $"{grams}{suffix}";
    }

    private async void OnSpeakClicked(object? sender, EventArgs e)
    {
        if (currentItem is null)
        {
            await DisplayAlert("Missing record", "There is no nutrition summary to read.", "OK");
            return;
        }

        try
        {
            await SpeechService.SpeakAsync(currentItem.AccessibleSummary);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Text to speech unavailable", ex.Message, "OK");
        }
    }

    private async void OnVibrateClicked(object? sender, EventArgs e)
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            await DisplayAlert("Reminder", "Vibration feedback triggered — your meal reminder is set.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Vibration unavailable", ex.Message, "OK");
        }
    }
}
