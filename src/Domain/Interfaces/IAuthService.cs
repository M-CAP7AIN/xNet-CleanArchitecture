using Domain.Entities;
using Domain.Results.Auth;


namespace Domain.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task LogoutAsync(Guid userId, string refreshToken);
        Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken);
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        Task<CurrentUserResult?> GetCurrentUserAsync(Guid userId);
    }
}
