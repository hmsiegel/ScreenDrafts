namespace ScreenDrafts.Modules.Communications.IntegrationTests.Doubles;

internal sealed class FakeTurnstileVerifier(bool result) : ITurnstileVerifier
{
  private readonly bool _result = result;

  public int CallCount { get; private set; }

  public string? LastToken { get; private set; }

  public string? LastRemoteIpAddress { get; private set; }

  public Task<bool> VerifyAsync(
    string token,
    string? remoteIpAddress,
    CancellationToken cancellationToken = default
  )
  {
    CallCount++;
    LastToken = token;
    LastRemoteIpAddress = remoteIpAddress;
    return Task.FromResult(_result);
  }
}
