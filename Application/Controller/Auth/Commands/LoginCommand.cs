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
    public record LoginCommand(
            string Email,
            string Password) : IRequest<LoginResponse>;

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public int ExpiresIn { get; set; }
        public List<string>? Errors { get; set; }
    }


    public class LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings) : IRequestHandler<LoginCommand, LoginResponse>
    {


        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Errors = new List<string> { "Invalid email or password" }
                };
            }

            var result = await signInManager.PasswordSignInAsync(user, request.Password, false, true);

            if (!result.Succeeded)
            {
                return new LoginResponse
                {
                    Success = false,
                    Errors = new List<string> { "Invalid email or password" }
                };
            }

            var accessToken = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            var expiresIn = jwtSettings.Value.AccessTokenExpiryMinutes;

            await tokenService.SaveRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

            return new LoginResponse
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                ExpiresIn = expiresIn 
            };
        }
    }
}
