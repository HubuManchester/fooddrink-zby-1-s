using FoodDrinkApp.Services;

namespace FoodDrinkApp;

/// <summary>
/// Base page that automatically applies accessibility font scaling on appearing
/// and cancels any in-progress speech on disappearing.
/// All content pages in the app inherit from this class to avoid duplicating
/// the <see cref="AccessibilityService.ApplyFontScale"/> and
/// <see cref="SpeechService.Stop"/> calls in every page.
/// </summary>
public class BasePage : ContentPage
{
    /// <summary>
    /// Applies the current large-text setting to all controls on this page.
    /// Called automatically when the page becomes visible.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    /// <summary>
    /// Cancels any active text-to-speech when navigating away from the page.
    /// Prevents speech from continuing in the background after the user has
    /// moved to a different screen.
    /// </summary>
    protected override void OnDisappearing()
    {
        SpeechService.Stop();
        base.OnDisappearing();
    }
}
