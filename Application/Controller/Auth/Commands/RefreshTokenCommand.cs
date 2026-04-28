using Domain.Entities;
using Domain.Interfaces;
using Domain.Settings;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Controller.Auth.Commands
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResponse>;

    public class RefreshTokenResponse
    {
        public bool Success { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public List<string>? Errors { get; set; }
    }


    public class RefreshTokenCommandHandler(UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings) 
        : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
    {

        public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var storedToken = await tokenService.GetRefreshTokenAsync(request.RefreshToken);

            if (storedToken == null)
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Errors = new List<string> { "Invalid or expired refresh token" }
                };
            }

            var user = await userManager.FindByIdAsync(storedToken.UserId.ToString());
            if (user == null)
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Errors = new List<string> { "User not found" }
                };
            }

            // Revoke old token
            await tokenService.RevokeRefreshTokenAsync(request.RefreshToken);

            // Generate new tokens
            var newAccessToken = tokenService.GenerateAccessToken(user);
            var newRefreshToken = tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            var expiresIn = jwtSettings.Value.AccessTokenExpiryMinutes;


            await tokenService.SaveRefreshTokenAsync(user.Id, newRefreshToken, refreshTokenExpiry);

            return new RefreshTokenResponse
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = expiresIn
            };
        }
    }

}
