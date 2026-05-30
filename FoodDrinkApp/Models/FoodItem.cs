using System.Text.Json.Serialization;

namespace FoodDrinkApp.Models;

/// <summary>
/// Represents a food or drink record with nutritional information.
/// Used as the primary data model throughout the application for display,
/// search, API serialization, and accessibility narration.
/// </summary>
public sealed class FoodItem
{
    /// <summary>Unique identifier. Auto-generated GUID when not provided by the API.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Display name of the food or drink item.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Meal category such as Breakfast, Lunch, Dinner, Snack, or Drink.
    /// Used for filtering and grouping in the UI.
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>Free-text description including ingredients, flavour notes, or context.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Total energy in kilocalories (kcal). Must be a non-negative integer.</summary>
    [JsonPropertyName("calories")]
    public int Calories { get; set; }

    /// <summary>Protein content in grams. Used in macronutrient summary display.</summary>
    [JsonPropertyName("protein")]
    public int Protein { get; set; }

    /// <summary>Carbohydrate content in grams. Used in macronutrient summary display.</summary>
    [JsonPropertyName("carbs")]
    public int Carbs { get; set; }

    /// <summary>Fat content in grams. Used in macronutrient summary display.</summary>
    [JsonPropertyName("fat")]
    public int Fat { get; set; }

    /// <summary>
    /// Allergy or dietary restriction information.
    /// Defaults to a neutral message when not provided by the user.
    /// </summary>
    [JsonPropertyName("allergyNote")]
    public string AllergyNote { get; set; } = string.Empty;

    /// <summary>
    /// Space-separated search tags (e.g. "healthy breakfast yogurt").
    /// Populated automatically from name, category, and description on save.
    /// </summary>
    [JsonPropertyName("tags")]
    public string Tags { get; set; } = string.Empty;

    /// <summary>Formatted calorie label for card UI display, e.g. "340 kcal".</summary>
    [JsonIgnore]
    public string CaloriesLabel => $"{Calories} kcal";

    /// <summary>Single-line macronutrient summary, e.g. "Protein 24g, carbs 42g, fat 8g".</summary>
    [JsonIgnore]
    public string MacroSummary => $"Protein {Protein}g, carbs {Carbs}g, fat {Fat}g";

    /// <summary>
    /// Full nutrition narration text suitable for screen readers and text-to-speech.
    /// Combines name, category, calories, macros, and allergy information.
    /// </summary>
    [JsonIgnore]
    public string AccessibleSummary => $"{Name}. {Category}. {Calories} kcal. {MacroSummary}. {AllergyNote}";
}
