namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Doubles;

/// <summary>
/// Returns deterministic fake tokens so that Zoom session commands work in
/// integration tests without real Video SDK credentials.
/// </summary>
internal sealed class FakeZoomSessionTokenService : IZoomSessionTokenService
{
  public string GenerateToken(string userIdentity, string sessionName, ZoomSessionRole role)
    => $"fake-{role.ToString().ToLowerInvariant()}-token-{userIdentity}";
}
