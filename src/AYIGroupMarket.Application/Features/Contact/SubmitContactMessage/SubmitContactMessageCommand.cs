using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Entities;
using FluentValidation;
using MediatR;

namespace AYIGroupMarket.Application.Features.Contact.SubmitContactMessage;

public record SubmitContactMessageCommand(string Name, string Email, string? Phone, string Subject, string Message) : IRequest;

public class SubmitContactMessageCommandValidator : AbstractValidator<SubmitContactMessageCommand>
{
    public SubmitContactMessageCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}

public class SubmitContactMessageCommandHandler(IApplicationDbContext db) : IRequestHandler<SubmitContactMessageCommand>
{
    public async Task Handle(SubmitContactMessageCommand request, CancellationToken cancellationToken)
    {
        db.ContactMessages.Add(new ContactMessage
        {
            Name = request.Name, Email = request.Email, Phone = request.Phone,
            Subject = request.Subject, Message = request.Message
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}