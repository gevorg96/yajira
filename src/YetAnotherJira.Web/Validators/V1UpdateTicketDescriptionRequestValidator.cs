using FluentValidation;
using YetAnotherJira.Models.V1.Requests;

namespace YetAnotherJira.Validators;

public class V1UpdateTicketDescriptionRequestValidator : AbstractValidator<V1UpdateTicketDescriptionRequest>
{
    public V1UpdateTicketDescriptionRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("Description must not be empty and cannot exceed 2000 characters");
    }
}
