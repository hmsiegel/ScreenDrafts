namespace ScreenDrafts.Common.IntegrationTests.Abstractions;

public static class TestFakerProvider
{
  public static Faker Faker => _faker.Value;
  private static readonly Lazy<Faker> _faker = new(() => new Faker());
}
