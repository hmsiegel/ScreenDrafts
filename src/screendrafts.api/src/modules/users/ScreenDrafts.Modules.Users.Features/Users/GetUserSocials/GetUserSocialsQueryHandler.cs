namespace ScreenDrafts.Modules.Users.Features.Users.GetUserSocials;

internal sealed class GetUserSocialsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetUserSocialsQuery, Response?>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<Response?>> Handle(GetUserSocialsQuery request, CancellationToken cancellationToken)
  {
    await using var dbConnection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
      select
        u.twitter_handle as {nameof(Response.Twitter)},
        u.instagram_handle as {nameof(Response.Instagram)},
        u.letterboxd_handle as {nameof(Response.Letterboxd)},
        u.bluesky_handle as {nameof(Response.Bluesky)},
        u.profile_picture_path as {nameof(Response.ProfilePicturePath)}
      from
        users.users u
      where
        u.public_id = @PublicId
      """;

    var result = await dbConnection.QuerySingleOrDefaultAsync<Response>(
      sql,
      new { request.PublicId });

    if (result is null)
    {
      return Result.Failure<Response?>(UserErrors.NotFound(request.PublicId));
    }

    return result;
  }
}
