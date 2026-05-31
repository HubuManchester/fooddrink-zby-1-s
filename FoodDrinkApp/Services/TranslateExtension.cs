using System.Globalization;
using System.Resources;

namespace FoodDrinkApp.Services;

/// <summary>
/// XAML markup extension for concise localized string binding.
/// Usage: <c>Text="{l10n:Translate AppName}"</c>
/// Registers itself as <c>l10n</c> via <see cref="GlobalXmlns"/>.
/// </summary>
[ContentProperty(nameof(Key))]
[AcceptEmptyServiceProvider]
public sealed class TranslateExtension : IMarkupExtension
{
    public string? Key { get; set; }

    public object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (Key is null) return null;

        var loc = LocalizationService.Instance;

        // Subscribe to culture changes so the binding updates when the user switches language.
        var binding = new Binding("[" + Key + "]")
        {
            Mode = BindingMode.OneWay,
            Source = loc,
        };

        return binding;
    }
}
