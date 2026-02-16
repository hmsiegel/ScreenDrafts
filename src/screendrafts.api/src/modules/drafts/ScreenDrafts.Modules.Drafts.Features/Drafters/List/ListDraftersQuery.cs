
namespace ScreenDrafts.Modules.Drafts.Features.Drafters.List;

internal sealed record ListDraftersQuery(ListDraftersRequest ListDraftersRequest) : IQuery<DrafterCollectionResponse>;


