using Domain.Entities;
using Domain.Interfaces;
using Domain.Results.Auth;
using Domain.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;


namespace Application.Controller.Auth.Commands
{
    public record LoginCommand(
            string Email,
            string Password) : IRequest<AuthResult>;


    public class LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings
        ) : IRequestHandler<LoginCommand, AuthResult>
    {
        public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            
            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid email or password" }
                };
            }

            if (!user.IsActive)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Account is deactivated" }
                };
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);

            if (result.IsLockedOut)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Account is locked. Try again later" }
                };
            }

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid email or password" }
                };
            }

            // بروزرسانی آخرین زمان ورود
            user.LastLoginAt = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

            // تولید توکن‌ها
            var roles = await userManager.GetRolesAsync(user);
            var accessToken = tokenService.GenerateAccessToken(user, roles);
            var refreshToken = tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpiryDays);

            await tokenService.SaveRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

            var expiresIn = Convert.ToInt32(TimeSpan.FromMinutes(jwtSettings.Value.AccessTokenExpiryMinutes).TotalSeconds);

            return new AuthResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}"
            };
        }
    }
}
