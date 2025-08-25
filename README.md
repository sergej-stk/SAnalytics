<div align="center">
  <h1>📊 SAnalytics</h1>
  <p><strong>Modern Analytics Dashboard for Windows</strong></p>
  <p>A WinUI 3 desktop application built with .NET 8 for comprehensive data analytics and insights.</p>
  
  ![Build Status](https://github.com/sergej-stk/SAnalytics/workflows/CI/badge.svg)
  ![Release](https://github.com/sergej-stk/SAnalytics/workflows/Release/badge.svg)
  ![License](https://img.shields.io/badge/license-MIT-blue.svg)
  ![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
  ![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)
</div>

---

## ✨ Features

- 🎨 **Modern WinUI 3 Interface** - Native Windows 11 design with Fluent Design System
- 📈 **Real-time Analytics** - Interactive dashboards with live data visualization
- 🔐 **Secure Authentication** - User management and access control
- 📦 **MSIX Packaging** - Modern Windows app deployment and distribution
- 🏗️ **MVVM Architecture** - Clean, maintainable code structure
- ⚡ **Multi-platform Support** - x64, x86, and ARM64 architectures

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Windows 10 SDK (10.0.19041.0 or later)](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (recommended) or Visual Studio Code
- Windows 10 version 1903 (build 18362) or later

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/sergej-stk/SAnalytics.git
   cd SAnalytics
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore SAnalytics.Desktop/SAnalytics.Desktop.sln
   ```

3. **Build the application**
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

## 🏗️ Architecture

SAnalytics follows modern .NET patterns and best practices:

- **WinUI 3** - Microsoft's modern UI framework for Windows
- **MVVM Pattern** - Clean separation of concerns with CommunityToolkit.Mvvm
- **Dependency Injection** - Microsoft.Extensions.Hosting for service management
- **MSIX Packaging** - Modern Windows application deployment

### Project Structure
```
SAnalytics.Desktop/
├── Core/                    # Infrastructure & shared components
│   ├── ViewModels/         # Base classes for MVVM
│   ├── Converters/         # XAML value converters
│   └── ServiceExtensions.cs # DI configuration
├── ViewModels/             # Application view models
│   ├── Auth/              # Authentication VMs
│   └── Analytics/         # Dashboard VMs
├── Views/                 # XAML user interface
│   ├── Pages/            # Application pages
│   └── Controls/         # Reusable UI controls
├── Services/             # Business logic services
└── Models/              # Data models & DTOs
```

## 🤝 Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Development Guidelines

- Follow the existing MVVM patterns
- Use CommunityToolkit.Mvvm source generators
- Maintain consistent code style
- Add appropriate documentation
- Test your changes thoroughly

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

## 🎯 Roadmap

- [ ] Advanced analytics visualizations
- [ ] Data export capabilities
- [ ] Custom dashboard creation
- [ ] Real-time data streaming
- [ ] Multi-language support
- [ ] Plugin architecture

---

<div align="center">
  <p>Made with ❤️ by <a href="https://github.com/sergej-stk">Sergej Steinsiek</a></p>
  <p><a href="#-sanalytics">Back to top</a></p>
</div>