using Microsoft.UI.Xaml;

namespace SAnalytics.Desktop;

public sealed partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(null);
    }
}