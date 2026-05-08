using ScreenDrafts.Modules.Reporting.Domain.Drafts;

namespace ScreenDrafts.Modules.Reporting.Infrastructure.Converters;

internal static class IdConverters
{
  public static ValueConverter<DrafterHonorificId, Guid> DrafterHonorificIdConverter =>
    new(v => v.Value, v => DrafterHonorificId.Create(v));

  public static ValueConverter<
    DrafterCanonicalAppearanceId,
    Guid
  > DrafterCanonicalAppearanceIdConverter =>
    new(v => v.Value, v => DrafterCanonicalAppearanceId.Create(v));

  public static ValueConverter<
    DrafterHonorificHistoryId,
    Guid
  > DrafterHonorificHistoryIdConverter =>
    new(v => v.Value, v => DrafterHonorificHistoryId.Create(v));

  public static ValueConverter<MovieCanonicalPickId, Guid> MovieCanonicalPickIdConverter =>
    new(v => v.Value, v => MovieCanonicalPickId.Create(v));

  public static ValueConverter<MovieHonorificId, Guid> MovieHonorificIdConverter =>
    new(v => v.Value, v => MovieHonorificId.Create(v));

  public static ValueConverter<MovieHonorificHistoryId, Guid> MovieHonorificHistoryIdConverter =>
    new(v => v.Value, v => MovieHonorificHistoryId.Create(v));

  public static ValueConverter<DraftPartReleaseId, Guid> DraftPartReleaseIdConverter =>
    new(v => v.Value, v => DraftPartReleaseId.Create(v));

  public static ValueConverter<DraftSpotlightId, Guid> DraftSpotlightIdConverter =>
    new(v => v.Value, v => DraftSpotlightId.Create(v));

  public static ValueConverter<DraftSummaryId, Guid> DraftSummaryIdConverter =>
    new(v => v.Value, v => DraftSummaryId.Create(v));
}
