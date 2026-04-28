using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Controller.Auth.Queries
{
    public record GetCurrentUserQuery(string UserId) : IRequest<CurrentUserResult?>;

    public class CurrentUserResult
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }



    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResult?>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetCurrentUserQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<CurrentUserResult?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new CurrentUserResult
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            };
        }
    }
}
