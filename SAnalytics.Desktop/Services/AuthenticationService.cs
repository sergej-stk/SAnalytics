using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SAnalytics.Desktop.Services;

/// <summary>
/// Implementation of authentication service for the application.
/// Provides secure authentication with password hashing and session management.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IAppConfigurationService _configurationService;
    private IUserPrincipal? _currentUser;
    private string? _authToken;
    private DateTime? _tokenExpiresAt;

    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        IAppConfigurationService configurationService)
    {
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    }

    public IUserPrincipal? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_authToken);
    public string? AuthToken => _authToken;

    public event EventHandler<AuthenticationStateChangedEventArgs>? AuthenticationStateChanged;

    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password, bool rememberMe = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            Log.Warning("Authentication attempted with empty username");
            return AuthenticationResult.Failure(AuthenticationError.InvalidCredentials, "Username is required");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            Log.Warning("Authentication attempted with empty password for user {Username}", username);
            return AuthenticationResult.Failure(AuthenticationError.InvalidCredentials, "Password is required");
        }

        try
        {
            Log.Information("Authenticating user {Username}", username);

            // Simulate async authentication process
            await Task.Delay(500, cancellationToken); // Simulate network delay

            // For now, we'll use a simple authentication mechanism
            // In a real application, this would authenticate against a backend service
            var authResult = await ValidateCredentialsAsync(username, password, cancellationToken);
            
            if (!authResult.IsSuccess)
            {
                Log.Warning("Authentication failed for user {Username}: {Error}", username, authResult.ErrorMessage);
                return authResult;
            }

            // Ensure user is not null
            if (authResult.User == null)
            {
                Log.Error("Authentication succeeded but user is null for {Username}", username);
                return AuthenticationResult.Failure(AuthenticationError.UnknownError, "Authentication failed - invalid user data");
            }

            // Generate authentication token
            var token = GenerateAuthToken();
            var expiresAt = DateTime.UtcNow.AddHours(8); // Token expires in 8 hours

            _currentUser = authResult.User;
            _authToken = token;
            _tokenExpiresAt = expiresAt;

            // Handle remember me functionality
            if (rememberMe)
            {
                await _configurationService.SetValueAsync(ConfigurationKeys.RememberLogin, true);
                await _configurationService.SetValueAsync(ConfigurationKeys.LastLoginUser, username);
                
                // Store hashed token for auto-authentication (in real app, use secure storage)
                var hashedToken = HashPassword(token);
                await _configurationService.SetValueAsync("Auth.RememberToken", hashedToken);
            }
            else
            {
                await _configurationService.SetValueAsync(ConfigurationKeys.RememberLogin, false);
                await _configurationService.RemoveKeyAsync(ConfigurationKeys.LastLoginUser);
                await _configurationService.RemoveKeyAsync("Auth.RememberToken");
            }

            OnAuthenticationStateChanged(new AuthenticationStateChangedEventArgs
            {
                IsAuthenticated = true,
                User = _currentUser,
                Reason = "User authenticated successfully"
            });

            Log.Information("User {Username} authenticated successfully", username);
            
            return AuthenticationResult.Success(_currentUser, token, expiresAt);
        }
        catch (OperationCanceledException)
        {
            Log.Information("Authentication cancelled for user {Username}", username);
            return AuthenticationResult.Failure(AuthenticationError.UnknownError, "Operation was cancelled");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during authentication for user {Username}", username);
            return AuthenticationResult.Failure(AuthenticationError.UnknownError, "An unexpected error occurred during authentication");
        }
    }

    public async Task<AuthenticationResult> TryAutoAuthenticateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rememberLogin = await _configurationService.GetValueAsync(ConfigurationKeys.RememberLogin, false);
            
            if (!rememberLogin)
            {
                Log.Debug("Auto-authentication skipped - remember me not enabled");
                return AuthenticationResult.Failure(AuthenticationError.InvalidCredentials, "Remember me not enabled");
            }

            var lastUser = await _configurationService.GetValueAsync<string>(ConfigurationKeys.LastLoginUser);
            var storedToken = await _configurationService.GetValueAsync<string>("Auth.RememberToken");

            if (string.IsNullOrEmpty(lastUser) || string.IsNullOrEmpty(storedToken))
            {
                Log.Debug("Auto-authentication failed - missing stored credentials");
                return AuthenticationResult.Failure(AuthenticationError.InvalidCredentials, "No stored credentials found");
            }

            Log.Information("Attempting auto-authentication for user {Username}", lastUser);

            // In a real application, you would validate the stored token with your backend
            // For now, we'll simulate a successful auto-authentication
            await Task.Delay(200, cancellationToken);

            var user = new UserPrincipal
            {
                UserId = Guid.NewGuid().ToString(),
                Username = lastUser,
                DisplayName = GetDisplayName(lastUser),
                Email = $"{lastUser}@example.com",
                Roles = GetUserRoles(lastUser),
                AuthenticatedAt = DateTime.UtcNow
            };

            var token = GenerateAuthToken();
            var expiresAt = DateTime.UtcNow.AddHours(8);

            _currentUser = user;
            _authToken = token;
            _tokenExpiresAt = expiresAt;

            OnAuthenticationStateChanged(new AuthenticationStateChangedEventArgs
            {
                IsAuthenticated = true,
                User = _currentUser,
                Reason = "Auto-authentication successful"
            });

            Log.Information("Auto-authentication successful for user {Username}", lastUser);
            
            return AuthenticationResult.Success(user, token, expiresAt);
        }
        catch (OperationCanceledException)
        {
            Log.Information("Auto-authentication cancelled");
            return AuthenticationResult.Failure(AuthenticationError.UnknownError, "Operation was cancelled");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during auto-authentication");
            return AuthenticationResult.Failure(AuthenticationError.UnknownError, "An unexpected error occurred during auto-authentication");
        }
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var username = _currentUser?.Username ?? "Unknown";
            
            Log.Information("Signing out user {Username}", username);

            // Clear current session
            _currentUser = null;
            _authToken = null;
            _tokenExpiresAt = null;

            // Clear remember me data if requested
            await _configurationService.SetValueAsync(ConfigurationKeys.RememberLogin, false);
            await _configurationService.RemoveKeyAsync(ConfigurationKeys.LastLoginUser);
            await _configurationService.RemoveKeyAsync("Auth.RememberToken");

            OnAuthenticationStateChanged(new AuthenticationStateChangedEventArgs
            {
                IsAuthenticated = false,
                User = null,
                Reason = "User signed out"
            });

            Log.Information("User {Username} signed out successfully", username);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during sign out");
            throw;
        }
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser == null || string.IsNullOrEmpty(_authToken))
        {
            return AuthenticationResult.Failure(AuthenticationError.InvalidToken, "No active session to refresh");
        }

        try
        {
            Log.Debug("Refreshing token for user {Username}", _currentUser.Username);

            // Simulate token refresh
            await Task.Delay(200, cancellationToken);

            var newToken = GenerateAuthToken();
            var expiresAt = DateTime.UtcNow.AddHours(8);

            _authToken = newToken;
            _tokenExpiresAt = expiresAt;

            Log.Debug("Token refreshed successfully for user {Username}", _currentUser.Username);
            
            return AuthenticationResult.Success(_currentUser, newToken, expiresAt);
        }
        catch (OperationCanceledException)
        {
            Log.Information("Token refresh cancelled for user {Username}", _currentUser?.Username);
            return AuthenticationResult.Failure(AuthenticationError.UnknownError, "Operation was cancelled");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error refreshing token for user {Username}", _currentUser?.Username);
            return AuthenticationResult.Failure(AuthenticationError.UnknownError, "Failed to refresh token");
        }
    }

    public async Task<bool> ValidateSessionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser == null || string.IsNullOrEmpty(_authToken))
        {
            return false;
        }

        if (_tokenExpiresAt.HasValue && _tokenExpiresAt.Value <= DateTime.UtcNow)
        {
            Log.Information("Session expired for user {Username}", _currentUser.Username);
            await SignOutAsync(cancellationToken);
            return false;
        }

        // In a real application, you would validate the token with your backend
        return true;
    }

    private async Task<AuthenticationResult> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken)
    {
        // For demonstration purposes, we'll use simple credential validation
        // In a real application, this would authenticate against a secure backend
        
        var validCredentials = new Dictionary<string, (string password, string[] roles)>
        {
            { "admin", ("admin", new[] { "Administrator", "User" }) },
            { "user", ("user", new[] { "User" }) },
            { "demo", ("demo", new[] { "User" }) }
        };

        await Task.Delay(100, cancellationToken); // Simulate backend call

        if (!validCredentials.TryGetValue(username.ToLowerInvariant(), out var credentialInfo))
        {
            return AuthenticationResult.Failure(AuthenticationError.UserNotFound, "User not found");
        }

        // In a real application, you would hash the password and compare hashes
        if (credentialInfo.password != password)
        {
            return AuthenticationResult.Failure(AuthenticationError.InvalidCredentials, "Invalid password");
        }

        var user = new UserPrincipal
        {
            UserId = Guid.NewGuid().ToString(),
            Username = username,
            DisplayName = GetDisplayName(username),
            Email = $"{username}@example.com",
            Roles = credentialInfo.roles,
            AuthenticatedAt = DateTime.UtcNow
        };

        return AuthenticationResult.Success(user, string.Empty);
    }

    private static string GetDisplayName(string username)
    {
        return username.ToLowerInvariant() switch
        {
            "admin" => "Administrator",
            "user" => "Standard User",
            "demo" => "Demo User",
            _ => username
        };
    }

    private static string[] GetUserRoles(string username)
    {
        return username.ToLowerInvariant() switch
        {
            "admin" => new[] { "Administrator", "User" },
            _ => new[] { "User" }
        };
    }

    private static string GenerateAuthToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "SaltKey"));
        return Convert.ToBase64String(hashedBytes);
    }

    protected virtual void OnAuthenticationStateChanged(AuthenticationStateChangedEventArgs args)
    {
        AuthenticationStateChanged?.Invoke(this, args);
    }
}