using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace FoodDrinkApp.Services;

/// <summary>
/// Singleton service for runtime UI localization via RESX resource files.
/// Binds to XAML via <c>{Binding [Key]}</c> on the Instance.
/// Call <see cref="SetLanguage"/> to switch languages at runtime —
/// all bound strings update automatically.
/// </summary>
public sealed class LocalizationService : INotifyPropertyChanged
{
    public static LocalizationService Instance { get; } = new();

    private ResourceManager? resourceManager;
    private CultureInfo currentCulture = CultureInfo.InvariantCulture;

    /// <summary>Fires when the active culture changes, refreshing all bindings.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The currently active culture for string lookups.</summary>
    public CultureInfo CurrentCulture
    {
        get => currentCulture;
        private set
        {
            if (Equals(currentCulture, value)) return;
            currentCulture = value;
            CultureInfo.CurrentUICulture = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }

    /// <summary>Stores the RESX ResourceManager for string resolution.</summary>
    public void SetResourceManager(ResourceManager manager)
    {
        resourceManager = manager;
    }

    /// <summary>
    /// XAML indexer: <c>{Binding [Key]}</c>.
    /// Tries the current culture first, then falls back to the invariant (English) resource.
    /// Returns the key itself if no match is found.
    /// </summary>
    public string this[string key]
    {
        get
        {
            if (resourceManager is null) return key;

            var value = resourceManager.GetString(key, currentCulture)
                        ?? resourceManager.GetString(key, CultureInfo.InvariantCulture);
            return value ?? key;
        }
    }

    /// <summary>
    /// Switches the UI language, navigates to root, and invalidates all
    /// cached tab pages so every ShellContent is recreated with new strings.
    /// </summary>
    public void SetLanguage(string cultureName)
    {
        CurrentCulture = new CultureInfo(cultureName);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Kill all cached pages.
            foreach (var shellItem in Shell.Current.Items)
            foreach (var shellSection in shellItem.Items)
            foreach (var shellContent in shellSection.Items)
                shellContent.Content = null;

            // Defer navigation so Content=null is processed first.
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync("//MainPage");
            });
        });
    }

    /// <summary>True when the current culture matches the given name.</summary>
    public bool IsLanguage(string cultureName)
    {
        return CurrentCulture.Name.StartsWith(cultureName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Convenience static getter for code-behind usage.</summary>
    public static string Get(string key) => Instance[key];

    /// <summary>Convenience static getter with format arguments.</summary>
    public static string Get(string key, params object[] args)
        => string.Format(Instance[key], args);
}
