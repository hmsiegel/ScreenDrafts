namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListLatestDrafts;

internal sealed class ListLatestDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListLatestDraftsQuery, ListLatestDraftsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  private const int CompletedStatus = 3;
  private const int MainFeedChannel = 0;

  public async Task<Result<ListLatestDraftsResponse>> Handle(ListLatestDraftsQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var draftsSql = new StringBuilder(
      $"""
      SELECT
        dp.public_id AS {nameof(LatestDraftResponse.DraftPartPublicId)},
        d.public_id AS {nameof(LatestDraftResponse.DraftPublicId)},
        d.title AS {nameof(LatestDraftResponse.Title)},
        dp.part_index AS {nameof(LatestDraftResponse.PartNumber)},
        dcr.episode_number AS {nameof(LatestDraftResponse.EpisodeNumber)},
        (SELECT COUNT(*) FROM drafts.draft_parts dp2 WHERE dp2.draft_id = d.id) AS {nameof(LatestDraftResponse.TotalParts)},
        MIN(r.release_date) AS {nameof(LatestDraftResponse.ReleaseDate)}
      FROM drafts.draft_parts dp
      JOIN drafts.drafts d ON dp.draft_id = d.id
      LEFT JOIN drafts.draft_channel_releases dcr ON dcr.draft_id = d.id AND dcr.release_channel = {MainFeedChannel}
      LEFT JOIN drafts.draft_releases r ON r.part_id = dp.id
      WHERE dp.status = {CompletedStatus}
      """
      );

    if (!request.IncludePatreonOnly)
    {
      draftsSql.Append(CultureInfo.InvariantCulture, $" AND r.release_channel = {MainFeedChannel} ");
    }

    draftsSql.Append(
      $"""

      GROUP BY
        dp.id, dp.public_id, dp.part_index,
        d.id, d.public_id, d.title, dcr.episode_number
      ORDER BY MIN(r.release_date) DESC NULLS LAST
      LIMIT 5
      """
      );

    IReadOnlyCollection<LatestDraftResponse> drafts = [.. await connection.QueryAsync<LatestDraftResponse>(
      sql: draftsSql.ToString(),
      param: new { CompletedStatus, MainFeedChannel })];

    if (drafts.Count == 0)
    {
      return Result.Success(new ListLatestDraftsResponse
      {
        Drafts = []
      });
    }

    var draftPartPublicIds = drafts.Select(d => d.DraftPartPublicId).ToArray();

    const string participantsSql =
      """
      SELECT
        dp.public_id AS DraftPartPublicId,
        p.participant_id_value AS ParticipantIdValue,
        p.participant_kind_value AS ParticipantKindValue,
        COALESCE(
          per.display_name,
          dt.name
        ) AS DisplayName
      FROM drafts.draft_part_participants p
      JOIN drafts.draft_parts dp ON dp.id = p.draft_part_id
      LEFT JOIN drafts.drafters dr 
        ON dr.id = p.participant_id_value AND p.participant_kind_value = 0
      LEFT JOIN  drafts.people per
        ON per.id = dr.person_id
      LEFT JOIN drafts.drafter_teams dt
        ON dt.id = p.participant_id_value AND p.participant_kind_value = 1
      WHERE dp.public_id = ANY(@DraftPartPublicIds)
      """;

    var participantRows = await connection.QueryAsync<ParticipantRow>(
      new CommandDefinition(
        participantsSql,
        new { DraftPartPublicIds = draftPartPublicIds },
      cancellationToken: cancellationToken));

    var participantsByPart = participantRows
      .GroupBy(r => r.DraftPartPublicId)
      .ToDictionary(
      g => g.Key,
      g => (IReadOnlyCollection<LatestDraftParticipantResponse>)[.. g.Select(r => new LatestDraftParticipantResponse
      {
        ParticipantIdValue = r.ParticipantIdValue,
        ParticipantKindValue = ParticipantKind.FromValue(r.ParticipantKindValue),
        DisplayName = r.DisplayName ?? ResolveCommunityDisplayName(r.ParticipantIdValue)
      })]);

    var results = drafts
      .Select(d => d with
      {
        Participants = participantsByPart.GetValueOrDefault(d.DraftPartPublicId, [])
      })
      .ToList();

    return Result.Success(new ListLatestDraftsResponse
    {
      Drafts = results
    });
  }

  private sealed record ParticipantRow(
    string DraftPartPublicId,
    Guid ParticipantIdValue,
    int ParticipantKindValue,
    string? DisplayName);

  private static string ResolveCommunityDisplayName(Guid participantIdValue) =>
    participantIdValue == CommunityParticipants.PatreonMembers.Value
      ? "Patreon Members"
      : "Community";
}
