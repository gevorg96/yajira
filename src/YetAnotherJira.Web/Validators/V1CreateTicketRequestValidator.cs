using FluentValidation;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Models.V1.Requests;

namespace YetAnotherJira.Validators;

public class V1CreateTicketRequestValidator: AbstractValidator<V1CreateTicketRequest>
{
    public V1CreateTicketRequestValidator()
    {
        RuleFor(x => x.Author)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty();

        RuleFor(x => x.Parent)
            .GreaterThan(0)
            .When(x => x.Parent is not null);

        RuleFor(x => x.Priority)
            .NotEmpty()
            .IsEnumName(typeof(TicketPriority));
    }
}