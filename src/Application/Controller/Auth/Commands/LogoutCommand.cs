using Domain.Interfaces;
using MediatR;

namespace Application.Controller.Auth.Commands
{
    public record LogoutCommand(string RefreshToken) : IRequest<bool>;


    public class LogoutCommandHandler(ITokenService tokenService) : IRequestHandler<LogoutCommand, bool>
    {
        public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {

            await tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
            return true;

        }
    }
}
