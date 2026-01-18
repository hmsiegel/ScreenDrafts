namespace ScreenDrafts.Modules.Drafts.Features.People.List;

internal sealed class Validator : AbstractValidator<Request>
{
  private static readonly HashSet<string> _allowedSorts =
  [
    "first_name",
    "last_name",
    "display_name",
    "public_id"
  ];

  private static readonly HashSet<string> _allowedDirs =
  [
    "asc",
    "desc"
  ];

  public Validator()
  {
    RuleFor(x => x.Page)
      .GreaterThan(0);

    RuleFor(x => x.PageSize)
      .InclusiveBetween(1, 100);

    RuleFor(x => x.Sort)
      .Must(sort => _allowedSorts.Contains(sort))
      .WithMessage($"Sort must be one of the following: {string.Join(", ", _allowedSorts)}.");

    RuleFor(x => x.Dir)
      .Must(dir => _allowedDirs.Contains(dir))
      .WithMessage($"Dir must be one of the following: {string.Join(", ", _allowedDirs)}.");

    RuleFor(x => x.Name)
      .MaximumLength(100)
      .When(x => !string.IsNullOrWhiteSpace(x.Name));
  }
}
