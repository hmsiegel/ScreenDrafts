namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed record SearchPeopleResponse
{
  public required string PublicId { get; init; }
  public required string FirstName { get; init; }
  public required string LastName { get; init; }
  public string? DisplayName { get; init; }
  public bool IsDrafter { get; init; }
  public bool IsHost { get; init;  }
}

