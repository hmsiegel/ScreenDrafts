namespace ScreenDrafts.Modules.Drafts.Infrastructure.Converters;

public static class FluentApiExtensions
{
  public static PropertyBuilder<T> HasListOfPicksConverter<T>(this PropertyBuilder<T> propertyBuilder)
  {
    ArgumentNullException.ThrowIfNull(propertyBuilder);

    return propertyBuilder.HasConversion(
      new ListOfPicksConverter(),
      new ListOfPicksComparer());
  }
}
