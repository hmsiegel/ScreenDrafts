namespace ScreenDrafts.Modules.Users.Features.Users.GetUser;

internal sealed class GetUserQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetUserQuery, GetUserResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetUserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string query = $"""
      SELECT
        u.public_id As {nameof(GetUserResponse.PublicId)},
        u.email As {nameof(GetUserResponse.Email)},
        u.first_name As {nameof(GetUserResponse.FirstName)},
        u.middle_name As {nameof(GetUserResponse.MiddleName)},
        u.last_name As {nameof(GetUserResponse.LastName)},
        u.profile_picture_path As {nameof(GetUserResponse.ProfilePicturePath)},
        u.twitter_handle As {nameof(GetUserResponse.TwitterHandle)},
        u.instagram_handle As {nameof(GetUserResponse.InstagramHandle)},
        u.letterboxd_handle As {nameof(GetUserResponse.LetterboxdHandle)},
        u.bluesky_handle As {nameof(GetUserResponse.BlueskyHandle)}
      FROM users.users u
      WHERE public_id = @PublicId
      """;

    var user = await connection.QuerySingleOrDefaultAsync<GetUserResponse>(new CommandDefinition(
      query,
      new { request.PublicId },
      cancellationToken: cancellationToken));

    if (user is null)
    {
      return Result.Failure<GetUserResponse>(UserErrors.NotFound(request.PublicId));
    }

    return Result.Success(user);
  }
}
