namespace ScreenDrafts.Common.Infrastructure.Services;

internal sealed class NanoPublicIdGenerator : IPublicIdGenerator
{
  private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
  private const int Size = 15;
  public string GeneratePublicId(string prefix)
  {
    return $"{prefix}_{Nanoid.Generate(Alphabet, Size)}";
  }
}
