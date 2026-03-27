using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Integration events require exposing generic lists for serialization purposes.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Integration events require exposing generic lists for serialization purposes.")]
public sealed class DraftPartStartedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId,
  string draftPartPublicId,
  Guid draftId,
  string draftPublicId,
  int partIndex,
  List<DraftPartParticipantModel> participants,
  int canonicalPolicyValue,
  bool hasMainFeedRelease)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; set; } = draftPartId;
  public string DraftPartPublicId { get; set; } = draftPartPublicId;
  public Guid DraftId { get; set; } = draftId;
  public string DraftPublicId { get; set; } = draftPublicId;
  public int PartIndex { get; set; } = partIndex;

  /// <summary>
  /// Canonical policy backing value
  /// </summary>
  public int CanonicalPolicyValue { get; set; } = canonicalPolicyValue;

  public bool HasMainFeedRelease { get; set; } = hasMainFeedRelease;

  /// <summary>
  ///  Release Channel backing value
  /// </summary>
  public List<DraftPartParticipantModel> Participants { get; set; } = participants;
}

public sealed record DraftPartParticipantModel
{
  public Guid ParticipantIdValue { get; init; }
  public int ParticipantKindValue { get; init; }
}
