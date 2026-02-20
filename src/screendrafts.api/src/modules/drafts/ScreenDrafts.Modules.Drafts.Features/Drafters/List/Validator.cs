
namespace ScreenDrafts.Modules.Drafts.Features.Drafters.List;

internal sealed class Validator : AbstractValidator<ListDraftersQuery>
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
    RuleFor(x => x.ListDraftersRequest.Page)
        .GreaterThan(0)
        .When(x => x.ListDraftersRequest.Page.HasValue)
        .WithMessage("Page must be greater than 0.");

    RuleFor(x => x.ListDraftersRequest.PageSize)
        .InclusiveBetween(1, 100)
        .WithMessage("Page Size must be between 1 and 100.");

    RuleFor(x => x.ListDraftersRequest.Direction)
        .Must(v => _allowedDir.Contains(v!))
        .When(x => x.ListDraftersRequest.Direction is not null)
        .WithMessage("Direction must be either 'asc' or 'desc' if specified.");

    RuleFor(x => x.ListDraftersRequest.Sort)
      .Must(v => _allowedSort.Contains(v!))
      .When(x => x.ListDraftersRequest.Sort is not null)
      .WithMessage("Sort must be either 'displayName' if specified.");

    RuleFor(x => x.ListDraftersRequest.Retired)
      .Must(v => _allowedRetired.Contains(v!))
      .When(x => x.ListDraftersRequest.Retired is not null)
      .WithMessage("Retired must be either 'all', 'active', or 'retired' if specified.");
  }
}


