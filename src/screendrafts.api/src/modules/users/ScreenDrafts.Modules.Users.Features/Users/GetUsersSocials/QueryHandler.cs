
namespace ScreenDrafts.Modules.Users.Features.Users.GetUsersSocials;

internal sealed class QueryHandler(IDbConnectionFactory connectionFactory) : IQueryHandler<Query, Response>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
        select
          u.person_public_id as {nameof(SocialResponse.PublicId)},
          u.profile_picture_path as {nameof(SocialResponse.ProfilePicturePath)},
          u.twitter_handle as {nameof(SocialResponse.Twitter)},
          u.instagram_handle as {nameof(SocialResponse.Instagram)},
          u.letterboxd_handle as {nameof(SocialResponse.Letterboxd)},
          u.bluesky_handle as {nameof(SocialResponse.Bluesky)}
        from
          users.users u
        where
          u.person_public_id = any(@PersonIds);
      """;

    var socials = await connection.QueryAsync<SocialResponse>(new CommandDefinition(
      sql,
      new { request.PersonIds },
      cancellationToken: cancellationToken));

    var response = new Response
    {
      Socials = [.. socials]
    };

    return Result.Success(response);
  }
}
