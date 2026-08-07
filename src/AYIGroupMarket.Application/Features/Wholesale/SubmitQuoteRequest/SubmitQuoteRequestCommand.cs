using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using AYIGroupMarket.Domain.Entities;
using AYIGroupMarket.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Wholesale.SubmitQuoteRequest;

public record SubmitQuoteRequestCommand(
    string UserId,
    string DeliveryLocation,
    string? Message,
    List<QuoteRequestItemInput> Items) : IRequest<Guid>;

public class SubmitQuoteRequestCommandValidator : AbstractValidator<SubmitQuoteRequestCommand>
{
    public SubmitQuoteRequestCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DeliveryLocation).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required for a quote request.");
    }
}

public class SubmitQuoteRequestCommandHandler(IApplicationDbContext db) : IRequestHandler<SubmitQuoteRequestCommand, Guid>
{
    public async Task<Guid> Handle(SubmitQuoteRequestCommand request, CancellationToken cancellationToken)
    {
        var wholesaleAccount = await db.WholesaleAccounts
            .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.Status == WholesaleStatus.Approved, cancellationToken)
            ?? throw new InvalidOperationException("Only approved wholesale accounts can request a quote.");

        var quote = new QuoteRequest
        {
            UserId = request.UserId,
            WholesaleAccountId = wholesaleAccount.Id,
            DeliveryLocation = request.DeliveryLocation,
            Message = request.Message,
            Status = QuoteStatus.Submitted
        };
        db.QuoteRequests.Add(quote);
        await db.SaveChangesAsync(cancellationToken); // ensure quote.Id exists before referencing it below

        foreach (var item in request.Items)
        {
            db.QuoteRequestItems.Add(new QuoteRequestItem
            {
                QuoteRequestId = quote.Id,
                ProductId = item.ProductId,
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return quote.Id;
    }
}