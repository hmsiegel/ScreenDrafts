namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed class Validator : AbstractValidator<Query>
{
  public Validator()
  {
    RuleFor(x => x.Search)
      .NotEmpty().WithMessage("Search query must not be empty.")
      .MinimumLength(2).WithMessage("Search query must be at least 2 characters long.")
      .MaximumLength(100).WithMessage("Search query must not exceed 100 characters.");

    RuleFor(x => x.Limit)
      .GreaterThan(0).WithMessage("Limit must be greater than 0.")
      .LessThanOrEqualTo(50).WithMessage("Limit must not exceed 50.");
  }
}
