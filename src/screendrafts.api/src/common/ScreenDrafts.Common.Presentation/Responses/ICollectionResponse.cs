namespace ScreenDrafts.Common.Presentation.Responses;

public interface ICollectionResponse<T>
{
  IReadOnlyList<T> Items { get; init; }
  int TotalCount { get; init; }
}
