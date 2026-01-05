namespace ScreenDrafts.Modules.Users.Application.Users.Commands.AddUserRole;

internal sealed class AddUserRoleCommandHandler(
  IDbConnectionFactory connectionFactory,
  IUserRepository userRepository) 
  : ICommandHandler<AddUserRoleCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<Result<bool>> Handle(AddUserRoleCommand request, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var user = await _userRepository.GetAsync(UserId.Create(request.UserId), cancellationToken);

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
