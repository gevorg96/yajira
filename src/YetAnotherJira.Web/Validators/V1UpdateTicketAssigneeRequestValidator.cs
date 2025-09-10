using FluentValidation;
using YetAnotherJira.Models.V1.Requests;

namespace YetAnotherJira.Validators;

public class V1UpdateTicketAssigneeRequestValidator : AbstractValidator<V1UpdateTicketAssigneeRequest>
{
    public V1UpdateTicketAssigneeRequestValidator()
    {
        RuleFor(x => x.Assignee)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Assignee must not be empty and cannot exceed 100 characters");
    }
}
