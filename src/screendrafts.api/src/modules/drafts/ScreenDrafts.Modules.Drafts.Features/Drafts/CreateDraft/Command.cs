namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

using ScreenDrafts.Common.Features.Abstractions.Messaging;

public sealed record Command : ICommand<string>
{
  public required string Title { get; init; }
  public required int DraftType { get; init; }
  public required Guid SeriesId { get; init; }
  public required int TotalPicks { get; init; }
  public required int TotalDrafters { get; init; }
  public required int TotalDrafterTeams { get; init; }
  public required int TotalHosts { get; init; }
  public bool AutoCreateFirstPart { get; init; } = true;
}

