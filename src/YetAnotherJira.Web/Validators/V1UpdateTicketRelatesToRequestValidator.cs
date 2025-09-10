using FluentValidation;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Models.V1.Requests;

namespace YetAnotherJira.Validators;

public class V1UpdateTicketRelatesToRequestValidator : AbstractValidator<V1AddTicketRelatesToRequest>
{
    public V1UpdateTicketRelatesToRequestValidator()
    {
        RuleForEach(x => x.RelatesTo)
            .GreaterThan(0)
            .When(x => x.RelatesTo != null)
            .WithMessage("Each RelatesTo ID must be greater than 0");

        RuleFor(x => x.RelatesTo)
            .Must(x => x is not { Length: > 50 })
            .WithMessage("Cannot relate to more than 50 tickets");

        RuleFor(x => x.RelationType)
            .NotEmpty()
            .WithMessage("RelationType is required")
            .IsEnumName(typeof(TicketRelationType), caseSensitive: false)
            .WithMessage($"RelationType must be one of: {string.Join(", ", Enum.GetNames<TicketRelationType>())}");
    }
}
