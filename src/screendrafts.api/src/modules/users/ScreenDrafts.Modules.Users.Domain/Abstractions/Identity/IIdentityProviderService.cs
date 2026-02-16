using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Users.Domain.Abstractions.Identity;

public interface IIdentityProviderService
{
  Task<Result<string>> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default);
}
