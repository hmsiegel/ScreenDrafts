namespace ScreenDrafts.Modules.Users.Features.PublicApi;

using UserResponse = UserResponse;

internal sealed class UsersApi(ISender sender) : IUsersApi
{
  private readonly ISender _sender = sender;

  public async Task<UserResponse?> GetUserById(
    Guid userId,
    CancellationToken cancellationToken)
  {
    var query = new Users.GetByUserId.GetByUserIdQuery(userId);

    var result = await _sender.Send(query, cancellationToken: cancellationToken);

    if (result.IsFailure)
    {
      return null;
    }

    return new UserResponse
    {
      UserId = result.Value.UserId,
      FirstName = result.Value.FirstName,
      LastName = result.Value.LastName,
      MiddleName = result.Value.MiddleName
    };
  }

  public async Task<UserSocialResponse?> GetUserSocialsAsync(string publicId, CancellationToken cancellationToken)
  {
    var query = new Users.GetUserSocials.GetUserSocialsQuery(publicId);

    var result = await _sender.Send(query, cancellationToken);

    if (result.IsFailure)
    {
      return null;
    }

    return new UserSocialResponse(
      PublicId: publicId,
      Twitter: result.Value?.Twitter,
      Instagram: result.Value?.Instagram,
      Letterboxd: result.Value?.Letterboxd,
      Bluesky: result.Value?.Bluesky,
      ProfilePicturePath: result.Value?.ProfilePicturePath);
  }

  public async Task<UserResponse?> GetUserByPublicId(string publicId, CancellationToken cancellationToken)
  {
    var query = new GetByPublicIdQuery(publicId);

    var result = await _sender.Send(query, cancellationToken: cancellationToken);

    if (result.IsFailure)
    {
      return null;
    }

    return new UserResponse
    {
      UserId = result.Value.UserId,
      FirstName = result.Value.FirstName,
      LastName = result.Value.LastName,
      MiddleName = result.Value.MiddleName
    };
  }
}
