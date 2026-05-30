namespace FoodDrinkApp.Services;

/// <summary>
/// Wraps the platform Text-to-Speech API for consistent voice output across the app.
/// Automatically cancels any in-progress speech before starting a new utterance,
/// and prefers the English locale for clarity during screencast demonstrations.
/// Callers should invoke <see cref="Stop"/> in page-disappearing handlers to avoid
/// speech continuing after navigation.
/// </summary>
public static class SpeechService
{
    /// <summary>
    /// Token used to cancel the currently-running speech operation.
    /// Disposed and set to null by <see cref="Stop"/>.
    /// </summary>
    private static CancellationTokenSource? currentSpeech;

    /// <summary>
    /// Speaks the given text, first cancelling any in-progress speech.
    /// Uses an English-preferring locale with slightly elevated pitch for naturalness.
    /// If the cancellation token fires, the <see cref="OperationCanceledException"/> is silently swallowed.
    /// </summary>
    public static async Task SpeakAsync(string text)
    {
        Stop();

        currentSpeech = new CancellationTokenSource();
        var options = new SpeechOptions
        {
            Volume = 0.9f,
            Pitch = 1.05f,
            Locale = await FindEnglishLocaleAsync()
        };

        try
        {
            await TextToSpeech.Default.SpeakAsync(text, options, currentSpeech.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>Convenience alias for speaking Chinese text. Delegates to <see cref="SpeakAsync"/>.</summary>
    public static Task SpeakChineseAsync(string text) => SpeakAsync(text);

    /// <summary>
    /// Cancels and disposes the current speech token, if any.
    /// Safe to call when no speech is active — does nothing.
    /// </summary>
    public static void Stop()
    {
        if (currentSpeech is null)
        {
            return;
        }

        currentSpeech.Cancel();
        currentSpeech.Dispose();
        currentSpeech = null;
    }

    /// <summary>
    /// Looks up the first available English locale from the device's TTS engine.
    /// Falls back to the system default if no English locale is installed.
    /// </summary>
    private static async Task<Locale?> FindEnglishLocaleAsync()
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();
        return locales.FirstOrDefault(locale => locale.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase));
    }
}
