namespace ScreenDrafts.Modules.Administration.Features.PublicApi;

internal sealed class AdministrationApi(ISender sender) : IAdministrationApi
{
  private readonly ISender _sender = sender;

  public async Task<IReadOnlyCollection<string>> GetUserRolesAsync(string publicId, CancellationToken cancellationToken = default)
  {
    var query = new GetUserRolesQuery
    {
      PublicId = publicId
    };

    var result = await _sender.Send(query, cancellationToken);

    return result.IsFailure
      ? []
      : [.. result.Value.Roles];
  }
}
