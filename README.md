# NutriBite (食光营养助手)

A cross-platform mobile application built with .NET MAUI for tracking food and drink nutrition. Developed as the final assignment for the **6G6Z0014 Mobile Computing** module at Manchester Metropolitan University.

## Overview

NutriBite lets users browse, search, and add food and drink records with full nutritional information. It integrates five mobile hardware features — camera, location/geolocation, text-to-speech, vibration, and haptic feedback — and follows WCAG 2.x accessibility guidelines with dark/light theme support, large-text mode, and screen-reader compatibility.

## Features

- **Food & drink list** — pull-to-refresh `CollectionView` with nutrition cards
- **Search** — real-time filtering across name, category, description, and tags
- **Nutrition details** — full breakdown of calories, protein, carbs, fat, and allergy notes
- **Add record** — validated form with vibration feedback on errors
- **Mobile hardware demo page:**
  - 📷 Camera — capture food photos
  - 📍 Geolocation + reverse geocoding — get meal location with country/city/coordinates
  - 🔊 Text-to-speech — read nutrition summaries aloud
  - 📳 Vibration + haptic feedback — tactile alerts with visual counter
- **Accessibility settings:**
  - System / light / dark theme switching
  - Large-text mode with immediate preview
  - `SemanticProperties` on all interactive controls
  - `SemanticScreenReader.Announce` for status changes
- **Resilient data layer** — mockapi.io REST API with transparent local fallback

## Architecture

```
FoodDrinkApp/
├── Models/
│   └── FoodItem.cs              # Data model with nutrition properties
├── Services/
│   ├── FoodCatalogService.cs    # API integration + local fallback
│   ├── SpeechService.cs         # Text-to-speech wrapper
│   ├── AccessibilityService.cs  # Font scaling for large-text mode
│   └── MockApiConfig.cs         # API endpoint configuration
├── BasePage.cs                  # Shared base page (accessibility + speech)
├── *.xaml / *.xaml.cs           # Page definitions and code-behind
├── Platforms/                   # Platform-specific code and permissions
└── Resources/                   # Styles, fonts, images
```

**Design patterns:**
- **Models / Services / Pages** layered separation
- **BasePage** inheritance to reuse `OnAppearing` (accessibility) and `OnDisappearing` (speech stop) logic
- **Try-catch with fallback** on all hardware and network operations — the app never crashes during demos
- **MockApiConfig** toggle: empty endpoint → local-only mode for offline reliability

## Build & Run

### Prerequisites
- .NET 9.0 SDK
- MAUI workload: `dotnet workload install maui`

### Android
```powershell
# Build
dotnet build .\FoodDrinkApp\FoodDrinkApp.csproj -f net9.0-android --no-incremental

# Run (requires emulator or connected device)
dotnet build .\FoodDrinkApp\FoodDrinkApp.csproj -f net9.0-android -t:Run
```

### Windows
```powershell
dotnet build .\FoodDrinkApp\FoodDrinkApp.csproj -f net9.0-windows10.0.19041.0 --no-incremental
```

> **Note:** Build output is redirected to `C:\MauiBuild\FoodDrinkApp\` via `Directory.Build.props` to avoid path-length issues with Chinese characters in the project path.

## Mock API Configuration

See [mockapi配置说明.md](mockapi配置说明.md) for detailed instructions on setting up the mockapi.io data source. By default, the app uses built-in local fallback data and requires no configuration.

## Development Plan

| Phase | Theme | Contents |
|-------|-------|----------|
| 1 | MainPage skeleton | Project structure, navigation shell, food list, data model, styles |
| 2 | Detail + Add + Settings | Nutrition detail page, add-item form with validation, theme/font settings, speech service |
| 3 | Hardware implementation | Camera, geolocation, geocoding, TTS, vibration, haptic feedback, Android permissions |
| 4 | Polish & quality | XML comments, BasePage refactor, README, debounce, error message improvements |

## Assessment Criteria Mapping

| Criterion | Weight | Implementation |
|-----------|--------|---------------|
| UI/UX Design & Accessibility | 30% | XAML-based 5-page UI, warm food-theme palette, dark/light theme, large-text mode, `SemanticProperties`, screen-reader announcements |
| Use of Mobile Hardware | 20% | Camera, Geolocation, Geocoding, Text-to-Speech, Vibration + Haptic Feedback (5 features) |
| Functionality | 20% | Search, list, detail, add form, settings, hardware demo — all demonstrated in screencast |
| Validation & Error Handling | 10% | Required-field and numeric validation, try-catch on all hardware ops, permission handling, network fallback |
| Code Quality | 10% | Layered architecture, XML documentation, BasePage reuse, consistent naming, service encapsulation |
| Deployment | 5% | Android + Windows builds verified; cross-platform target frameworks (Android, iOS, macOS, Windows) |
| GitHub Usage | 5% | Multi-phase commits with descriptive messages, detailed README with dev plan |

## WCAG Accessibility

The app follows WCAG 2.x principles:

| Principle | Applied |
|-----------|---------|
| **Perceivable** | `SemanticProperties` (HeadingLevel, Hint, Description), alternative text for images, sufficient colour contrast in both themes |
| **Operable** | All functionality available via touch and keyboard navigation, no keyboard traps |
| **Understandable** | Consistent navigation, clear labels, validation error messages, readable typography |
| **Robust** | Compatible with platform screen readers (`SemanticScreenReader.Announce`), standard MAUI controls |

## Author

- **Name:** Zhang Boyan
- **Student ID:** 21906056
- **GitHub:** zby-1-s
- **Module:** 6G6Z0014 Mobile Computing (1CWK100)
- **Institution:** Manchester Metropolitan University
