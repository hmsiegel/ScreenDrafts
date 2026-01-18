
namespace ScreenDrafts.Modules.Drafts.Features.Drafters.List;

internal sealed record Request
{
  public string? Q { get; init; }
  public string? Retired { get; init; }
  public int? Page { get; init; }
  public int? PageSize { get; init; }
  public string? Sort { get; init; }
  public string? Direction { get; init; }
}
