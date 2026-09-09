namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Converters;

internal static class IdConverters
{
  public static ValueConverter<DraftId, Guid> DraftIdConverter =>
    new(v => v.Value, v => DraftId.Create(v));

  public static ValueConverter<DraftParticipantId, Guid> DraftParticipantIdConverter =>
    new(v => v.Value, v => DraftParticipantId.Create(v));

  public static ValueConverter<DraftParticipantId?, Guid?> NullableDraftParticipantIdConverter =>
    new(
      v => v == null ? null : v.Value,
      v => v == null ? null : DraftParticipantId.Create(v.Value)
    );

  public static ValueConverter<GameBoardId, Guid> GameBoardIdConverter =>
    new(v => v.Value, v => GameBoardId.Create(v));

  public static ValueConverter<DraftPositionId, Guid> DraftPositionIdConverter =>
    new(v => v.Value, v => DraftPositionId.Create(v));

  public static ValueConverter<PickId, Guid> PickIdConverter =>
    new(v => v.Value, v => PickId.Create(v));

  public static ValueConverter<VetoId, Guid> VetoIdConverter =>
    new(v => v.Value, v => VetoId.Create(v));

  public static ValueConverter<VetoOverrideId, Guid> VetoOverrideIdConverter =>
    new(v => v.Value, v => VetoOverrideId.Create(v));

  public static ValueConverter<CommissionerOverrideId, Guid> CommissionerOverrideIdConverter =>
    new(v => v.Value, v => CommissionerOverrideId.Create(v));

  public static ValueConverter<DrafterId, Guid> DrafterIdConverter =>
    new(v => v.Value, v => DrafterId.Create(v));

  public static ValueConverter<DrafterTeamId, Guid> DrafterTeamIdConverter =>
    new(v => v.Value, v => DrafterTeamId.Create(v));
}
