using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Controller.Auth.Commands
{
    public record ChangePasswordCommand(
        string CurrentPassword,
        string NewPassword) : IRequest<bool>;


    public class ChangePasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService,
        ITokenService tokenService) : IRequestHandler<ChangePasswordCommand, bool>
    {
        public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId;

            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return false;

            var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                // باطل کردن همه توکن‌ها برای امنیت بیشتر
                await tokenService.RevokeAllUserTokensAsync(userId);
            }

            return result.Succeeded;
        }
    }
}
