namespace ScreenDrafts.Modules.Users.Features.Admin.AddRoleToUser;

internal sealed class AddRoleToUserCommandHandler(
    IDbConnectionFactory connectionFactory,
    IUserRepository userRepository)
    : ICommandHandler<AddRoleToUserCommand, bool>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IUserRepository _userRepository = userRepository;
  public async Task<Result<bool>> Handle(AddRoleToUserCommand request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var userId = UserId.Create(request.UserId);

    var user = await _userRepository.GetAsync(userId, cancellationToken);

    if (user is null)
    {
      return Result.Failure<bool>(UserErrors.NotFound(request.UserId));
    }

    if (user.Roles.Any(role => role.Name == request.Role))
    {
      return Result.Failure<bool>(UserErrors.RoleAlreadyExists(request.UserId, request.Role));
    }

    const string sql =
        $"""
            INSERT INTO users.user_roles (user_id, role_name)
            VALUES (@UserId, @Role)
            """;
    await connection.ExecuteAsync(sql, request);

    return Result.Success(true);
  }
}
