# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SAnalytics is a WinUI 3 desktop application built on .NET 8, targeting Windows 10.0.19041.0+ with MSIX packaging support. The application uses MVVM architecture with CommunityToolkit.Mvvm and Microsoft.Extensions.Hosting for dependency injection.

**Solution Structure:**
- `SAnalytics.sln` - Main solution file in project root
- `SAnalytics.Desktop/` - WinUI 3 desktop application project
- `.github/workflows/` - CI/CD pipelines for automated builds and releases

## Development Commands

### Build and Run
```bash
# Build the solution (from project root)
dotnet build SAnalytics.sln

# Run the application (unpackaged)
dotnet run --project SAnalytics.Desktop/SAnalytics.Desktop.csproj

# Clean solution
dotnet clean SAnalytics.sln

# Restore dependencies
dotnet restore SAnalytics.sln
```

### Publishing
```bash
# Publish for different platforms (using publish profiles)
dotnet publish SAnalytics.Desktop/SAnalytics.Desktop.csproj -p:PublishProfile=win-x64
dotnet publish SAnalytics.Desktop/SAnalytics.Desktop.csproj -p:PublishProfile=win-x86  
dotnet publish SAnalytics.Desktop/SAnalytics.Desktop.csproj -p:PublishProfile=win-arm64

# Alternative: Direct publish commands
dotnet publish SAnalytics.Desktop/SAnalytics.Desktop.csproj --configuration Release --runtime win-x64 --self-contained true --output ./publish/x64
dotnet publish SAnalytics.Desktop/SAnalytics.Desktop.csproj --configuration Release --runtime win-x86 --self-contained true --output ./publish/x86
dotnet publish SAnalytics.Desktop/SAnalytics.Desktop.csproj --configuration Release --runtime win-arm64 --self-contained true --output ./publish/arm64
```

## Architecture

### Application Structure
The application follows a sophisticated MVVM pattern with proper dependency injection and service-oriented architecture:

- **Application Host**: Uses Microsoft.Extensions.Hosting with comprehensive service registration in `App.xaml.cs:56-64`
- **Navigation**: MainWindow initializes with LoginPage and uses INavigationService for page navigation
- **Exception Handling**: UnhandledException event handler in `App.xaml.cs:28-32` logs fatal errors via Serilog
- **Service Lifecycle**: Core services (Theme, Configuration, Authentication) initialized asynchronously in `App.xaml.cs:66-97`

### Project Structure
- **Core/**: Infrastructure components
  - `ServiceExtensions.cs`: Comprehensive DI configuration with service validation and logging
  - `ViewModels/BaseViewModel.cs`: Rich base class with localization, logging, async operations with cancellation support
  - `Converters/`: XAML value converters
  - `Exceptions/`: Custom exception handling for WinUI 3
- **ViewModels/**: MVVM ViewModels organized by feature
  - `Auth/`: Authentication-related ViewModels (LoginViewModel)
  - `Analytics/`: Dashboard and analytics ViewModels (DashboardViewModel)
  - `Settings/`: Configuration ViewModels (SettingsViewModel, ThemeSelectorViewModel)
  - `Controls/`: Control-specific ViewModels
  - `Dialogs/`: Dialog ViewModels with factory pattern for exception handling
- **Views/**: XAML views and pages
  - `Pages/`: Application pages (LoginPage, DashboardPage, SettingsPage)
  - `Controls/`: Reusable user controls (ThemeSelector, LanguageSelector with hover variants)
- **Services/**: Business logic services with interface-based design
  - INavigationService, IAuthenticationService, IAppConfigurationService
  - ILocalizationService, IThemeService
- **Models/Data/**: Data models (placeholder structure)

### Key Technologies
- **WinUI 3**: Modern Windows UI framework with WindowsAppSDK 1.8.250916003
- **CommunityToolkit.Mvvm**: MVVM framework with source generators for ObservableProperty and RelayCommand
- **Microsoft.Extensions.Hosting**: Full hosting model with DI, logging, and configuration
- **Serilog**: Structured logging with console and debug sinks, configured in `App.xaml.cs:121-127`
- **MSIX Packaging**: Modern Windows app packaging with multi-architecture support

### Dependency Injection Architecture
The application uses a sophisticated DI setup:
- **Service Registration**: `Core/ServiceExtensions.cs` provides extension methods for different service categories
- **Service Validation**: Built-in validation ensures critical services are properly registered
- **Lifetime Management**: Singleton services for app-wide state, Transient ViewModels for UI components
- **Factory Pattern**: Used for ViewModels requiring constructor parameters (ExceptionDialogViewModel)
- **Service Access**: Services accessed via `App.Services` property throughout the application

### MVVM Pattern Implementation
- **BaseViewModel**: Comprehensive base class with localization, logging, error handling, and async operation support
- **Observable Properties**: Uses CommunityToolkit.Mvvm source generators for `[ObservableProperty]`
- **Command Pattern**: `[RelayCommand]` for user interactions with automatic busy state management
- **Error Handling**: Built-in error state management with localized error messages
- **Resource Management**: Proper disposal pattern with event unsubscription
- **Async Operations**: Structured async operation execution with cancellation token support

### Localization and Theming
- **ILocalizationService**: Centralized localization with culture change notifications
- **IThemeService**: Theme management with system integration
- **Language Support**: Multi-language UI with German text currently present
- **Dynamic Updates**: ViewModels automatically update when language/theme changes

## CI/CD Pipeline

The project includes GitHub Actions workflows for automated building and deployment:

- **CI Pipeline** (`.github/workflows/ci.yml`): 
  - Runs on push/PR to main/develop branches
  - Builds for all platforms (x64, x86, ARM64) in Debug and Release configurations
  - Includes code quality analysis
- **Release Pipeline** (`.github/workflows/release.yml`):
  - Triggered on GitHub releases or manual dispatch
  - Creates MSIX packages for all platforms
  - Uploads release artifacts and packages

## Development Notes

- **Application Entry**: Starts with MainWindow which navigates to LoginPage
- **Authentication**: Placeholder authentication system with auto-login attempt on startup
- **Platform Support**: Multi-architecture support (x86, x64, ARM64) with dedicated publish profiles
- **MSIX Packaging**: Enabled for Windows Store distribution with proper manifest configuration
- **Logging**: Serilog configured with Debug minimum level, outputs to Debug and Console sinks
- **Publish Settings**: ReadyToRun and Trimming disabled for WinUI 3 compatibility (see `SAnalytics.Desktop.csproj:60-63`)

## Important Instructions

Do what has been asked; nothing more, nothing less.
NEVER create files unless they're absolutely necessary for achieving your goal.
ALWAYS prefer editing an existing file to creating a new one.
NEVER proactively create documentation files (*.md) or README files. Only create documentation files if explicitly requested by the User.