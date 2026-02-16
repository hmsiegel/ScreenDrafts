namespace ScreenDrafts.Modules.Users.Features.Users.GetByPublicId;

internal sealed class GetByPublicIdQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetByPublicIdQuery, GetByPublicIdResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetByPublicIdResponse>> Handle(GetByPublicIdQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string query = $"""
      SELECT
        u.public_id As {nameof(GetByPublicIdResponse.UserId)},
        u.email as {nameof(GetByPublicIdResponse.Email)},
        u.first_name As {nameof(GetByPublicIdResponse.FirstName)},
        u.middle_name As {nameof(GetByPublicIdResponse.MiddleName)},
        u.last_name As {nameof(GetByPublicIdResponse.LastName)},
      FROM users.users u
      WHERE id = @UserId
      """;

    var user = await connection.QuerySingleOrDefaultAsync<GetByPublicIdResponse>(new CommandDefinition(
      query,
      new { request.PublicId },
      cancellationToken: cancellationToken));

    if (user is null)
    {
      return Result.Failure<GetByPublicIdResponse>(UserErrors.PublicIdNotFound(request.PublicId));
    }

    return Result.Success(user);
  }
}
