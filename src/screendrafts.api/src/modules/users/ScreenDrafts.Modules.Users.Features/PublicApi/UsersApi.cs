namespace ScreenDrafts.Modules.Users.Features.PublicApi;

internal sealed class UsersApi(ISender sender) : IUsersApi
{
  private readonly ISender _sender = sender;

  public async Task<IReadOnlyList<UserPublicApiResponse>> GetAllUsersAsync(
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
      .. result.Value.Users.Select(r => new UserPublicApiResponse
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

  public async Task<UserPublicApiResponse?> GetUserById(
    Guid userId,
    CancellationToken cancellationToken
  )
  {
    var query = new GetByUserIdQuery(userId);

    var result = await _sender.Send(query, cancellationToken: cancellationToken);

    if (result.IsFailure)
    {
      return null;
    }

    return Map(result.Value);
  }

  public async Task<UserPublicApiResponse?> GetUserByPublicId(
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

  public async Task<IReadOnlyList<UserPublicApiResponse>> GetUsersByIds(
    IReadOnlyList<Guid> userIds,
    CancellationToken cancellationToken
  )
  {
    if (userIds.Count == 0)
    {
      return [];
    }

    var query = new GetUsersByIdsQuery(userIds);

    var result = await _sender.Send(query, cancellationToken: cancellationToken);

    if (result.IsFailure)
    {
      return [];
    }

    return [.. result.Value.Users.Select(Map)];
  }

  private static UserPublicApiResponse Map(GetByUserIdResponse r) =>
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

  private static UserPublicApiResponse Map(GetByPublicIdResponse r) =>
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
