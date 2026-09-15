namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.GenerateEmailBootstrapTokens;

internal sealed record GenerateEmailBootstrapTokensCommand
  : ICommand<List<EmailBootstrapTokenResponse>>
{
  public IReadOnlyCollection<string>? UserPublicIds { get; init; }
  public string? BatchLabel { get; init; }
  public int ExpiryHours { get; init; } = 72;
}
