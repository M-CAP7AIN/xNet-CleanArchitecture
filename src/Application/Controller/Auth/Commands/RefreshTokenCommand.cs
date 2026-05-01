using Domain.Entities;
using Domain.Interfaces;
using Domain.Results.Auth;
using Domain.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Application.Controller.Auth.Commands
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResult>;



    public class RefreshTokenCommandHandler(UserManager<ApplicationUser> userManager,
        ITokenService tokenService, IOptions<JwtSettings> jwtSettings) :
        IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
    {

        public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var storedToken = await tokenService.GetRefreshTokenAsync(request.RefreshToken);

            if (storedToken == null)
            {
                return new RefreshTokenResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid or expired refresh token" }
                };
            }

            var user = await userManager.FindByIdAsync(storedToken.UserId.ToString());

            if (user == null)
            {
                return new RefreshTokenResult
                {
                    Success = false,
                    Errors = new List<string> { "User not found" }
                };
            }

            // باطل کردن توکن قدیمی
            await tokenService.RevokeRefreshTokenAsync(request.RefreshToken);

            // تولید توکن‌های جدید
            var roles = await userManager.GetRolesAsync(user);
            var newAccessToken = tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpiryDays);

            await tokenService.SaveRefreshTokenAsync(user.Id, newRefreshToken, refreshTokenExpiry);

            var expiresIn = Convert.ToInt32(TimeSpan.FromMinutes(jwtSettings.Value.AccessTokenExpiryMinutes).TotalSeconds);

            return new RefreshTokenResult
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = expiresIn
            };
        }
    }

}
