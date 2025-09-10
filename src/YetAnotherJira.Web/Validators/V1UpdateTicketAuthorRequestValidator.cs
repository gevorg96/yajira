using FluentValidation;
using YetAnotherJira.Models.V1.Requests;

namespace YetAnotherJira.Validators;

public class V1UpdateTicketAuthorRequestValidator : AbstractValidator<V1UpdateTicketAuthorRequest>
{
    public V1UpdateTicketAuthorRequestValidator()
    {
        RuleFor(x => x.Author)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Author must not be empty and cannot exceed 100 characters");
    }
}
