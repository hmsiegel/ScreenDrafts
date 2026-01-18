namespace ScreenDrafts.Common.Features.Http.Responses;

public interface ICollectionResponse<T>
{
  Collection<T> Items { get; init; }
  int TotalCount { get; init; }
}
