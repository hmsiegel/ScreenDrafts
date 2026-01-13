namespace ScreenDrafts.Modules.Drafts.Infrastructure.Converters;

internal static class IdConverters
{
  public static ValueConverter<DraftId, Guid> DraftIdConverter =>
    new(
      v => v.Value,
      v => DraftId.CreateUnique());

  public static ValueConverter<DrafterId, Guid> DrafterIdConverter =>
    new(
      v => v.Value,
      v => DrafterId.CreateUnique());

  public static ValueConverter<DrafterTeamId, Guid> DrafterTeamIdConverter =>
    new(
      v => v.Value,
      v => DrafterTeamId.CreateUnique());

  public static ValueConverter<HostId, Guid> HostIdConverter =>
    new(
      v => v.Value,
      v => HostId.CreateUnique());

  public static ValueConverter<DraftPartId, Guid> DraftPartIdConverter =>
    new(
      v => v.Value,
      v => DraftPartId.CreateUnique());

  public static ValueConverter<PickId, Guid> DraftPickIdConverter =>
    new(
      v => v.Value,
      v => PickId.CreateUnique());

  public static ValueConverter<VetoId, Guid> VetoIdConverter =>
    new(
      v => v.Value,
      v => VetoId.CreateUnique());

  public static ValueConverter<VetoOverrideId, Guid> VetoOverrideIdConverter =>
    new(
      v => v.Value,
      v => VetoOverrideId.CreateUnique());

  public static ValueConverter<RolloverVetoId, Guid> RolloverVetoIdConverter =>
    new(
      v => v.Value,
      v => RolloverVetoId.CreateUnique());

  public static ValueConverter<RolloverVetoOverrideId, Guid> RolloverVetoOverrideIdConverter =>
    new(
      v => v.Value,
      v => RolloverVetoOverrideId.CreateUnique());
}
