namespace ScreenDrafts.Modules.Drafts.Features.Series.Create;

internal sealed record Command : Common.Features.Abstractions.Messaging.ICommand<Guid>
{
  public required string Name { get; init; }
  public int Kind { get; init; }
  public int CanonicalPolicy { get; init; }
  public int ContinuityScope { get; init; }
  public int ContinuityDateRule { get; init; }
  public int AllowedDraftTypes { get; init; }
  public int? DefaultDraftType { get; init; }
}


