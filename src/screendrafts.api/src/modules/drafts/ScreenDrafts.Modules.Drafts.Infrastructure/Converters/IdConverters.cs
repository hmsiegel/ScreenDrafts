namespace ScreenDrafts.Modules.Drafts.Infrastructure.Converters;

internal static class IdConverters
{
  public static ValueConverter<DraftId, Guid> DraftIdConverter =>
    new(
      v => v.Value,
      v => DraftId.Create(v));

  public static ValueConverter<DrafterId, Guid> DrafterIdConverter =>
    new(
      v => v.Value,
      v => DrafterId.Create(v));

  public static ValueConverter<DrafterTeamId, Guid> DrafterTeamIdConverter =>
    new(
      v => v.Value,
      v => DrafterTeamId.Create(v));

  public static ValueConverter<HostId, Guid> HostIdConverter =>
    new(
      v => v.Value,
      v => HostId.Create(v));

  public static ValueConverter<DraftPartId, Guid> DraftPartIdConverter =>
    new(
      v => v.Value,
      v => DraftPartId.Create(v));

  public static ValueConverter<DraftPartId?, Guid?> NullableDraftPartIdConverter =>
    new(
      v => v == null ? null : v.Value,
      v => v == null ? null : DraftPartId.Create(v.Value));

  public static ValueConverter<PickId, Guid> DraftPickIdConverter =>
    new(
      v => v.Value,
      v => PickId.Create(v));

  public static ValueConverter<VetoId, Guid> VetoIdConverter =>
    new(
      v => v.Value,
      v => VetoId.Create(v));

  public static ValueConverter<VetoOverrideId, Guid> VetoOverrideIdConverter =>
    new(
      v => v.Value,
      v => VetoOverrideId.Create(v));

  public static ValueConverter<CategoryId, Guid> CategoryIdConverter =>
    new(
      v => v.Value,
      v => CategoryId.Create(v));

  public static ValueConverter<SeriesId, Guid> SeriesIdConverter =>
    new(
      v => v.Value,
      v => SeriesId.Create(v));

  public static ValueConverter<GameBoardId, Guid> GameBoardIdConverter =>
    new(
      v => v.Value,
      v => GameBoardId.Create(v));

  public static ValueConverter<TriviaResultId, Guid> TriviaResultsIdConverter =>
    new(
      v => v.Value,
      v => TriviaResultId.Create(v));

  public static ValueConverter<DraftPartParticipantId, Guid> DraftPartParticipantIdConverter =>
    new(
      v => v.Value,
      v => DraftPartParticipantId.Create(v));

  public static ValueConverter<PersonId, Guid> PersonIdConverter =>
    new(
      v => v.Value,
      v => PersonId.Create(v));

  public static ValueConverter<DraftPositionId, Guid> DraftPositionIdConverter
    => new(
      v => v.Value,
      v => DraftPositionId.Create(v));

  public static ValueConverter<DraftBoardId, Guid> DraftBoardIdConverter =>
    new(
      v => v.Value,
      value => DraftBoardId.Create(value));

  public static ValueConverter<DraftPoolId, Guid> DraftPoolIdConverter =>
    new(
      v => v.Value,
      value => DraftPoolId.Create(value));

  public static ValueConverter<CandidateListEntryId, Guid> CandidateListEntryIdConverter =>
    new(
      v => v.Value,
      value => CandidateListEntryId.Create(value));

  public static ValueConverter<SubDraftId, Guid> SubDraftIdConverter =>
    new(
      v => v.Value,
      value => SubDraftId.Create(value));

  public static ValueConverter<SubDraftId?, Guid?> NullableSubDraftIdConverter =>
    new(
      v => v == null ? null : v.Value,
      v => v == null ? null : SubDraftId.Create(v.Value));
}
