namespace ScreenDrafts.Modules.Drafts.Features.Participants.List;

internal sealed record ListParticipantsQuery : IQuery<ListParticipantsResponse>
{
  public string? Q { get; init; }
  public string? Role { get; init; }
  public string? Retired { get; init; }
  public string? Sort { get; init; }
  public int? HonorificValue { get; init; }
  public bool IncludePatreon { get; init; }
  public int? Page { get; init; } = 1;
  public int? PageSize { get; init; } = 24;
}
