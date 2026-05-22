namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.TriviaResults.GetTriviaResults;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetTriviaResultsRequest, GetTriviaResultsResponse>
{
  public override void Configure()
  {
    Get(DraftPartRoutes.TriviaResults);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_GetTriviaResults)
        .Produces<GetTriviaResultsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetTriviaResultsRequest req, CancellationToken ct)
  {
    var query = new GetTriviaResultsQuery { DraftPartId = req.DraftPartId };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

internal sealed record GetTriviaResultsResponse
{
  public required string DraftPartId { get; init; }
  public required IReadOnlyList<TriviaResultResponse> Results { get; init; } = [];
}

internal sealed record GetTriviaResultsRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }
}

internal sealed record TriviaResultResponse
{
  public required int Position { get; init; }
  public required int QuestionsWon { get; init; }
  public required string ParticipantDisplayName { get; init; }
  public required string ParticipantKind { get; init; }
}

internal sealed record GetTriviaResultsQuery : IQuery<GetTriviaResultsResponse>
{
  public required string DraftPartId { get; init; }
}

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
        END                    AS ParticipantDisplayName
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
        };
      })
      .ToList();

    return new GetTriviaResultsResponse { DraftPartId = request.DraftPartId, Results = results };
  }

  private sealed record TriviaResultRow(
    int Position,
    int QuestionsWon,
    int ParticipantKind,
    string ParticipantDisplayName
  );
}
