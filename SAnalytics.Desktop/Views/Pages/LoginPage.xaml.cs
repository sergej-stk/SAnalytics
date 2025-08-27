using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SAnalytics.Desktop.ViewModels.Auth;
using System;
using System.Collections.Generic;

namespace SAnalytics.Desktop.Views.Pages;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; }

    public LoginPage()
    {
        ViewModel = App.GetService<LoginViewModel>();
        InitializeComponent();
        
        // Start particle animations when page loads
        this.Loaded += (sender, e) => 
        {
            PositionParticlesRandomly();
            ParticleAnimation.Begin();
        };
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.ResetForm();
        
        // Restart animations when navigating to this page
        PositionParticlesRandomly();
        ParticleAnimation.Begin();
    }

    private void PositionParticlesRandomly()
    {
        var random = new Random();
        var particles = new List<FrameworkElement> { Particle1, Particle2, Particle3, Particle4, Particle5 };
        var placedParticles = new List<(double X, double Y, double Width, double Height)>();
        const double minDistance = 20; // Minimum distance between particles in pixels
        
        // Get canvas dimensions (use ActualWidth/Height if available, otherwise use reasonable defaults)
        var canvasWidth = ParticlesCanvas.ActualWidth > 0 ? ParticlesCanvas.ActualWidth : 1200;
        var canvasHeight = ParticlesCanvas.ActualHeight > 0 ? ParticlesCanvas.ActualHeight : 800;
        
        // Calculate login panel area to avoid (center area with some padding)
        var panelWidth = Math.Min(500, canvasWidth * 0.6); // 60% of canvas or 500px max
        var panelHeight = Math.Min(600, canvasHeight * 0.8); // 80% of canvas or 600px max
        var panelLeft = (canvasWidth - panelWidth) / 2;
        var panelRight = panelLeft + panelWidth;
        var panelTop = (canvasHeight - panelHeight) / 2;
        var panelBottom = panelTop + panelHeight;
        
        foreach (var particle in particles)
        {
            double x, y;
            int attempts = 0;
            
            do
            {
                // Generate random position within canvas bounds
                var maxX = Math.Max(0, canvasWidth - particle.Width);
                var maxY = Math.Max(0, canvasHeight - particle.Height);
                
                x = random.NextDouble() * maxX;
                y = random.NextDouble() * maxY;
                
                attempts++;
            } 
            while (attempts < 100 && (
                IsInLoginPanelArea(x, y, particle.Width, particle.Height, panelLeft, panelRight, panelTop, panelBottom) ||
                OverlapsWithOtherParticles(x, y, particle.Width, particle.Height, placedParticles, minDistance)));
            
            Canvas.SetLeft(particle, x);
            Canvas.SetTop(particle, y);
            
            // Add this particle to the placed particles list
            placedParticles.Add((x, y, particle.Width, particle.Height));
        }
    }
    
    private bool IsInLoginPanelArea(double x, double y, double width, double height, 
        double panelLeft, double panelRight, double panelTop, double panelBottom)
    {
        // Check if particle overlaps with login panel area
        var particleRight = x + width;
        var particleBottom = y + height;
        
        return !(x >= panelRight || particleRight <= panelLeft || y >= panelBottom || particleBottom <= panelTop);
    }
    
    private bool OverlapsWithOtherParticles(double x, double y, double width, double height,
        List<(double X, double Y, double Width, double Height)> placedParticles, double minDistance)
    {
        foreach (var placed in placedParticles)
        {
            // Calculate distance between particle centers
            var currentCenterX = x + width / 2;
            var currentCenterY = y + height / 2;
            var placedCenterX = placed.X + placed.Width / 2;
            var placedCenterY = placed.Y + placed.Height / 2;
            
            var distance = Math.Sqrt(Math.Pow(currentCenterX - placedCenterX, 2) + Math.Pow(currentCenterY - placedCenterY, 2));
            
            // Calculate minimum required distance (sum of radii + minimum distance)
            var currentRadius = Math.Max(width, height) / 2;
            var placedRadius = Math.Max(placed.Width, placed.Height) / 2;
            var requiredDistance = currentRadius + placedRadius + minDistance;
            
            if (distance < requiredDistance)
            {
                return true; // Overlap detected
            }
        }
        
        return false; // No overlap
    }
}