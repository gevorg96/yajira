using FluentValidation;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Models.V1.Requests;

namespace YetAnotherJira.Validators;

public class V1UpdateTicketPriorityRequestValidator : AbstractValidator<V1UpdateTicketPriorityRequest>
{
    public V1UpdateTicketPriorityRequestValidator()
    {
        RuleFor(x => x.Priority)
            .NotEmpty()
            .IsEnumName(typeof(TicketPriority), caseSensitive: false)
            .WithMessage("Priority must be a valid ticket priority (Low, Medium, High)");
    }
}
