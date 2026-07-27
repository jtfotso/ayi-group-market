using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Entities;
using FluentValidation;
using MediatR;

namespace AYIGroupMarket.Application.Features.Wholesale.RegisterWholesaleAccount;

public record RegisterWholesaleAccountCommand(
    string UserId,
    string CompanyName,
    string ContactPerson,
    string Phone,
    string Email,
    string BusinessAddress,
    string City,
    string? BusinessRegistrationInfo,
    string ExpectedOrderVolume) : IRequest<Guid>;

public class RegisterWholesaleAccountCommandValidator : AbstractValidator<RegisterWholesaleAccountCommand>
{
    public RegisterWholesaleAccountCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactPerson).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.BusinessAddress).NotEmpty().MaximumLength(300);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
    }
}

public class RegisterWholesaleAccountCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RegisterWholesaleAccountCommand, Guid>
{
    public async Task<Guid> Handle(RegisterWholesaleAccountCommand request, CancellationToken cancellationToken)
    {
        var account = new WholesaleAccount
        {
            UserId = request.UserId,
            CompanyName = request.CompanyName,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Email = request.Email,
            BusinessAddress = request.BusinessAddress,
            City = request.City,
            BusinessRegistrationInfo = request.BusinessRegistrationInfo,
            ExpectedOrderVolume = request.ExpectedOrderVolume
        };

        db.WholesaleAccounts.Add(account);
        await db.SaveChangesAsync(cancellationToken);
        return account.Id;
    }
}