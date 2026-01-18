namespace ScreenDrafts.Modules.Users.Features.Users.GetByUserId;

internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, Response>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string query = $"""
      SELECT
        u.public_id As {nameof(Response.UserId)},
        u.email as {nameof(Response.Email)},
        u.first_name As {nameof(Response.FirstName)},
        u.middle_name As {nameof(Response.MiddleName)},
        u.last_name As {nameof(Response.LastName)},
      FROM users.users u
      WHERE id = @UserId
      """;

    var user = await connection.QuerySingleOrDefaultAsync<Response>(new CommandDefinition(
      query,
      new { request.UserId },
      cancellationToken: cancellationToken));

    if (user is null)
    {
      return Result.Failure<Response>(UserErrors.NotFound(request.UserId));
    }

    return Result.Success(user);
  }
}

