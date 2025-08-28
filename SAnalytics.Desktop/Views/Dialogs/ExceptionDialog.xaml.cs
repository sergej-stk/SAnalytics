using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.ViewModels.Dialogs;
using System;

namespace SAnalytics.Desktop.Views.Dialogs;

public sealed partial class ExceptionDialog : ContentDialog
{
    public ExceptionDialogViewModel ViewModel { get; private set; }

    public ExceptionDialog(Exception exception, string? userMessage = null, Func<Exception, string?, ExceptionDialogViewModel>? factory = null)
    {
        if (factory != null)
        {
            ViewModel = factory(exception, userMessage);
        }
        else
        {
            // Fallback - should not happen in normal DI usage
            throw new InvalidOperationException("ExceptionDialog requires a ViewModel factory");
        }
        this.InitializeComponent();
        
        // Set debug mode expander visibility
#if DEBUG
        ExceptionDetailsExpander.IsExpanded = true; // Show details by default in debug
#else
        ExceptionDetailsExpander.IsExpanded = false;
#endif
        
        // Subscribe to ViewModel property changes to update InfoBar feedback
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.FeedbackMessage) && !string.IsNullOrEmpty(ViewModel.FeedbackMessage))
            {
                UserActionsBar.Message = ViewModel.FeedbackMessage;
            }
        };
        
        // Set up button event handlers
        this.PrimaryButtonClick += OnPrimaryButtonClick;
        this.SecondaryButtonClick += OnSecondaryButtonClick;
    }


    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        ViewModel.CopyToClipboardCommand.Execute(null);
    }

    private async void OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        await ViewModel.SaveToFileCommand.ExecuteAsync(null);
    }
}