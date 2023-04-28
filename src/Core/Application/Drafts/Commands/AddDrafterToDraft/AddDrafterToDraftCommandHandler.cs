namespace ScreenDrafts.Application.Drafts.Commands.AddDraftersToDraft;
internal sealed class AddDrafterToDraftCommandHandler : IRequestHandler<AddDrafterToDraftCommand, ErrorOr<AddDrafterReponse>>
{
    private readonly IRepository<Draft> _draftRepository;
    private readonly IRepository<Drafter> _drafterRepository;

    public AddDrafterToDraftCommandHandler(
        IRepository<Draft> draftRepository,
        IRepository<Drafter> drafterRepository)
    {
        _draftRepository = draftRepository;
        _drafterRepository = drafterRepository;
    }

    public async Task<ErrorOr<AddDrafterReponse>> Handle(AddDrafterToDraftCommand request, CancellationToken cancellationToken)
    {
        var draft = await _draftRepository.GetByIdAsync(request.DraftId, cancellationToken);

        if (draft is null)
        {
            return Errors.Draft.InvalidDraftId;
        }

        var drafter = await _drafterRepository.GetByIdAsync(request.DrafterId, cancellationToken);

        if (drafter is null)
        {
            return Errors.Drafter.InvalidDrafterId;
        }

        draft.AddDrafter(DrafterId.Create(drafter.Id));

        await _draftRepository.SaveChangesAsync(cancellationToken);

        return new AddDrafterReponse(draft.Id, drafter.Id);
    }
}
