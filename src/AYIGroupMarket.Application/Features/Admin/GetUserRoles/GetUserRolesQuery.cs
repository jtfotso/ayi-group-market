using AYIGroupMarket.Application.Abstractions;
using MediatR;

namespace AYIGroupMarket.Application.Features.Admin.GetUserRoles;

public record GetUserRolesQuery(string UserId) : IRequest<List<string>>;

public class GetUserRolesQueryHandler(IApplicationDbContext db) : IRequestHandler<GetUserRolesQuery, List<string>>
{
    public Task<List<string>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        => db.GetUserRolesAsync(request.UserId, cancellationToken);
}

public record GetAllRolesQuery : IRequest<List<string>>;

public class GetAllRolesQueryHandler(IApplicationDbContext db) : IRequestHandler<GetAllRolesQuery, List<string>>
{
    public Task<List<string>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        => db.GetAllRoleNamesAsync(cancellationToken);
}