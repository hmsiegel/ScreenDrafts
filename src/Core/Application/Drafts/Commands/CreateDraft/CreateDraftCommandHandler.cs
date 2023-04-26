using ScreenDrafts.Domain.DraftAggregate.Enums;

namespace ScreenDrafts.Application.Drafts.Commands.CreateDraft;
internal sealed class CreateDraftCommandHandler : IRequestHandler<CreateDraftCommand, DefaultIdType>
{
    private readonly IRepositoryWithEvents<Draft> _draftRepository;

    public CreateDraftCommandHandler(IRepositoryWithEvents<Draft> draftRepository)
    {
        _draftRepository = draftRepository;
    }

    public async Task<DefaultIdType> Handle(CreateDraftCommand request, CancellationToken cancellationToken)
    {
        var draft = Draft.Create(request.Name!, (DraftType)request.DraftType, request.EpisodeNumber);

        await _draftRepository.AddAsync(draft, cancellationToken);

        return default;
    }
}
