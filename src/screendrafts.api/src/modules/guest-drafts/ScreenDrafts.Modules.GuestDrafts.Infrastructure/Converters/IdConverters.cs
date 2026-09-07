using ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafters;
using ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafterTeams;

namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Converters;

internal static class IdConverters
{
  public static ValueConverter<GuestDraftId, Guid> GuestDraftIdConverter =>
    new(v => v.Value, v => GuestDraftId.Create(v));

  public static ValueConverter<GuestDraftParticipantId, Guid> GuestDraftParticipantIdConverter =>
    new(v => v.Value, v => GuestDraftParticipantId.Create(v));

  public static ValueConverter<
    GuestDraftParticipantId?,
    Guid?
  > NullableGuestDraftParticipantIdConverter =>
    new(
      v => v == null ? null : v.Value,
      v => v == null ? null : GuestDraftParticipantId.Create(v.Value)
    );

  public static ValueConverter<GuestDraftGameBoardId, Guid> GuestDraftGameBoardIdConverter =>
    new(v => v.Value, v => GuestDraftGameBoardId.Create(v));

  public static ValueConverter<GuestDraftPositionId, Guid> GuestDraftPositionIdConverter =>
    new(v => v.Value, v => GuestDraftPositionId.Create(v));

  public static ValueConverter<GuestDraftPickId, Guid> GuestDraftPickIdConverter =>
    new(v => v.Value, v => GuestDraftPickId.Create(v));

  public static ValueConverter<GuestDraftVetoId, Guid> GuestDraftVetoIdConverter =>
    new(v => v.Value, v => GuestDraftVetoId.Create(v));

  public static ValueConverter<GuestDraftVetoOverrideId, Guid> GuestDraftVetoOverrideIdConverter =>
    new(v => v.Value, v => GuestDraftVetoOverrideId.Create(v));

  public static ValueConverter<
    GuestDraftCommissionerOverrideId,
    Guid
  > GuestDraftCommissionerOverrideIdConverter =>
    new(v => v.Value, v => GuestDraftCommissionerOverrideId.Create(v));

  public static ValueConverter<GuestDrafterId, Guid> GuestDrafterIdConverter =>
    new(v => v.Value, v => GuestDrafterId.Create(v));

  public static ValueConverter<GuestDrafterTeamId, Guid> GuestDrafterTeamIdConverter =>
    new(v => v.Value, v => GuestDrafterTeamId.Create(v));
}
