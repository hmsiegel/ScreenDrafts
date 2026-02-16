namespace ScreenDrafts.Common.Presentation.Http.Authorization;

internal sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
  public string Permission { get; } = permission;
}
