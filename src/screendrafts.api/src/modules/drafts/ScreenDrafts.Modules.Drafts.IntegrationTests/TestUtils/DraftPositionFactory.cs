namespace ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

public static class DraftPositionFactory
{
  public static Collection<DraftPosition> CreateDraftPositions(int numberOfPositions, int numberOfPicks)
  {
    var draftPositions = new Collection<DraftPosition>();

    var basePicks = numberOfPicks / numberOfPositions;
    var extraPicks = numberOfPicks % numberOfPositions;

    var allPicks = Enumerable.Range(1, numberOfPicks)
      .OrderByDescending(x => x)
      .ToList();

    var positionPicks = new List<Collection<int>>();
    for (int i = 0; i < numberOfPositions; i++)
    {
      positionPicks.Add([]);
    }

    int currentIndex = 0;
    for (int i = 0; i < extraPicks; i++)
    {
      positionPicks[i].Add(allPicks[currentIndex++]);
    }

    for (var round = 0; round < basePicks; round++)
    {
      for (int pos = 0; pos < numberOfPositions; pos++)
      {
        positionPicks[pos].Add(allPicks[currentIndex++]);
      }
    }

    for (int i = 0; i < numberOfPositions; i++)
    {
      var name = $"Drafter {IntToLetter(i + 1)}";
      var result = DraftPosition.Create(name, positionPicks[i]);
      draftPositions.Add(result.Value);
    }

    return draftPositions;
  }

  private static string IntToLetter(int value)
  {
    string result = string.Empty;
    while (--value >= 0)
    {
      result = (char)('A' + value % 26) + result;
      value /= 26;
    }
    return result;
  }
}
