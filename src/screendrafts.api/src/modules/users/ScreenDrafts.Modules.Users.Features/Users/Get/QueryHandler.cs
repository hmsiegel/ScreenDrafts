namespace ScreenDrafts.Modules.Users.Features.Users.Get;

internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, Response>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string query = $"""
      SELECT
        u.public_id As {nameof(Response.PublicId)},
        u.email As {nameof(Response.Email)},
        u.first_name As {nameof(Response.FirstName)},
        u.middle_name As {nameof(Response.MiddleName)},
        u.last_name As {nameof(Response.LastName)},
        u.profile_picture_path As {nameof(Response.ProfilePicturePath)},
        u.twitter_handle As {nameof(Response.TwitterHandle)},
        u.instagram_handle As {nameof(Response.InstagramHandle)},
        u.letterboxd_handle As {nameof(Response.LetterboxdHandle)},
        u.bluesky_handle As {nameof(Response.BlueskyHandle)}
      FROM users.users u
      WHERE public_id = @PublicId
      """;

    var user = await connection.QuerySingleOrDefaultAsync<Response>(new CommandDefinition(
      query,
      new { request.PublicId },
      cancellationToken: cancellationToken));

    if (user is null)
    {
      return Result.Failure<Response>(UserErrors.NotFound(request.PublicId));
    }

    return Result.Success(user);
  }
}
