namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateDraft;

internal sealed class CreateDraftCommandHandler(IDraftsRepository draftsRepository)
  : ICommandHandler<CreateDraftCommand, Guid>
{
    private readonly IDraftsRepository _draftsRepository = draftsRepository;

    public async Task<Result<Guid>> Handle(CreateDraftCommand request, CancellationToken cancellationToken)
    {
    var result = Draft.Create(
      new Title(request.Title),
      request.DraftType,
      request.TotalPicks,
      request.TotalDrafters,
      request.TotalDrafterTeams,
      request.TotalHosts,
      request.DraftStatus);

        if (result.IsFailure)
        {
            return await Task.FromResult(Result.Failure<Guid>(result.Error!));
        }

        var draft = result.Value;

        _draftsRepository.Add(draft);
        return draft.Id.Value;
    }
}
