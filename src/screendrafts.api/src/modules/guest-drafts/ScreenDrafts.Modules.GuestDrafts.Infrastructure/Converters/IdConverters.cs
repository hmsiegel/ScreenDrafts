namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Converters;

internal static class IdConverters
{
  public static ValueConverter<GuestDraftId, Guid> GuestDraftIdConverter =>
    new(
      convertToProviderExpression: v => v.Value,
      convertFromProviderExpression: v => GuestDraftId.Create(v)
    );

  public static ValueConverter<GuestDraftParticipantId, Guid> GuestDraftParticipantIdConverter =>
    new(
      convertToProviderExpression: v => v.Value,
      convertFromProviderExpression: v => GuestDraftParticipantId.Create(v)
    );
}
