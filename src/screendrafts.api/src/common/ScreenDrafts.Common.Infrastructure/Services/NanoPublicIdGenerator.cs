namespace ScreenDrafts.Common.Infrastructure.Services;

internal sealed class NanoPublicIdGenerator : IPublicIdGenerator
{
  private const string _alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
  public string GeneratePublicId(string prefix)
  {
    return $"{prefix}_{Nanoid.Generate(_alphabet, 15)}";
  }
}
