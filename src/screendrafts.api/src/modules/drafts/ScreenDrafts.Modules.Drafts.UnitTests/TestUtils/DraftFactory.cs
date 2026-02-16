namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class DraftFactory
{
  private static readonly Faker _faker = new();

  public static Draft CreateStandardDraft()
  {
    var series = CreateSeries();
    return Draft.Create(
      Title.Create(_faker.Lorem.Word()),
      _faker.Random.AlphaNumeric(10),
      DraftType.Standard,
      series).Value;
  }

  public static Draft CreateMiniMegaDraft()
  {
    var series = CreateSeries();
    return Draft.Create(
      Title.Create(_faker.Lorem.Word()),
      _faker.Random.AlphaNumeric(10),
      DraftType.MiniMega,
      series).Value;
  }

  public static Draft CreateMegaDraft()
  {
    var series = CreateSeries();
    return Draft.Create(
      Title.Create(_faker.Lorem.Word()),
      _faker.Random.AlphaNumeric(10),
      DraftType.Mega,
      series).Value;
  }

  private static Series CreateSeries()
  {
    return Series.Create(
      name: _faker.Lorem.Word(),
      publicId: _faker.Random.AlphaNumeric(10),
      canonicalPolicy: CanonicalPolicy.Always,
      continuityScope: ContinuityScope.Global,
      continuityDateRule: ContinuityDateRule.AnyChannelFirstRelease,
      kind: SeriesKind.Regular,
      defaultDraftType: DraftType.Standard,
      allowedDraftTypes: DraftTypeMask.All).Value;
  }
}
