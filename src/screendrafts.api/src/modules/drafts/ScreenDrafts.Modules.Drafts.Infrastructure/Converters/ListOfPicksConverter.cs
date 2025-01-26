namespace ScreenDrafts.Modules.Drafts.Infrastructure.Converters;

public class ListOfPicksConverter : ValueConverter<ICollection<int>, string>
{
    public ListOfPicksConverter(ConverterMappingHints? mappingHints = null)
      : base(
        v => string.Join(',', v),
        v => new Collection<int>(v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()),
        mappingHints)
    {
    }
}

public class ListOfPicksComparer : ValueComparer<ICollection<int>>
{
  public ListOfPicksComparer()
    : base(
      (v1, v2) => v1!.SequenceEqual(v2!),
      v => v.Select(x => x.GetHashCode()).Aggregate((x, y) => x ^ y),
      v => new Collection<int>(v.ToList()))
  {
  }
}
