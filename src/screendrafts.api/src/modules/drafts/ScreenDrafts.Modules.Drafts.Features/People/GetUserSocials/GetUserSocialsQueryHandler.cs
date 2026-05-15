namespace ScreenDrafts.Modules.Drafts.Features.People.GetUserSocials;

internal sealed class GetUserSocialsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetUserSocialsQuery, GetUserSocialsResponse?>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetUserSocialsResponse?>> Handle(
    GetUserSocialsQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var dbConnection = await _dbConnectionFactory.OpenConnectionAsync(
      cancellationToken
    );

    const string sql = $"""
      select
        p.twitter_handle as {nameof(GetUserSocialsResponse.Twitter)},
        p.instagram_handle as {nameof(GetUserSocialsResponse.Instagram)},
        p.letterboxd_handle as {nameof(GetUserSocialsResponse.Letterboxd)},
        p.bluesky_handle as {nameof(GetUserSocialsResponse.Bluesky)},
        p.profile_picture_path as {nameof(GetUserSocialsResponse.ProfilePicturePath)}
      from
        drafts.people p
      where
        p.public_id = @PublicId
      """;

    var result = await dbConnection.QuerySingleOrDefaultAsync<GetUserSocialsResponse>(
      sql,
      new { request.PublicId }
    );

    if (result is null)
    {
      return Result.Failure<GetUserSocialsResponse?>(DrafterErrors.NotFound(request.PublicId));
    }

    return result;
  }
}
