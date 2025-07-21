namespace ScreenDrafts.Modules.Users.Application.Users.Queries.GetUser;

internal sealed class GetUserQueryHandler(IDbConnectionFactory dbConnectionFactory) 
  : IQueryHandler<GetUserQuery, UserResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<UserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var query = $"""
      SELECT
        id As {nameof(UserResponse.UserId)},
        email As {nameof(UserResponse.Email)},
        first_name As {nameof(UserResponse.FirstName)},
        middle_name As {nameof(UserResponse.MiddleName)},
        last_name As {nameof(UserResponse.LastName)}
      FROM users.users
      WHERE id = @UserId
      """;

    var user = await connection.QuerySingleOrDefaultAsync<UserResponse>(query, request);

    if (user is null)
    {
      return Result.Failure<UserResponse>(UserErrors.NotFound(request.UserId));
    }

    return user;
  }
}
