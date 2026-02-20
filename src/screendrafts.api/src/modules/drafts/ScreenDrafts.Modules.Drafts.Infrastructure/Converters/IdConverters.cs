using ScreenDrafts.Modules.Drafts.Domain.DrafterTeams;
using ScreenDrafts.Modules.Drafts.Domain.Participants;

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

  public static ValueConverter<ParticipantKind, int> ParticipantKindConverter =>
    new(
      v => v.Value,
      v => ParticipantKind.FromValue(v));

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
}
