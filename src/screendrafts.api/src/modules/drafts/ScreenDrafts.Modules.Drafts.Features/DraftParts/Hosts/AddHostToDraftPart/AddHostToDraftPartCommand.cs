namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Hosts.AddHostToDraftPart;

internal sealed record AddHostToDraftPartCommand : ICommand
{
  public string DraftPartId { get; init; } = default!;
  public string HostPublicId { get; init; } = default!;
  public HostRole HostRole { get; init; } = default!;
}
