using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

/// <summary>
/// Form page for creating a new food or drink record.
/// Performs client-side validation on all required fields before saving
/// via <see cref="FoodCatalogService"/>. Displays an inline error panel
/// with vibration feedback when validation fails.
/// </summary>
public partial class AddItemPage : BasePage
{
    /// <summary>Prevents duplicate save requests from rapid button taps.</summary>
    private bool isSaving;

    public AddItemPage()
    {
        InitializeComponent();
    }

    /// <summary>Base accessibility scaling is handled by <see cref="BasePage"/>.</summary>

    /// <summary>
    /// Validates the form, creates a <see cref="FoodItem"/> on success, persists it,
    /// and navigates back to the main list. On validation failure, shows an inline
    /// error panel and triggers a short vibration for tactile feedback.
    /// Uses a guard flag to prevent duplicate saves from rapid taps.
    /// </summary>
    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (isSaving)
        {
            return;
        }

        isSaving = true;
        try
        {
            var validationMessage = ValidateForm(out var calories, out var protein, out var carbs, out var fat);
            if (validationMessage is not null)
            {
                ShowValidation(validationMessage);
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250));
                return;
            }

            var item = new FoodItem
            {
                Name = NameEntry.Text!.Trim(),
                Category = CategoryPicker.SelectedItem?.ToString() ?? "Snack",
                Description = DescriptionEditor.Text!.Trim(),
                Calories = calories,
                Protein = protein,
                Carbs = carbs,
                Fat = fat,
                AllergyNote = string.IsNullOrWhiteSpace(AllergyEntry.Text)
                    ? "No allergy note provided."
                    : AllergyEntry.Text.Trim(),
                Tags = $"{NameEntry.Text} {CategoryPicker.SelectedItem} {DescriptionEditor.Text}"
            };

            await FoodCatalogService.AddAsync(item);
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce("Food record saved.");

            await DisplayAlert(
                "Saved",
                MockApiConfig.IsConfigured
                    ? "The record has been saved to mockapi.io."
                    : "The record has been saved to local fallback data.",
                "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ShowValidation($"Unable to save the record — {ex.Message}");
        }
        finally
        {
            isSaving = false;
        }
    }

    private string? ValidateForm(out int calories, out int protein, out int carbs, out int fat)
    {
        calories = protein = carbs = fat = 0;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            return "Please enter a food or drink name.";
        }

        if (NameEntry.Text.Trim().Length > 60)
        {
            return "Name is too long — please keep it under 60 characters.";
        }

        if (CategoryPicker.SelectedIndex < 0)
        {
            return "Please choose a category.";
        }

        if (string.IsNullOrWhiteSpace(DescriptionEditor.Text))
        {
            return "Please add a short description.";
        }

        return TryReadNumber(CaloriesEntry.Text, "calories", out calories)
            ?? TryReadNumber(ProteinEntry.Text, "protein", out protein)
            ?? TryReadNumber(CarbsEntry.Text, "carbs", out carbs)
            ?? TryReadNumber(FatEntry.Text, "fat", out fat);
    }

    /// <summary>
    /// Attempts to parse a non-negative integer from user input.
    /// Returns an error message string on failure, or null on success.
    /// </summary>
    private static string? TryReadNumber(string? value, string fieldName, out int number)
    {
        if (int.TryParse(value, out number) && number >= 0)
        {
            return null;
        }

        return $"Please enter a valid non-negative number for {fieldName}.";
    }

    private void ShowValidation(string message)
    {
        ValidationLabel.Text = message;
        ValidationPanel.IsVisible = true;
        SemanticScreenReader.Announce(message);
    }
}
