using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Wholesale.RespondToQuoteRequest;

public record QuoteItemPriceInput(Guid QuoteRequestItemId, decimal UnitPrice);

public record RespondToQuoteRequestCommand(
    Guid QuoteRequestId,
    List<QuoteItemPriceInput> ItemPrices) : IRequest;

public class RespondToQuoteRequestCommandValidator : AbstractValidator<RespondToQuoteRequestCommand>
{
    public RespondToQuoteRequestCommandValidator()
    {
        RuleFor(x => x.QuoteRequestId).NotEmpty();
        RuleFor(x => x.ItemPrices).NotEmpty();
    }
}

public class RespondToQuoteRequestCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RespondToQuoteRequestCommand>
{
    public async Task Handle(RespondToQuoteRequestCommand request, CancellationToken cancellationToken)
    {
        var quote = await db.QuoteRequests
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == request.QuoteRequestId, cancellationToken)
            ?? throw new KeyNotFoundException("Quote request not found");

        decimal total = 0;

        foreach (var priceInput in request.ItemPrices)
        {
            var item = quote.Items.FirstOrDefault(i => i.Id == priceInput.QuoteRequestItemId);
            if (item is null) continue;

            item.QuotedUnitPrice = priceInput.UnitPrice;
            total += priceInput.UnitPrice * item.Quantity;
        }

        quote.QuotedTotal = total;
        quote.Status = QuoteStatus.Quoted;
        quote.ExpiresAt = DateTime.UtcNow.AddDays(14);

        await db.SaveChangesAsync(cancellationToken);
    }
}