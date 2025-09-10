using FluentValidation;
using YetAnotherJira.Models.V1.Requests;

namespace YetAnotherJira.Validators;

public class V1UpdateTicketTitleRequestValidator : AbstractValidator<V1UpdateTicketTitleRequest>
{
    public V1UpdateTicketTitleRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Title must not be empty and cannot exceed 200 characters");
    }
}
