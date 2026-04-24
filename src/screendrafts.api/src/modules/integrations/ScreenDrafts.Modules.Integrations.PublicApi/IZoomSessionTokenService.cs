namespace ScreenDrafts.Modules.Integrations.PublicApi;

public interface IZoomSessionTokenService
{
  public string GenerateToken(string userIdentity, string sessionName, ZoomSessionRole role);
}

public enum ZoomSessionRole
{
  Participant = 0,
  Host = 1,
}
