
namespace ScreenDrafts.Modules.Drafts.Features.Drafters.List;

internal sealed class Validator : AbstractValidator<Query>
{
  private static readonly HashSet<string> _allowedSort = new(StringComparer.OrdinalIgnoreCase)
  {
    "displayName",
  };

  private static readonly HashSet<string> _allowedRetired = new(StringComparer.OrdinalIgnoreCase)
  {
    "all",
    "active",
    "retired"
  };

  private static readonly HashSet<string> _allowedDir = new(StringComparer.OrdinalIgnoreCase)
  {
    "asc",
    "desc"
  };

  public Validator()
  {
    RuleFor(x => x.Request.Page)
        .GreaterThan(0)
        .When(x => x.Request.Page.HasValue)
        .WithMessage("Page must be greater than 0.");

    RuleFor(x => x.Request.PageSize)
        .InclusiveBetween(1, 100)
        .WithMessage("Page Size must be between 1 and 100.");

    RuleFor(x => x.Request.Direction)
        .Must(v => _allowedDir.Contains(v!))
        .WithMessage("Direction must be either 'asc' or 'desc' if specified.");

    RuleFor(x => x.Request.Sort)
      .Must(v => _allowedSort.Contains(v!))
      .WithMessage("Sort must be either 'displayName' if specified.");

    RuleFor(x => x.Request.Retired)
      .Must(v => _allowedRetired.Contains(v!))
      .WithMessage("Retired must be either 'all', 'active', or 'retired' if specified.");
  }
}
