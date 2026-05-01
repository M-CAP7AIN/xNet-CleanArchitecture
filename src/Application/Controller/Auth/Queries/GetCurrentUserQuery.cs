using Domain.Entities;
using Domain.Results.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Controller.Auth.Queries
{
    public record GetCurrentUserQuery(Guid UserId) : IRequest<CurrentUserResult?>;


    public class GetCurrentUserQueryHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<GetCurrentUserQuery, CurrentUserResult?>
    {

        public async Task<CurrentUserResult?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return null;

            var roles = await userManager.GetRolesAsync(user);

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
