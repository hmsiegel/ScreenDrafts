namespace ScreenDrafts.Common.Infrastructure.Authorization;

public static class PasswordGenerator
{
  private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
  private const string Lower = "abcdefghijklmnopqrstuvwxyz";
  private const string Digits = "0123456789";
  private const string Special = "!@#$%^&*()-_=+[]{}|;:,.<>?";

  private const int DefaultLength = 12;

  public static string GeneratePassword(int length = DefaultLength)
  {
    if (length < 8)
    {
      throw new ArgumentException("Password length must be at least 8 characters.", nameof(length));
    }

    var allChars = Upper + Lower + Digits + Special;
    var password = new StringBuilder();

    using var random = RandomNumberGenerator.Create();
    void AppendRandom(string chars)
    {
      byte[] buffer = new byte[1];
      int idx;
      do
      {
        random.GetBytes(buffer);
        idx = buffer[0] % chars.Length;
      } while (buffer[0] >= chars.Length * (byte.MaxValue / chars.Length));
      password.Append(chars[idx]);
    }

    AppendRandom(Upper);
    AppendRandom(Lower);
    AppendRandom(Digits);
    AppendRandom(Special);

    for (var i = 4; i < length; i++)
    {
      AppendRandom(allChars);
    }

    return Shuffle(password.ToString(), random);
  }

  private static string Shuffle(string input, RandomNumberGenerator random)
  {
    var chars = input.ToCharArray();
    for (var i = chars.Length - 1; i > 0; i--)
    {
      byte[] buffer = new byte[1];
      random.GetBytes(buffer);
      int j = buffer[0] % (i + 1);
      (chars[i], chars[j]) = (chars[j], chars[i]); // Swap characters
    }
    return new string(chars);
  }
}
