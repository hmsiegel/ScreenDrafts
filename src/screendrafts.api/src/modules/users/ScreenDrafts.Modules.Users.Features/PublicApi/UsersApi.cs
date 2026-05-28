namespace ScreenDrafts.Modules.Users.Features.PublicApi;

using ScreenDrafts.Modules.Users.Features.Users.GetByUserId;
using ScreenDrafts.Modules.Users.Features.Users.ListUsers;
using UserResponse = UserResponse;

internal sealed class UsersApi(ISender sender) : IUsersApi
{
  private readonly ISender _sender = sender;

  public async Task<IReadOnlyList<UserResponse>> GetAllUsersAsync(
    string? search,
    CancellationToken cancellationToken
  )
  {
    var query = new ListUsersQuery { Search = search };

    var result = await _sender.Send(query, cancellationToken: cancellationToken);

    if (result.IsFailure)
    {
      return [];
    }

    return
    [
      .. result.Value.Users.Select(r => new UserResponse
      {
        UserId = r.UserId,
        PublicId = r.PublicId,
        FirstName = r.FirstName,
        LastName = r.LastName,
        MiddleName = r.MiddleName,
        Email = r.Email,
        IdentityId = r.IdentityId,
      }),
    ];
  }

  public async Task<UserResponse?> GetUserById(Guid userId, CancellationToken cancellationToken)
  {
    var query = new GetByUserIdQuery(userId);

    var result = await _sender.Send(query, cancellationToken: cancellationToken);

    if (result.IsFailure)
    {
      return null;
    }

    return Map(result.Value);
  }

  public async Task<UserResponse?> GetUserByPublicId(
    string publicId,
    CancellationToken cancellationToken
  )
  {
    var query = new GetByPublicIdQuery(publicId);

    var result = await _sender.Send(query, cancellationToken: cancellationToken);

    if (result.IsFailure)
    {
      return null;
    }

    return Map(result.Value);
  }

  private static UserResponse Map(Users.GetByUserId.GetByUserIdResponse r) =>
    new()
    {
      UserId = r.UserId,
      PublicId = r.PublicId,
      FirstName = r.FirstName,
      LastName = r.LastName,
      MiddleName = r.MiddleName,
      Email = r.Email,
      IdentityId = r.IdentityId,
    };

  private static UserResponse Map(GetByPublicIdResponse r) =>
    new()
    {
      UserId = r.UserId,
      PublicId = r.PublicId,
      FirstName = r.FirstName,
      LastName = r.LastName,
      MiddleName = r.MiddleName,
      Email = r.Email,
      IdentityId = r.IdentityId,
    };
}
