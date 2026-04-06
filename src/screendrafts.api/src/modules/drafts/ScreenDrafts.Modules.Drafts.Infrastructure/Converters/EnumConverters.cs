namespace ScreenDrafts.Modules.Drafts.Infrastructure.Converters;

internal static class EnumConverters
{
  public static ValueConverter<ParticipantKind, int> ParticipantKindConverter =>
    new(
      v => v.Value,
      v => ParticipantKind.FromValue(v));
  public static ValueConverter<SubjectKind?, int?> NullableSubjectKindConverter =>
    new(
      v => v == null ? null : v.Value,
      v => v == null ? null : SubjectKind.FromValue(v.Value));
  public static ValueConverter<SubDraftStatus, int> SubDraftStatusConverter =>
    new(
      v => v.Value,
      v => SubDraftStatus.FromValue(v));
}
