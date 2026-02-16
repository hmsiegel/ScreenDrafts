namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class HostCreatedDomainEvent(
  Guid hostId,
  Guid personId,
  string publicId) : DomainEvent
{
  public Guid HostId { get; init; } = hostId;
  public Guid PersonId { get; init; } = personId;
  public string PublicId { get; init; } = publicId;
}

