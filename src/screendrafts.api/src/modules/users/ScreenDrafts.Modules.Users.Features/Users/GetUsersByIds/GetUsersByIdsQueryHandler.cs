namespace ScreenDrafts.Modules.Users.Features.Users.GetUsersByIds;

internal sealed class GetUsersByIdsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetUsersByIdsQuery, GetUsersByIdsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetUsersByIdsResponse>> Handle(
    GetUsersByIdsQuery request,
    CancellationToken cancellationToken
  )
  {
    if (request.UserIds.Count == 0)
    {
      return Result.Success(new GetUsersByIdsResponse());
    }

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string query = $"""
      SELECT
        u.id as {nameof(GetByUserIdResponse.UserId)},
        u.public_id As {nameof(GetByUserIdResponse.PublicId)},
        u.email as {nameof(GetByUserIdResponse.Email)},
        u.first_name As {nameof(GetByUserIdResponse.FirstName)},
        u.middle_name As {nameof(GetByUserIdResponse.MiddleName)},
        u.last_name As {nameof(GetByUserIdResponse.LastName)},
        u.identity_id As {nameof(GetByUserIdResponse.IdentityId)}
      FROM users.users u
      WHERE u.id = ANY(@UserIds)
      """;

    var users = await connection.QueryAsync<GetByUserIdResponse>(
      new CommandDefinition(query, new { request.UserIds }, cancellationToken: cancellationToken)
    );

    return Result.Success(new GetUsersByIdsResponse { Users = [.. users] });
  }
}
