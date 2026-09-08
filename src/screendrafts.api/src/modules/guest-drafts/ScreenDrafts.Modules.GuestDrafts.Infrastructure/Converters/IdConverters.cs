using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;
using ScreenDrafts.Modules.GuestDrafts.Domain.DrafterTeams;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Converters;

internal static class IdConverters
{
  public static ValueConverter<DraftId, Guid> GuestDraftIdConverter =>
    new(v => v.Value, v => DraftId.Create(v));

  public static ValueConverter<DraftParticipantId, Guid> GuestDraftParticipantIdConverter =>
    new(v => v.Value, v => DraftParticipantId.Create(v));

  public static ValueConverter<
    DraftParticipantId?,
    Guid?
  > NullableGuestDraftParticipantIdConverter =>
    new(
      v => v == null ? null : v.Value,
      v => v == null ? null : DraftParticipantId.Create(v.Value)
    );

  public static ValueConverter<GameBoardId, Guid> GuestDraftGameBoardIdConverter =>
    new(v => v.Value, v => GameBoardId.Create(v));

  public static ValueConverter<DraftPositionId, Guid> GuestDraftPositionIdConverter =>
    new(v => v.Value, v => DraftPositionId.Create(v));

  public static ValueConverter<PickId, Guid> GuestDraftPickIdConverter =>
    new(v => v.Value, v => PickId.Create(v));

  public static ValueConverter<VetoId, Guid> GuestDraftVetoIdConverter =>
    new(v => v.Value, v => VetoId.Create(v));

  public static ValueConverter<VetoOverrideId, Guid> GuestDraftVetoOverrideIdConverter =>
    new(v => v.Value, v => VetoOverrideId.Create(v));

  public static ValueConverter<
    CommissionerOverrideId,
    Guid
  > GuestDraftCommissionerOverrideIdConverter =>
    new(v => v.Value, v => CommissionerOverrideId.Create(v));

  public static ValueConverter<DrafterId, Guid> GuestDrafterIdConverter =>
    new(v => v.Value, v => DrafterId.Create(v));

  public static ValueConverter<DrafterTeamId, Guid> GuestDrafterTeamIdConverter =>
    new(v => v.Value, v => DrafterTeamId.Create(v));
}
