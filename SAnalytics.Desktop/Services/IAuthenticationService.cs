using System;
using System.Threading;
using System.Threading.Tasks;

namespace SAnalytics.Desktop.Services;

/// <summary>
/// Service for handling user authentication and authorization.
/// Provides secure authentication mechanisms and user session management.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with username and password.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="rememberMe">Whether to remember the user for future sessions.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The authentication result.</returns>
    Task<AuthenticationResult> AuthenticateAsync(string username, string password, bool rememberMe = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Attempts to authenticate using stored credentials (remember me functionality).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The authentication result.</returns>
    Task<AuthenticationResult> TryAutoAuthenticateAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Signs out the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the operation.</returns>
    Task SignOutAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the currently authenticated user.
    /// </summary>
    IUserPrincipal? CurrentUser { get; }
    
    /// <summary>
    /// Gets whether a user is currently authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Gets the authentication token for the current session.
    /// </summary>
    string? AuthToken { get; }
    
    /// <summary>
    /// Refreshes the authentication token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The refresh result.</returns>
    Task<AuthenticationResult> RefreshTokenAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates if the current session is still valid.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the session is valid, false otherwise.</returns>
    Task<bool> ValidateSessionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Event raised when authentication state changes.
    /// </summary>
    event EventHandler<AuthenticationStateChangedEventArgs>? AuthenticationStateChanged;
}

/// <summary>
/// Represents the result of an authentication operation.
/// </summary>
public class AuthenticationResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public AuthenticationError? ErrorCode { get; init; }
    public IUserPrincipal? User { get; init; }
    public string? AuthToken { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool RequiresTwoFactor { get; init; }

    public static AuthenticationResult Success(IUserPrincipal user, string authToken, DateTime? expiresAt = null)
    {
        return new AuthenticationResult
        {
            IsSuccess = true,
            User = user,
            AuthToken = authToken,
            ExpiresAt = expiresAt
        };
    }

    public static AuthenticationResult Failure(AuthenticationError errorCode, string errorMessage)
    {
        return new AuthenticationResult
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
    }

    public static AuthenticationResult RequiresTwoFactorAuthentication()
    {
        return new AuthenticationResult
        {
            IsSuccess = false,
            RequiresTwoFactor = true,
            ErrorMessage = "Two-factor authentication required"
        };
    }
}

/// <summary>
/// Enumeration of possible authentication errors.
/// </summary>
public enum AuthenticationError
{
    InvalidCredentials,
    UserNotFound,
    AccountLocked,
    AccountDisabled,
    PasswordExpired,
    TwoFactorRequired,
    NetworkError,
    ServiceUnavailable,
    InvalidToken,
    TokenExpired,
    UnknownError
}

/// <summary>
/// Represents an authenticated user.
/// </summary>
public interface IUserPrincipal
{
    string UserId { get; }
    string Username { get; }
    string? DisplayName { get; }
    string? Email { get; }
    string[] Roles { get; }
    DateTime AuthenticatedAt { get; }
    bool IsInRole(string role);
}

/// <summary>
/// Implementation of user principal.
/// </summary>
public class UserPrincipal : IUserPrincipal
{
    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Email { get; init; }
    public string[] Roles { get; init; } = Array.Empty<string>();
    public DateTime AuthenticatedAt { get; init; } = DateTime.UtcNow;

    public bool IsInRole(string role)
    {
        return Array.Exists(Roles, r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Event arguments for authentication state changes.
/// </summary>
public class AuthenticationStateChangedEventArgs : EventArgs
{
    public bool IsAuthenticated { get; init; }
    public IUserPrincipal? User { get; init; }
    public string? Reason { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}