namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.GenerateEmailBootstrapTokens;

internal sealed class GenerateEmailBootstrapTokensCommandHandler(
  IUserRepository userRepository,
  IEmailBootstrapClaimRepository claimRepository,
  IEmailBootstrapTokenService tokenService,
  IAdministrationApi administrationApi
) : ICommandHandler<GenerateEmailBootstrapTokensCommand, List<EmailBootstrapTokenResponse>>
{
  private readonly IUserRepository _userRepository = userRepository;
  private readonly IEmailBootstrapClaimRepository _claimRepository = claimRepository;
  private readonly IEmailBootstrapTokenService _tokenService = tokenService;
  private readonly IAdministrationApi _administrationApi = administrationApi;

  private const string PatreonRole = "Patreon";

  public async Task<Result<List<EmailBootstrapTokenResponse>>> Handle(
    GenerateEmailBootstrapTokensCommand request,
    CancellationToken cancellationToken
  )
  {
    List<User> targetUsers;

    if (request.UserPublicIds is { Count: > 0 })
    {
      targetUsers = [];

      foreach (var publicId in request.UserPublicIds)
      {
        var user = await _userRepository.GetByPublicIdAsync(publicId, cancellationToken);

        if (user is null)
        {
          return Result.Failure<List<EmailBootstrapTokenResponse>>(
            UserErrors.PublicIdNotFound(publicId)
          );
        }

        targetUsers.Add(user);
      }
    }
    else
    {
      targetUsers = [.. await _userRepository.GetAllAsync(cancellationToken)];
    }

    var now = DateTimeOffset.UtcNow;
    var expiresAt = now.AddHours(request.ExpiryHours);

    var responses = new List<EmailBootstrapTokenResponse>();

    foreach (var user in targetUsers)
    {
      var existingClaim = await _claimRepository.GetByUserIdAsync(user.Id, cancellationToken);

      if (existingClaim is { IsClaimed: true })
      {
        // Already migrated — nothing to reissue, skip silently rather than
        // erroring the whole batch over one already-done user.
        continue;
      }

      if (existingClaim is null)
      {
        _claimRepository.Add(
          EmailBootstrapClaim.Issue(user.Id, now, expiresAt, request.BatchLabel)
        );
      }
      else
      {
        var reissueResult = existingClaim.Reissue(now, expiresAt, request.BatchLabel);

        if (reissueResult.IsFailure)
        {
          continue;
        }

        _claimRepository.Update(existingClaim);
      }

      var token = _tokenService.GenerateToken(user.Id, expiresAt);

      var roles = await _administrationApi.GetUserRolesAsync(user.PublicId, cancellationToken);
      var isPatreon = roles.Contains(PatreonRole, StringComparer.OrdinalIgnoreCase);

      responses.Add(
        new EmailBootstrapTokenResponse
        {
          Token = token,
          CurrentEmail = user.Email.Value!,
          ExpiresAt = expiresAt,
          FirstName = user.FirstName.Value!,
          LastName = user.LastName.Value!,
          IsPatreon = isPatreon,
          UserPublicId = user.PublicId,
        }
      );
    }

    return responses;
  }
}
