using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Wholesale.ReviewWholesaleAccount;

public record ReviewWholesaleAccountCommand(
    Guid WholesaleAccountId,
    WholesaleStatus NewStatus,
    string ReviewedByUserId,
    string? RejectionReason) : IRequest;

public class ReviewWholesaleAccountCommandValidator : AbstractValidator<ReviewWholesaleAccountCommand>
{
    public ReviewWholesaleAccountCommandValidator()
    {
        RuleFor(x => x.WholesaleAccountId).NotEmpty();
        RuleFor(x => x.ReviewedByUserId).NotEmpty();
    }
}

public class ReviewWholesaleAccountCommandHandler(IApplicationDbContext db, IWholesaleRoleAssigner roleAssigner, INotificationService notificationService)
    : IRequestHandler<ReviewWholesaleAccountCommand>
{
    public async Task Handle(ReviewWholesaleAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await db.WholesaleAccounts.FirstOrDefaultAsync(w => w.Id == request.WholesaleAccountId, cancellationToken)
            ?? throw new KeyNotFoundException("Wholesale account not found");

        account.Status = request.NewStatus;
        account.RejectionReason = request.NewStatus == WholesaleStatus.Rejected ? request.RejectionReason : null;
        account.ReviewedAt = DateTime.UtcNow;
        account.ReviewedByUserId = request.ReviewedByUserId;

        await db.SaveChangesAsync(cancellationToken);

        // Grant or revoke the Identity role based on the new status — this is the ONLY
        // place the Wholesale role is ever assigned, keeping approval and access in lockstep.
        if (request.NewStatus == WholesaleStatus.Approved)
            await roleAssigner.AddToWholesaleRoleAsync(account.UserId, cancellationToken);
        else
            await roleAssigner.RemoveFromWholesaleRoleAsync(account.UserId, cancellationToken);
        
        var statusText = request.NewStatus switch
        {
            WholesaleStatus.Approved => ("Compte grossiste approuvé", "Wholesale account approved"),
            WholesaleStatus.Rejected => ("Compte grossiste rejeté", "Wholesale account rejected"),
            WholesaleStatus.Suspended => ("Compte grossiste suspendu", "Wholesale account suspended"),
            _ => ("Statut du compte mis à jour", "Account status updated")
        };

        await notificationService.NotifyAsync(
            account.UserId, statusText.Item1, statusText.Item2,
            request.NewStatus == WholesaleStatus.Rejected && request.RejectionReason is not null
                ? request.RejectionReason : "",
            request.NewStatus == WholesaleStatus.Rejected && request.RejectionReason is not null
                ? request.RejectionReason : "",
            "/grossistes", cancellationToken);
    }
}