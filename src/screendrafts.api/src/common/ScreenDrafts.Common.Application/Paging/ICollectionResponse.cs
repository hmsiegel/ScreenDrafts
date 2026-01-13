namespace ScreenDrafts.Common.Application.Paging;

public interface ICollectionResponse<T>
{
  Collection<T> Items { get; init; }
  int TotalCount { get; init; }
}
