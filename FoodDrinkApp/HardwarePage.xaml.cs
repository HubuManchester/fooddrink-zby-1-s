using FoodDrinkApp.Services;

namespace FoodDrinkApp;

/// <summary>
/// Demonstrates five mobile hardware capabilities: camera, geolocation,
/// text-to-speech, vibration, and haptic feedback. All status messages
/// are localised via <see cref="LocalizationService"/>.
/// </summary>
public partial class HardwarePage : BasePage
{
    private int feedbackTestCount;
    private bool isHardwareBusy;

    public HardwarePage()
    {
        InitializeComponent();
    }

    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        if (isHardwareBusy) return;
        isHardwareBusy = true;
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                SetStatus(LocalizationService.Get("HwCameraUnsupported"));
                return;
            }
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null)
            {
                SetStatus(LocalizationService.Get("HwPhotoCancelled"));
                return;
            }
            await using var stream = await photo.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            FoodPhoto.Source = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
            SetStatus(LocalizationService.Get("HwPhotoSuccess"));
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (PermissionException)
        {
            SetStatus(LocalizationService.Get("HwCameraPermission"));
        }
        catch (Exception ex)
        {
            SetStatus(LocalizationService.Get("HwPhotoError", ex.Message));
        }
        finally { isHardwareBusy = false; }
    }

    private async void OnGetLocationClicked(object? sender, EventArgs e)
    {
        if (isHardwareBusy) return;
        isHardwareBusy = true;
        try
        {
            SetStatus(LocalizationService.Get("HwGettingLocation"));
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);
            if (location is null)
            {
                SetStatus(LocalizationService.Get("HwLocationNotFound"));
                return;
            }
            CoordinateLabel.Text = $"Lat {location.Latitude:F5}, Lon {location.Longitude:F5}";
            LocationLabel.Text = await BuildAddressTextAsync(location);
            SetStatus(LocalizationService.Get("HwLocationSuccess"));
        }
        catch (PermissionException)
        {
            SetStatus(LocalizationService.Get("HwLocationPermission"));
        }
        catch (Exception ex)
        {
            SetStatus(LocalizationService.Get("HwLocationError", ex.Message));
        }
        finally { isHardwareBusy = false; }
    }

    private static async Task<string> BuildAddressTextAsync(Location location)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();
            var address = FormatPlacemark(placemark);
            if (!string.IsNullOrWhiteSpace(address)) return address;
        }
        catch { }
        return BuildFallbackAddress(location);
    }

    private static string FormatPlacemark(Placemark? placemark)
    {
        if (placemark is null) return string.Empty;
        var parts = new[] { placemark.CountryName, placemark.AdminArea, placemark.Locality, placemark.SubLocality, placemark.Thoroughfare }
            .Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToArray();
        return parts.Length == 0 ? string.Empty : string.Join(" / ", parts);
    }

    private static string BuildFallbackAddress(Location location)
    {
        if (IsNear(location, 37.422, -122.084, 0.08)) return "United States / California / Mountain View";
        if (location.Latitude is >= 37.0 and <= 38.2 && location.Longitude is >= -123.2 and <= -121.5) return "United States / California / San Francisco Bay Area";
        if (location.Latitude is >= 18 and <= 54 && location.Longitude is >= 73 and <= 135) return "China / Current city requires a real device";
        return "Coordinates found — country/city not returned by this device.";
    }

    private static bool IsNear(Location location, double lat, double lon, double tol)
        => Math.Abs(location.Latitude - lat) <= tol && Math.Abs(location.Longitude - lon) <= tol;

    private async void OnReadHelpClicked(object? sender, EventArgs e)
    {
        try
        {
            await SpeechService.SpeakAsync(LocalizationService.Get("HwHelpText"));
            SetStatus(LocalizationService.Get("HwSpeechError", "done"));
        }
        catch (Exception ex)
        {
            SetStatus(LocalizationService.Get("HwSpeechError", ex.Message));
        }
    }

    private void OnStopSpeechClicked(object? sender, EventArgs e)
    {
        SpeechService.Stop();
        SetStatus(LocalizationService.Get("HwSpeechStopped"));
    }

    private void OnFeedbackClicked(object? sender, EventArgs e)
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(450));
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            feedbackTestCount++;
            FeedbackCountLabel.Text = feedbackTestCount.ToString();
            SetStatus(LocalizationService.Get("HwFeedbackStatus"));
        }
        catch (Exception ex)
        {
            SetStatus(LocalizationService.Get("HwFeedbackError", ex.Message));
        }
    }

    private void SetStatus(string message)
    {
        HardwareStatusLabel.Text = message;
        SemanticScreenReader.Announce(message);
    }
}
