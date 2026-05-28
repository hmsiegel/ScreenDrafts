namespace ScreenDrafts.Modules.Users.Features.Users.ListUsers;

internal sealed class ListUsersQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<ListUsersQuery, ListUsersResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<ListUsersResponse>> Handle(
    ListUsersQuery request,
    CancellationToken cancellationToken
  )
  {
    using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
      SELECT
        u.id           AS {nameof(UserRow.UserId)},
        u.public_id    AS {nameof(UserRow.PublicId)},
        u.first_name   AS {nameof(UserRow.FirstName)},
        u.last_name    AS {nameof(UserRow.LastName)},
        u.middle_name  AS {nameof(UserRow.MiddleName)},
        u.email        AS {nameof(UserRow.Email)},
        u.identity_id  AS {nameof(UserRow.IdentityId)}
      FROM users.users u
      WHERE (@Search IS NULL
         OR u.first_name ILIKE '%' || @Search || '%'
         OR u.last_name  ILIKE '%' || @Search || '%'
         OR u.email      ILIKE '%' || @Search || '%')
      ORDER BY u.last_name, u.first_name
      """;

    var rows = await connection.QueryAsync<UserRow>(
      new CommandDefinition(
        sql,
        new { Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search },
        cancellationToken: cancellationToken
      )
    );

    var users = rows.Select(r => new ListUsersItem
      {
        UserId = r.UserId,
        PublicId = r.PublicId,
        FirstName = r.FirstName,
        LastName = r.LastName,
        MiddleName = r.MiddleName,
        Email = r.Email,
        IdentityId = r.IdentityId,
      })
      .ToList();

    return Result.Success(new ListUsersResponse { Users = users });
  }

  private sealed record UserRow(
    Guid UserId,
    string PublicId,
    string FirstName,
    string LastName,
    string? MiddleName,
    string Email,
    string IdentityId
  );
}
