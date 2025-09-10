using FluentValidation;
using YetAnotherJira.Application.Enums;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Models.V1.Requests;

namespace YetAnotherJira.Validators;

public class V1GetTicketsRequestValidator : AbstractValidator<V1GetTicketsRequest>
{
    public V1GetTicketsRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.SortBy)
            .IsEnumName(typeof(SortBy), caseSensitive: false)
            .When(x => !string.IsNullOrEmpty(x.SortBy))
            .WithMessage("SortBy must be a valid sort field");

        RuleFor(x => x.SortOrder)
            .IsEnumName(typeof(SortOrder), caseSensitive: false)
            .When(x => !string.IsNullOrEmpty(x.SortOrder))
            .WithMessage("SortOrder must be 'Asc' or 'Desc'");

        RuleForEach(x => x.Statuses)
            .IsEnumName(typeof(TicketStatus), caseSensitive: false)
            .When(x => x.Statuses != null)
            .WithMessage("Each status must be a valid ticket status");

        RuleForEach(x => x.Priorities)
            .IsEnumName(typeof(TicketPriority), caseSensitive: false)
            .When(x => x.Priorities != null)
            .WithMessage("Each priority must be a valid ticket priority");

        RuleForEach(x => x.Authors)
            .NotEmpty()
            .When(x => x.Authors != null)
            .WithMessage("Author names cannot be empty");

        RuleForEach(x => x.Assignees)
            .NotEmpty()
            .When(x => x.Assignees != null)
            .WithMessage("Assignee names cannot be empty");
    }
}
