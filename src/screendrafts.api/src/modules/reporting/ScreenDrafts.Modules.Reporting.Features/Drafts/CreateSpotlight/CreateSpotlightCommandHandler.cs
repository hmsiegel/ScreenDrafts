namespace ScreenDrafts.Modules.Reporting.Features.Drafts.CreateSpotlight;

internal sealed class CreateSpotlightCommandHandler(
  IDbConnectionFactory connectionFactory,
  IDraftReportingRepository draftReportingRepository
) : ICommandHandler<CreateSpotlightCommand, CreateSpotlightResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IDraftReportingRepository _draftReportingRepository = draftReportingRepository;

  public async Task<Result<CreateSpotlightResponse>> Handle(
    CreateSpotlightCommand request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    // Verify that the draft exists in reporting and is complete
    // a spotlight on an imcomplete draft would show a partial pick list
    const string draftCheckSql = """
      SELECT is_complete, is_patreon
      FROM reporting.draft_summaries
      WHERE draft_public_id = @DraftPublicId
      LIMIT 1
      """;

    var draftRow = await connection.QuerySingleOrDefaultAsync<DraftCheckRow>(
      new CommandDefinition(
        commandText: draftCheckSql,
        parameters: new { request.DraftPublicId },
        cancellationToken: cancellationToken
      )
    );

    if (draftRow is null)
    {
      return Result.Failure<CreateSpotlightResponse>(
        DraftReportingErrors.NotFound(request.DraftPublicId)
      );
    }

    if (!draftRow.IsComplete)
    {
      return Result.Failure<CreateSpotlightResponse>(DraftReportingErrors.DraftNotComplete);
    }

    if (draftRow.IsPatreon)
    {
      return Result.Failure<CreateSpotlightResponse>(DraftReportingErrors.DraftIsPatreon);
    }

    var spotifyUrl = string.IsNullOrWhiteSpace(request.SpotifyUrl)
      ? string.Empty
      : request.SpotifyUrl;

    var spotlight = DraftSpotlight.Create(
      request.DraftPublicId,
      request.SpotlightDescription,
      new Uri(spotifyUrl)
    );

    _draftReportingRepository.AddSpotlight(spotlight);

    return Result.Success(new CreateSpotlightResponse { SpotlightId = spotlight.Id.Value });
  }

  private sealed record DraftCheckRow(bool IsComplete, bool IsPatreon);
}
