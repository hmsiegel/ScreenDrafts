namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Hosts.RemoveHost;

internal sealed record RemoveHostDraftPartCommand : ICommand
{
  public string DraftPartId { get; init; } = default!;
  public string HostId { get; init; } = default!;
}


