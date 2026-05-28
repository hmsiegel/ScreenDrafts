using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Administration.Domian.Abstractions;

public interface IAdministrationIdentityProviderService
{
  Task<Result> SendPasswordResetEmailAsync(
    string identityId,
    CancellationToken cancellationToken = default
  );
}
