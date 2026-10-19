namespace ScreenDrafts.Modules.Communications.Infrastructure.ContactMessages;

internal sealed class TurnstileOptions
{
  public const string SectionName = "Communications:Turnstile";

  /// <summary>
  /// Turnstile secret key. Vault in dev, Docker secret in prod
  /// (Communications__Turnstile__SecretKey) — never appsettings. In dev, Cloudflare's
  /// documented always-pass test secret works.
  /// </summary>
  public string? SecretKey { get; set; }

  /// <summary>
  /// Comma-separated hostnames a valid token may come from, for example
  /// "screen-drafts.com,www.screen-drafts.com". Empty skips the check (dev: Cloudflare's
  /// test keys report "example.com"). Set it in production appsettings.
  /// </summary>
  public string? AllowedHostnames { get; set; }

  public string VerifyUrl { get; set; } =
    "https://challenges.cloudflare.com/turnstile/v0/siteverify";
}
