namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Abstractions;

/// <summary>Deterministic IPublicIdGenerator: "{prefix}_{counter padded to 15}".</summary>
public sealed class SequentialPublicIdGenerator : IPublicIdGenerator
{
  private int _counter;

  private readonly List<string> _prefixes = [];

  public IReadOnlyList<string> Prefixes => _prefixes;

  public string GeneratePublicId(string prefix)
  {
    _prefixes.Add(prefix);
    return $"{prefix}_{Interlocked.Increment(ref _counter):D15}";
  }
}
