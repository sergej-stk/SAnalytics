# Project Overview

This is a .NET 8 WinUI 3 desktop application for Windows called SAnalytics. It serves as a modern analytics dashboard with features like real-time analytics, secure authentication, and MSIX packaging. The application is built using the MVVM architecture, ensuring a clean and maintainable code structure.

## Building and Running

### Prerequisites

- .NET 8.0 SDK
- Windows 10 SDK (10.0.19041.0 or later)
- Visual Studio 2022 (recommended) or Visual Studio Code
- Windows 10 version 1903 (build 18362) or later

### Installation

1.  **Clone the repository**
    ```bash
    git clone https://github.com/sergej-stk/SAnalytics.git
    cd SAnalytics
    ```

2.  **Restore dependencies**
    ```bash
    dotnet restore SAnalytics.Desktop/SAnalytics.Desktop.sln
    ```

3.  **Build the application**
    ```bash
    dotnet build SAnalytics.Desktop/SAnalytics.Desktop.sln --configuration Release
    ```

### Running the Application

#### Development Mode

```bash
dotnet run --project SAnalytics.Desktop/SAnalytics.Desktop.csproj
```

#### Published Application

```bash
# Publish for your platform
dotnet publish SAnalytics.Desktop/SAnalytics.Desktop.csproj -p:PublishProfile=win-x64

# Navigate to publish directory and run
cd SAnalytics.Desktop/bin/Release/net8.0-windows10.0.19041.0/win-x64/publish/
./SAnalytics.Desktop.exe
```

## Development Conventions

- The project follows the MVVM (Model-View-ViewModel) pattern.
- It uses `CommunityToolkit.Mvvm` for MVVM implementation.
- Dependency injection is configured in `SAnalytics.Desktop/Core/ServiceExtensions.cs`.
- The application entry point is `SAnalytics.Desktop/App.xaml.cs`.
- The main window is `SAnalytics.Desktop/MainWindow.xaml.cs`.
- Views are located in the `SAnalytics.Desktop/Views` directory.
- ViewModels are located in the `SAnalytics.Desktop/ViewModels` directory.
- Services are located in the `SAnalytics.Desktop/Services` directory.
