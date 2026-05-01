using Domain.Entities;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Results.Auth;
using Domain.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;


namespace Application.Controller.Auth.Commands
{
    public record RegisterCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password) : IRequest<AuthResult>;

    public class RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings
        /*IEmailService emailService*/) : IRequestHandler<RegisterCommand, AuthResult>
    {

        public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // بررسی وجود ایمیل تکراری
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Email already exists" }
                };
            }

            // ایجاد کاربر جدید
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // ایجاد نقش "User" اگر وجود نداشت
            string userRole = UserRoleExtensions.GetDisplayName(Domain.Enums.UserRole.User);
            if (!await roleManager.RoleExistsAsync(UserRoleExtensions.GetDisplayName(Domain.Enums.UserRole.User)))
            {
                await roleManager.CreateAsync(new ApplicationRole(userRole));
            }

            await userManager.AddToRoleAsync(user, userRole);

            // ارسال ایمیل خوش‌آمدگویی (در پس‌زمینه)
            //_ = Task.Run(() => _emailService.SendEmailAsync(
            //    user.Email!,
            //    "Welcome to Notes API",
            //    $"<h1>Welcome {user.FirstName}!</h1><p>Thanks for joining.</p>"), cancellationToken);

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
