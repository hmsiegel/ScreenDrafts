namespace ScreenDrafts.Modules.Administration.PublicApi;

public interface IAdministrationApi
{
  Task<IReadOnlyCollection<string>> GetUserRolesAsync(string publicId, CancellationToken cancellationToken = default);
}
