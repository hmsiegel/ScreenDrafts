namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaPublicIdByImdbId;

internal sealed class GetMediaPublicIdByImdbIdQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetMediaPublicIdByImdbIdQuery, GetMediaPublicIdByImdbIdResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetMediaPublicIdByImdbIdResponse>> Handle(
    GetMediaPublicIdByImdbIdQuery request,
    CancellationToken cancellationToken
  )
  {
    if (!LooksLikeImdbId(request.ImdbId))
    {
      return Result.Failure<GetMediaPublicIdByImdbIdResponse>(MediaErrors.MediaNotFoundByImdbId);
    }

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT m.public_id
      FROM movies.media m
      WHERE m.imdb_id = @ImdbId
      ORDER BY m.public_id
      LIMIT 1;
      """;

    var publicId = await connection.QuerySingleOrDefaultAsync<string>(
      new CommandDefinition(sql, new { request.ImdbId }, cancellationToken: cancellationToken)
    );

    return publicId is null
      ? Result.Failure<GetMediaPublicIdByImdbIdResponse>(MediaErrors.MediaNotFoundByImdbId)
      : Result.Success(new GetMediaPublicIdByImdbIdResponse { PublicId = publicId });
  }

  // "tt" followed by 7 to 10 digits. Rejects anything else without a database call.
  private static bool LooksLikeImdbId(string value) =>
    value.Length is >= 9 and <= 12
    && value.StartsWith("tt", StringComparison.Ordinal)
    && value.AsSpan(2).IndexOfAnyExceptInRange('0', '9') < 0;
}
