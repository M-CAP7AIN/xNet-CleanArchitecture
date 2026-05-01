using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Domain.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(ApplicationUser user, IEnumerable<string>? roles = null);
        string GenerateRefreshToken();
        Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryDate);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token);
        Task RevokeAllUserTokensAsync(Guid userId);
        bool ValidateToken(string token);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
