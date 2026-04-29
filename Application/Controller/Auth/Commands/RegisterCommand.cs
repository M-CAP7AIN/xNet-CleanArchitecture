using Domain.Entities;
using Domain.Interfaces;
using Domain.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Controller.Auth.Commands
{
    public record RegisterCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password) : IRequest<RegisterDto>;

    public class RegisterDto
    {
        public bool Success { get; set; }

        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; } 
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public List<string>? Errors { get; set; }
    }


    public class RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings) : IRequestHandler<RegisterCommand, RegisterDto>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private readonly ITokenService _tokenService = tokenService;

        public async Task<RegisterDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return new RegisterDto
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // بررسی و ایجاد نقش "User"
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new ApplicationRole("User", "Standard user role"));
            }

            await _userManager.AddToRoleAsync(user, "User");

            // ذخیره RefreshToken
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            var expiresIn = jwtSettings.Value.AccessTokenExpiryMinutes;

            await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

            return new RegisterDto
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
