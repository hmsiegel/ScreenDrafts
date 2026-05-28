using ScreenDrafts.Common.Presentation.Responses;

namespace ScreenDrafts.Modules.Administration.Features.Users.ListUsers;

internal sealed class ListUsersQueryHandler(
  IDbConnectionFactory connectionFactory,
  IUsersApi usersApi
) : IQueryHandler<ListUsersQuery, PagedResult<UserItem>>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result<PagedResult<UserItem>>> Handle(
    ListUsersQuery request,
    CancellationToken cancellationToken
  )
  {
    var allUsers = await _usersApi.GetAllUsersAsync(request.Search, cancellationToken);

    var totalCount = allUsers.Count;

    var pageUsers = allUsers
      .Skip((request.Page - 1) * request.PageSize)
      .Take(request.PageSize)
      .ToList();

    if (pageUsers.Count == 0)
    {
      return Result.Success(
        new PagedResult<UserItem>
        {
          Items = [],
          TotalCount = totalCount,
          Page = request.Page,
          PageSize = request.PageSize,
        }
      );
    }

    var userIds = pageUsers.Select(u => u.UserId).ToList();

    using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
      SELECT
        ur.user_id   AS {nameof(RoleRow.UserId)},
        ur.role_name AS {nameof(RoleRow.RoleName)}
      FROM administration.user_roles ur
      WHERE ur.user_id = ANY(@UserIds)
      """;

    var roleRows = await connection.QueryAsync<RoleRow>(
      new CommandDefinition(sql, new { UserIds = userIds }, cancellationToken: cancellationToken)
    );

    // Step 3: group roles by user internal Guid
    var rolesByUserId = roleRows
      .GroupBy(r => r.UserId)
      .ToDictionary(g => g.Key, g => g.Select(r => r.RoleName).ToList());

    // Step 4: assemble response
    var users = pageUsers
      .Select(u => new UserItem
      {
        PublicId = u.PublicId,
        DisplayName = $"{u.FirstName} {u.LastName}".Trim(),
        Email = u.Email,
        Roles = rolesByUserId.TryGetValue(u.UserId, out var roles) ? roles : [],
      })
      .ToList();

    return Result.Success(
      new PagedResult<UserItem>
      {
        Items = users,
        TotalCount = allUsers.Count,
        Page = request.Page,
        PageSize = request.PageSize,
      }
    );
  }

  private sealed record RoleRow(Guid UserId, string RoleName);
}
