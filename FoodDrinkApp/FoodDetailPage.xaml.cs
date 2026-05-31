using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

/// <summary>
/// Full nutritional detail view with hero card, macronutrient progress bars,
/// and TTS / vibration action buttons. All strings are localised.
/// </summary>
[QueryProperty(nameof(ItemId), "id")]
public partial class FoodDetailPage : BasePage
{
    private const double MacroBarMaxGrams = 100.0;
    private FoodItem? currentItem;

    public FoodDetailPage()
    {
        InitializeComponent();
    }

    public string ItemId { set => _ = LoadItemAsync(value); }

    private async Task LoadItemAsync(string id)
    {
        currentItem = await FoodCatalogService.GetByIdAsync(id);
        RenderItem();
    }

    private void RenderItem()
    {
        if (currentItem is null)
        {
            NameLabel.Text = LocalizationService.Get("DetailNotFound");
            DescriptionLabel.Text = LocalizationService.Get("DetailNotFoundDesc");
            return;
        }
        NameLabel.Text = currentItem.Name;
        CategoryLabel.Text = currentItem.Category;
        CaloriesLabel.Text = currentItem.CaloriesLabel;
        DescriptionLabel.Text = currentItem.Description;
        AllergyLabel.Text = currentItem.AllergyNote;
        SemanticProperties.SetDescription(NameLabel, currentItem.AccessibleSummary);

        var max = Math.Max(MacroBarMaxGrams,
            Math.Max(currentItem.Protein, Math.Max(currentItem.Carbs, currentItem.Fat)));
        SetMacroBar(ProteinBar, ProteinValueLabel, currentItem.Protein, "g", max);
        SetMacroBar(CarbsBar, CarbsValueLabel, currentItem.Carbs, "g", max);
        SetMacroBar(FatBar, FatValueLabel, currentItem.Fat, "g", max);
    }

    private static void SetMacroBar(BoxView bar, Label label, int grams, string unit, double maxGrams)
    {
        bar.WidthRequest = Math.Min(grams / maxGrams, 1.0) * 280;
        label.Text = $"{grams}{unit}";
    }

    private async void OnSpeakClicked(object? sender, EventArgs e)
    {
        if (currentItem is null)
        {
            await DisplayAlert(
                LocalizationService.Get("DetailMissingRecord"),
                LocalizationService.Get("DetailNoSummary"), "OK");
            return;
        }
        try
        {
            await SpeechService.SpeakAsync(currentItem.AccessibleSummary);
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                LocalizationService.Get("DetailTtsUnavailable"), ex.Message, "OK");
        }
    }

    private async void OnVibrateClicked(object? sender, EventArgs e)
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            await DisplayAlert(
                LocalizationService.Get("DetailReminderTitle"),
                LocalizationService.Get("DetailReminderMsg"), "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                LocalizationService.Get("DetailVibrationUnavailable"), ex.Message, "OK");
        }
    }
}
