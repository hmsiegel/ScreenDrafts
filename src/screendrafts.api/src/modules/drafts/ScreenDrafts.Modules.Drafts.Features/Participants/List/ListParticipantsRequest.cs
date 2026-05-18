namespace ScreenDrafts.Modules.Drafts.Features.Participants.List;

internal sealed record ListParticipantsRequest
{
  [FromQuery(Name = "q")]
  public string? Q { get; init; }

  [FromQuery(Name = "role")]
  public string? Role { get; init; }

  [FromQuery(Name = "retired")]
  public string? Retired { get; init; }

  [FromQuery(Name = "sort")]
  public string? Sort { get; init; }

  [FromQuery(Name = "honorific")]
  public string? Honorific { get; init; }

  [FromQuery(Name = "page")]
  public int? Page { get; init; }

  [FromQuery(Name = "pageSize")]
  public int? PageSize { get; init; }
}
