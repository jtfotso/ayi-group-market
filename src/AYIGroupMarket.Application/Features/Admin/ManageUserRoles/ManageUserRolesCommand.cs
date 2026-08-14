using AYIGroupMarket.Application.Abstractions;
using MediatR;

namespace AYIGroupMarket.Application.Features.Admin.ManageUserRoles;

public record SetUserRoleCommand(string UserId, string RoleName, bool Grant) : IRequest;

public class SetUserRoleCommandHandler(IUserRoleManager roleManager) : IRequestHandler<SetUserRoleCommand>
{
    public async Task Handle(SetUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.Grant)
            await roleManager.AddToRoleAsync(request.UserId, request.RoleName, cancellationToken);
        else
            await roleManager.RemoveFromRoleAsync(request.UserId, request.RoleName, cancellationToken);
    }
}

public record DeleteUserCommand(string UserId) : IRequest;

public class DeleteUserCommandHandler(IUserRoleManager roleManager) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await roleManager.DeleteUserAsync(request.UserId, cancellationToken);
    }
}