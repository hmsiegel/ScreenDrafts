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
        var draft = Draft.Create(request.Name!, (DraftType)request.DraftType, request.NumberOfDrafters);

        await _draftRepository.AddAsync(draft, cancellationToken);

        await _draftRepository.SaveChangesAsync(cancellationToken);

        return default;
    }
}
