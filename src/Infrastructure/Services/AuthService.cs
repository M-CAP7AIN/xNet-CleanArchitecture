
using Domain.Entities;
using Domain.Enums;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Results.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;


namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ITokenService tokenService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<AuthResult> RegisterAsync(Domain.Results.Auth.RegisterRequest request)
        {
            // بررسی وجود ایمیل تکراری
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
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

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            var userRole = UserRoleExtensions.GetDisplayName(UserRole.User);

            // ایجاد نقش "User" اگر وجود نداشت
            if (!await _roleManager.RoleExistsAsync(userRole))
            {
                await _roleManager.CreateAsync(new ApplicationRole(userRole, "Standard user role"));
            }

            // افزودن نقش به کاربر
            await _userManager.AddToRoleAsync(user, userRole);

            // ارسال ایمیل خوش‌آمدگویی
            //await _emailService.SendEmailAsync(
            //    user.Email,
            //    "Welcome to Notes API",
            //    $"<h1>Welcome {user.FirstName}!</h1><p>Thanks for joining.</p>"
            //);

            // تولید توکن‌ها
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

            return new AuthResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = Convert.ToInt32(TimeSpan.FromMinutes(15).TotalSeconds),
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}"
            };
        }

        public async Task<AuthResult> LoginAsync(Domain.Results.Auth.LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

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

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);

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
            await _userManager.UpdateAsync(user);

            // تولید توکن‌ها
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

            return new AuthResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = Convert.ToInt32(TimeSpan.FromMinutes(15).TotalSeconds),
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}"
            };
        }

        public async Task LogoutAsync(Guid userId, string refreshToken)
        {
            await _tokenService.RevokeRefreshTokenAsync(refreshToken);
        }

        public async Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _tokenService.GetRefreshTokenAsync(refreshToken);

            if (storedToken == null)
            {
                return new RefreshTokenResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid or expired refresh token" }
                };
            }

            var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());

            if (user == null)
            {
                return new RefreshTokenResult
                {
                    Success = false,
                    Errors = new List<string> { "User not found" }
                };
            }

            // باطل کردن توکن قدیمی
            await _tokenService.RevokeRefreshTokenAsync(refreshToken);

            // تولید توکن‌های جدید
            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _tokenService.SaveRefreshTokenAsync(user.Id, newRefreshToken, refreshTokenExpiry);

            return new RefreshTokenResult
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = Convert.ToInt32(TimeSpan.FromMinutes(15).TotalSeconds)
            };
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return false;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                // باطل کردن همه توکن‌ها برای امنیت بیشتر
                await _tokenService.RevokeAllUserTokensAsync(userId);
            }

            return result.Succeeded;
        }

        public async Task<CurrentUserResult?> GetCurrentUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new CurrentUserResult
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
