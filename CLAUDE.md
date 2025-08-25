# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SAnalytics is a WinUI 3 desktop application built on .NET 8, targeting Windows 10.0.19041.0+ with MSIX packaging support. The application uses MVVM architecture with CommunityToolkit.Mvvm and Microsoft.Extensions.Hosting for dependency injection.

## Development Commands

### Build and Run
```bash
# Build the solution
dotnet build SAnalytics.Desktop/SAnalytics.Desktop.sln

# Run the application (unpackaged)
dotnet run --project SAnalytics.Desktop/SAnalytics.Desktop.csproj

# Clean solution
dotnet clean SAnalytics.Desktop/SAnalytics.Desktop.sln
```

### Publishing
```bash
# Publish for different platforms (using publish profiles)
dotnet publish SAnalytics.Desktop/SAnalytics.Desktop.csproj -p:PublishProfile=win-x64
dotnet publish SAnalytics.Desktop/SAnalytics.Desktop.csproj -p:PublishProfile=win-x86  
dotnet publish SAnalytics.Desktop/SAnalytics.Desktop.csproj -p:PublishProfile=win-arm64
```

## Architecture

### Project Structure
- **Core/**: Infrastructure components
  - `ServiceExtensions.cs`: DI container configuration for ViewModels and services
  - `ViewModels/BaseViewModel.cs`: Base class for all ViewModels with IsBusy and Title properties
  - `Converters/`: XAML value converters
- **ViewModels/**: MVVM ViewModels organized by feature
  - `Auth/`: Authentication-related ViewModels
  - `Analytics/`: Dashboard and analytics ViewModels  
- **Views/**: XAML views and pages
  - `Pages/`: Application pages
  - `Controls/`: Reusable user controls
- **Services/**: Business logic services (placeholder)
- **Models/Data/**: Data models (placeholder)

### Key Technologies
- **WinUI 3**: Modern Windows UI framework
- **CommunityToolkit.Mvvm**: MVVM framework with source generators
- **Microsoft.Extensions.Hosting**: Dependency injection and service configuration
- **MSIX Packaging**: Modern Windows app packaging

### Dependency Injection
The application uses Microsoft.Extensions.Hosting with service registration in `App.xaml.cs:25-27` and extension methods in `Core/ServiceExtensions.cs`. Services are accessed via `App.GetService<T>()`.

### MVVM Pattern
- ViewModels inherit from `BaseViewModel` which provides `IsBusy` and `Title` properties
- Uses CommunityToolkit.Mvvm source generators for `[ObservableProperty]` and `[RelayCommand]`
- ViewModels are registered as Transient services in DI container

## Development Notes

- Application entry point starts with `LoginWindow` (App.xaml.cs:44)
- Currently has placeholder authentication with hardcoded credentials (admin/admin)
- Project supports multiple architectures: x86, x64, ARM64
- MSIX packaging enabled for Windows Store distribution
- German UI text present in some components