using System.Runtime.CompilerServices;

namespace FoodDrinkApp.Services;

/// <summary>
/// Manages the application-wide large-text accessibility feature.
/// When <see cref="LargeTextEnabled"/> is set to true, calls to <see cref="ApplyFontScale"/>
/// increase font sizes on all supported controls (Label, Button, Entry, Editor, Picker, SearchBar)
/// by a factor of <see cref="LargeTextScale"/> (1.22×).
/// Original font sizes are remembered per-control so scaling can be reversed.
/// Pages should call <see cref="ApplyFontScale"/> in their <c>OnAppearing</c> override.
/// </summary>
public static class AccessibilityService
{
    private const double LargeTextScale = 1.22;
    private static readonly ConditionalWeakTable<BindableObject, FontSizeStore> OriginalFontSizes = new();

    /// <summary>
    /// Global toggle controlling whether font scaling is applied across all pages.
    /// Persisted in memory only — resets on app restart.
    /// </summary>
    public static bool LargeTextEnabled { get; set; }

    /// <summary>
    /// Recursively walks the visual tree starting at <paramref name="root"/>,
    /// applying or removing the font scale on every eligible control.
    /// </summary>
    /// <param name="root">The root visual element (typically the current page).</param>
    public static void ApplyFontScale(Element root)
    {
        ApplyToElement(root);

        if (root is not IVisualTreeElement visualTreeElement)
        {
            return;
        }

        foreach (var child in visualTreeElement.GetVisualChildren().OfType<Element>())
        {
            ApplyFontScale(child);
        }
    }

    private static void ApplyToElement(Element element)
    {
        var scale = LargeTextEnabled ? LargeTextScale : 1.0;

        switch (element)
        {
            case Label label:
                label.FontSize = GetOriginalFontSize(label, label.FontSize) * scale;
                break;
            case Button button:
                button.FontSize = GetOriginalFontSize(button, button.FontSize) * scale;
                break;
            case Entry entry:
                entry.FontSize = GetOriginalFontSize(entry, entry.FontSize) * scale;
                break;
            case Editor editor:
                editor.FontSize = GetOriginalFontSize(editor, editor.FontSize) * scale;
                break;
            case Picker picker:
                picker.FontSize = GetOriginalFontSize(picker, picker.FontSize) * scale;
                break;
            case SearchBar searchBar:
                searchBar.FontSize = GetOriginalFontSize(searchBar, searchBar.FontSize) * scale;
                break;
        }
    }

    /// <summary>
    /// Records and returns the original font size for the given control.
    /// On first call the current size is stored; subsequent calls return the stored value.
    /// This prevents cumulative scaling when <see cref="ApplyFontScale"/> is called multiple times.
    /// </summary>
    private static double GetOriginalFontSize(BindableObject control, double currentSize)
    {
        var store = OriginalFontSizes.GetOrCreateValue(control);
        if (!store.HasValue)
        {
            store.Value = currentSize > 0 ? currentSize : 14;
            store.HasValue = true;
        }

        return store.Value;
    }

    private sealed class FontSizeStore
    {
        public bool HasValue { get; set; }
        public double Value { get; set; }
    }
}
