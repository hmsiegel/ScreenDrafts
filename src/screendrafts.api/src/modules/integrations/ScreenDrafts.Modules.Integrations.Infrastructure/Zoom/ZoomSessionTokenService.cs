namespace ScreenDrafts.Modules.Integrations.Infrastructure.Zoom;

/// <summary>
/// Generates Zoom Video SDK JWT tokens for participants joining a session.
/// 
/// The Video SDK JWT structure differs from standard JWTs:
///   - app_key:    Video SDK key (not the OAuth client ID)
///   - version:    must be 1
///   - user_identity: any string identifying the participant (we use PublicId)
///   - session_name: the session participants join (stored on DraftPart)
///   - role_type:  0 = participant, 1 = host
///   - tpc:        same as session_name (Zoom legacy field, must match)
///
/// Tokens are short-lived (2 hours) — the frontend must request a fresh token
/// if a session runs longer than that. For the Legends draft, the frontend
/// should refresh before expiry.
/// </summary>
internal sealed class ZoomSessionTokenService(
  IOptions<ZoomSettings> settings,
  IDateTimeProvider timeProvider)
  : IZoomSessionTokenService
{
  private const int TokenLifetimeSeconds = 7200; // 2 hours
  private readonly ZoomSettings _settings = settings.Value;
  private readonly IDateTimeProvider _timeProvider = timeProvider;
  public string GenerateToken(
    string userIdentity,
    string sessionName,
    ZoomSessionRole role)
  {
    var now = _timeProvider.UtcTimeZoneNow;
    var iat = now.ToUnixTimeSeconds();
    var exp = now.AddSeconds(TokenLifetimeSeconds).ToUnixTimeSeconds();

    var claims = new[]
    {
      new Claim("app_key", _settings.VideoSdkKey),
      new Claim("version", "1"),
      new Claim("user_identity", userIdentity),
      new Claim("session_name", sessionName),
      new Claim("role_type", ((int)role).ToString(CultureInfo.InvariantCulture)),
      new Claim("tpc", sessionName),
      new Claim(JwtRegisteredClaimNames.Iat, iat.ToString(CultureInfo.InvariantCulture)),
      new Claim(JwtRegisteredClaimNames.Exp, exp.ToString(CultureInfo.InvariantCulture)),
    };

    var keyBytes = Encoding.UTF8.GetBytes(_settings.VideoSdkSecret);
    var signingCredentials = new SigningCredentials(
      new SymmetricSecurityKey(keyBytes),
      SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
      claims: claims,
      signingCredentials: signingCredentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}
