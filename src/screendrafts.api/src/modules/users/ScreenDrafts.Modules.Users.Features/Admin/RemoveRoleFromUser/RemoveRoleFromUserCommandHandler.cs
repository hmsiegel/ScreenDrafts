namespace ScreenDrafts.Modules.Users.Features.Admin.RemoveRoleFromUser;

internal sealed class RemoveRoleFromUserCommandHandler(
  IDbConnectionFactory dbConnectionFactory,
  IUserRepository userRepository)
  : ICommandHandler<RemoveRoleFromUserCommand, bool>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IUserRepository _userRepository = userRepository;
  public async Task<Result<bool>> Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var user = await _userRepository.GetAsync(UserId.Create(request.UserId), cancellationToken);

    if (user is null)
    {
      return Result.Failure<bool>(UserErrors.NotFound(request.UserId));
    }

    if (!user.Roles.Any(role => role.Name == request.Role))
    {
      return Result.Failure<bool>(UserErrors.RoleDoesNotExist);
    }

    if (user.Roles.Count == 1)
    {
      return Result.Failure<bool>(UserErrors.CannotRemoveLastRole);
    }

    const string sql =
      $"""
        DELETE FROM users.user_roles
        WHERE user_id = @UserId AND role_name = @Role
      """;

    await connection.ExecuteAsync(sql, request);

    return Result.Success(true);
  }
}
