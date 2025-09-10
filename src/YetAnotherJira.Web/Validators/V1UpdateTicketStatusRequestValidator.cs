using FluentValidation;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Models.V1.Requests;

namespace YetAnotherJira.Validators;

public class V1UpdateTicketStatusRequestValidator : AbstractValidator<V1UpdateTicketStatusRequest>
{
    public V1UpdateTicketStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .IsEnumName(typeof(TicketStatus), caseSensitive: false)
            .WithMessage("Status must be a valid ticket status (New, InProgress, Done)");
    }
}
