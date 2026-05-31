using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

/// <summary>
/// Form for creating a new food or drink record with client-side validation.
/// All user-facing messages are localised via <see cref="LocalizationService"/>.
/// </summary>
public partial class AddItemPage : BasePage
{
    private bool isSaving;

    public AddItemPage()
    {
        InitializeComponent();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (isSaving) return;
        isSaving = true;
        try
        {
            var msg = ValidateForm(out var cal, out var prot, out var carbs, out var fat);
            if (msg is not null)
            {
                ShowValidation(msg);
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250));
                return;
            }
            var item = new FoodItem
            {
                Name = NameEntry.Text!.Trim(),
                Category = CategoryPicker.SelectedItem?.ToString() ?? "Snack",
                Description = DescriptionEditor.Text!.Trim(),
                Calories = cal, Protein = prot, Carbs = carbs, Fat = fat,
                AllergyNote = string.IsNullOrWhiteSpace(AllergyEntry.Text)
                    ? "No allergy note provided." : AllergyEntry.Text.Trim(),
                Tags = $"{NameEntry.Text} {CategoryPicker.SelectedItem} {DescriptionEditor.Text}"
            };
            await FoodCatalogService.AddAsync(item);
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce(LocalizationService.Get("AddSavedAnnounce"));
            await DisplayAlert(
                LocalizationService.Get("AddSaveSuccess"),
                MockApiConfig.IsConfigured
                    ? LocalizationService.Get("AddSaveSuccessApi")
                    : LocalizationService.Get("AddSaveSuccessLocal"),
                "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ShowValidation(LocalizationService.Get("ValidationSaveFailed", ex.Message));
        }
        finally { isSaving = false; }
    }

    private string? ValidateForm(out int calories, out int protein, out int carbs, out int fat)
    {
        calories = protein = carbs = fat = 0;
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
            return LocalizationService.Get("ValidationNameRequired");
        if (NameEntry.Text.Trim().Length > 60)
            return LocalizationService.Get("ValidationNameTooLong");
        if (CategoryPicker.SelectedIndex < 0)
            return LocalizationService.Get("ValidationCategory");
        if (string.IsNullOrWhiteSpace(DescriptionEditor.Text))
            return LocalizationService.Get("ValidationDescription");
        return TryReadNumber(CaloriesEntry.Text, "calories", out calories)
            ?? TryReadNumber(ProteinEntry.Text, "protein", out protein)
            ?? TryReadNumber(CarbsEntry.Text, "carbs", out carbs)
            ?? TryReadNumber(FatEntry.Text, "fat", out fat);
    }

    private static string? TryReadNumber(string? value, string field, out int number)
    {
        if (int.TryParse(value, out number) && number >= 0) return null;
        return LocalizationService.Get("ValidationNumber", field);
    }

    private void ShowValidation(string message)
    {
        ValidationLabel.Text = message;
        ValidationPanel.IsVisible = true;
        SemanticScreenReader.Announce(message);
    }
}
