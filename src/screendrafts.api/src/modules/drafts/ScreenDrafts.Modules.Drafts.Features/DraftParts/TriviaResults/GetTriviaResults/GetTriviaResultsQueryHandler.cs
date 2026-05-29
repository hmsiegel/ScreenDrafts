namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.TriviaResults.GetTriviaResults;

internal sealed class GetTriviaResultsQueryHandler(
  IDbConnectionFactory connectionFactory,
  IDraftPartRepository draftPartRepository
) : IQueryHandler<GetTriviaResultsQuery, GetTriviaResultsResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;

  public async Task<Result<GetTriviaResultsResponse>> Handle(
    GetTriviaResultsQuery request,
    CancellationToken cancellationToken
  )
  {
    var draftPartExists = await _draftPartRepository.ExistsAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (!draftPartExists)
    {
      return Result.Failure<GetTriviaResultsResponse>(
        DraftPartErrors.NotFound(request.DraftPartId)
      );
    }

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    // Participant kind values: 0 = Drafter, 1 = DrafterTeam, 2 = Community
    const string sql = """
      SELECT
        tr.position            AS Position,
        tr.questions_won       AS QuestionsWon,
        tr.participant_kind    AS ParticipantKind,
        CASE tr.participant_kind
          WHEN @DrafterKind THEN pe.display_name
          WHEN @TeamKind THEN dt.name
          WHEN @CommunityKind THEN 'Patreon Members'
          ELSE 'Unknown'
        END                    AS ParticipantDisplayName,
        sd.index               AS SubDraftIndex
      FROM drafts.trivia_results tr
      JOIN drafts.draft_parts dp
        ON dp.id = tr.draft_part_id
      LEFT JOIN drafts.drafters dr
        ON tr.participant_kind = @DrafterKind
        AND dr.id = tr.participant_id
      LEFT JOIN drafts.people pe
        ON pe.id = dr.person_id
        AND tr.participant_kind = @DrafterKind
      LEFT JOIN drafts.drafter_teams dt
        ON tr.participant_kind = @TeamKind
        AND dt.id = tr.participant_id
      LEFT JOIN drafts.sub_drafts sd
        ON sd.id = tr.sub_draft_id
      WHERE dp.public_id = @DraftPartId
      ORDER BY tr.position ASC
      """;

    var rows = (
      await connection.QueryAsync<TriviaResultRow>(
        new CommandDefinition(
          sql,
          new
          {
            request.DraftPartId,
            DrafterKind = ParticipantKind.Drafter.Value,
            TeamKind = ParticipantKind.Team.Value,
            CommunityKind = ParticipantKind.Community.Value,
          },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var results = rows.Select(r =>
      {
        var kind = ParticipantKind.FromValue(r.ParticipantKind);
        return new TriviaResultResponse
        {
          Position = r.Position,
          QuestionsWon = r.QuestionsWon,
          ParticipantDisplayName = r.ParticipantDisplayName,
          ParticipantKind = kind.Name,
          SubDraftIndex = r.SubDraftIndex,
        };
      })
      .ToList();

    return new GetTriviaResultsResponse { DraftPartId = request.DraftPartId, Results = results };
  }

  private sealed record TriviaResultRow(
    int Position,
    int QuestionsWon,
    int ParticipantKind,
    string ParticipantDisplayName,
    int? SubDraftIndex
  );
}
