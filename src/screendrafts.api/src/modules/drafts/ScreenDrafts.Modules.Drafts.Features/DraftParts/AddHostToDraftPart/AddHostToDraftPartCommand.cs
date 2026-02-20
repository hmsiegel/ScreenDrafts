using ScreenDrafts.Modules.Drafts.Domain.Hosts;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AddHostToDraftPart;

internal sealed record AddHostToDraftPartCommand : ICommand
{
  public Guid DraftPartId { get; init; }
  public string HostPublicId { get; init; } = default!;
  public HostRole HostRole { get; init; } = default!;
}
