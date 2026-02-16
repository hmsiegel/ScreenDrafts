namespace ScreenDrafts.Modules.Users.Features.Users.GetByUserId;

internal sealed class GetByUserIdQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetByUserIdQuery, GetByUserIdResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetByUserIdResponse>> Handle(GetByUserIdQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string query = $"""
      SELECT
        u.id as {nameof(GetByUserIdResponse.UserId)},
        u.public_id As {nameof(GetByUserIdResponse.PublicId)},
        u.email as {nameof(GetByUserIdResponse.Email)},
        u.first_name As {nameof(GetByUserIdResponse.FirstName)},
        u.middle_name As {nameof(GetByUserIdResponse.MiddleName)},
        u.last_name As {nameof(GetByUserIdResponse.LastName)},
      FROM users.users u
      WHERE id = @UserId
      """;

    var user = await connection.QuerySingleOrDefaultAsync<GetByUserIdResponse>(new CommandDefinition(
      query,
      new { request.UserId },
      cancellationToken: cancellationToken));

    if (user is null)
    {
      return Result.Failure<GetByUserIdResponse>(UserErrors.NotFound(request.UserId));
    }

    return Result.Success(user);
  }
}

