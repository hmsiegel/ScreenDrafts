namespace ScreenDrafts.Modules.Drafts.Features.People.List;

internal sealed record ListPeopleRequest
{
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 10;

  public string? Name { get; init; }
  public bool? HasDrafter { get; init; }
  public bool? HasHost { get; init; }

  public string Sort { get; init; } = "display_name";
  public string Dir { get; init; } = "asc";
}

