namespace ScreenDrafts.Modules.Users.Application.Users.Queries.GetUserSocials;

internal sealed class GetUserSocialsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetUserSocialsQuery, SocialResponse?>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<SocialResponse?>> Handle(GetUserSocialsQuery request, CancellationToken cancellationToken)
  {
    await using var dbConnection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
      select
        u.twitter_handle as {nameof(SocialResponse.Twitter)},
        u.instagram_handle as {nameof(SocialResponse.Instagram)},
        u.letterboxd_handle as {nameof(SocialResponse.Letterboxd)},
        u.bluesky_handle as {nameof(SocialResponse.Bluesky)},
        u.profile_picture_path as {nameof(SocialResponse.ProfilePicturePath)}
      from
        users.users u
      where
        u.id = @UserId;
      """;

    var result = await dbConnection.QuerySingleOrDefaultAsync<SocialResponse>(
      sql,
      new { request.UserId });

    if (result is null)
    {
      return Result.Failure<SocialResponse?>(UserErrors.NotFound(request.UserId));
    }

    return result;
  }
}
