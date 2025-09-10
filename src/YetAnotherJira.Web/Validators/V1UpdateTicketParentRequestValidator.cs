using FluentValidation;
using YetAnotherJira.Models.V1.Requests;

namespace YetAnotherJira.Validators;

public class V1UpdateTicketParentRequestValidator : AbstractValidator<V1UpdateTicketParentRequest>
{
    public V1UpdateTicketParentRequestValidator()
    {
        RuleFor(x => x.Parent)
            .GreaterThan(0)
            .When(x => x.Parent.HasValue)
            .WithMessage("Parent ID must be greater than 0");
    }
}
