using FluentValidation;
using YetAnotherJira.Models.V1.Requests;

namespace YetAnotherJira.Validators;

public class V1UpdateTicketRequestValidator : AbstractValidator<V1UpdateTicketRequest>
{
    public V1UpdateTicketRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.Title != null)
            .WithMessage("Title must not be empty and cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description != null)
            .WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.Author)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.Author != null)
            .WithMessage("Author must not be empty and cannot exceed 100 characters");

        RuleFor(x => x.Assignee)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.Assignee != null)
            .WithMessage("Assignee must not be empty and cannot exceed 100 characters");

        RuleFor(x => x.Parent)
            .GreaterThan(0)
            .When(x => x.Parent.HasValue)
            .WithMessage("Parent ID must be greater than 0");
    }
}
