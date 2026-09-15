namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.GenerateEmailBootstrapTokens;

internal sealed record GenerateEmailBootstrapTokensRequest
{
  /// <summary>Omit or leave empty to target every user who hasn't claimed yet.</summary>
  public List<string>? UserPublicIds { get; init; }

  /// <summary>e.g. "patreon-2026-09", "discord-2026-10" — tags this run for the audit trail.</summary>
  public string? BatchLabel { get; init; }

  public int ExpiryHours { get; init; } = 72;
}
