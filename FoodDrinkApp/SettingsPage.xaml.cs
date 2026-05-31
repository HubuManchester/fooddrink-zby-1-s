using FoodDrinkApp.Services;

namespace FoodDrinkApp;

/// <summary>
/// Settings page for theme, large-text, and language preferences.
/// Language switching is applied immediately via <see cref="LocalizationService"/>
/// and refreshes all localized bindings across the app.
/// </summary>
public partial class SettingsPage : BasePage
{
    public SettingsPage()
    {
        InitializeComponent();
        PopulateThemePicker();
        ThemePicker.SelectedIndex = 0;
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
        UpdateLanguageButtons();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
        ApplyLargeTextState();
        PopulateThemePicker();
        UpdateLanguageButtons();
    }

    /// <summary>Fills the theme picker with localized options (rebuilt on language change).</summary>
    private void PopulateThemePicker()
    {
        var selected = ThemePicker.SelectedIndex;
        ThemePicker.Items.Clear();
        ThemePicker.Items.Add(LocalizationService.Get("SettingsThemeSystem"));
        ThemePicker.Items.Add(LocalizationService.Get("SettingsThemeLight"));
        ThemePicker.Items.Add(LocalizationService.Get("SettingsThemeDark"));
        ThemePicker.SelectedIndex = selected >= 0 ? selected : 0;
    }

    /// <summary>Highlights the active language button.</summary>
    private void UpdateLanguageButtons()
    {
        var isEnglish = LocalizationService.Instance.IsLanguage("en");
        EnglishButton.BackgroundColor = isEnglish
            ? Color.FromArgb("#D9472B")
            : Color.FromArgb("#F5E8DC");
        EnglishButton.TextColor = isEnglish ? Colors.White : Color.FromArgb("#B87A4A");
        ChineseButton.BackgroundColor = !isEnglish
            ? Color.FromArgb("#D9472B")
            : Color.FromArgb("#F5E8DC");
        ChineseButton.TextColor = !isEnglish ? Colors.White : Color.FromArgb("#B87A4A");
    }

    private void OnEnglishClicked(object? sender, EventArgs e)
    {
        LocalizationService.Instance.SetLanguage("en");
        UpdateLanguageButtons();
        SemanticScreenReader.Announce("Language changed to English.");
    }

    private void OnChineseClicked(object? sender, EventArgs e)
    {
        LocalizationService.Instance.SetLanguage("zh-Hans");
        UpdateLanguageButtons();
        SemanticScreenReader.Announce("语言已切换为中文。");
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        Application.Current!.UserAppTheme = ThemePicker.SelectedIndex switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
        Announce(LocalizationService.Get("SettingsThemeAnnounce"));
    }

    private void OnLargeTextToggled(object? sender, ToggledEventArgs e)
    {
        AccessibilityService.LargeTextEnabled = e.Value;
        ApplyLargeTextState();
        Announce(e.Value
            ? LocalizationService.Get("SettingsLargeTextOnAnnounce")
            : LocalizationService.Get("SettingsLargeTextOffAnnounce"));
    }

    private void ApplyLargeTextState()
    {
        AccessibilityService.ApplyFontScale(this);
        LargeTextPreviewTitle.Text = AccessibilityService.LargeTextEnabled
            ? LocalizationService.Get("SettingsLargeTextOn")
            : LocalizationService.Get("SettingsLargeTextPreview");
        LargeTextPreviewBody.Text = AccessibilityService.LargeTextEnabled
            ? LocalizationService.Get("SettingsLargeTextOnDesc")
            : LocalizationService.Get("SettingsLargeTextOffDesc");
    }

    private void Announce(string message)
    {
        SettingsStatusLabel.Text = message;
        SemanticScreenReader.Announce(message);
    }
}
