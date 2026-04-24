using ScreenDrafts.Modules.Drafts.Domain.Predictions.Enums;

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

  public static ValueConverter<PredictionMode, int> PredictionModeConverter =>
    new(
      v => v.Value,
      v => PredictionMode.FromValue(v));

  public static ValueConverter<PredictionSourceKind, int> PredictionSourceKindConverter =>
    new(
      v => v.Value,
      value => PredictionSourceKind.FromValue(value));

  public static ValueConverter<MergePolicy, int> MergePolicyConverter =>
    new(
      v => v.Value,
      v => MergePolicy.FromValue(v));

  public static ValueConverter<ZoomRecordingFileType, int> ZoomRecordingFileTypeConverter =>
    new(
      v => v.Value,
      v => ZoomRecordingFileType.FromValue(v));
}
