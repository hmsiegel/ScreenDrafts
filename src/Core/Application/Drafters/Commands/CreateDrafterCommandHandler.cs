namespace ScreenDrafts.Application.Drafters.Commands;
internal sealed class CreateDrafterCommandHandler : IRequestHandler<CreateDrafterCommand, Drafter>
{
    private readonly IRepositoryWithEvents<Drafter> _repository;

    public CreateDrafterCommandHandler(IRepositoryWithEvents<Drafter> repository)
    {
        _repository = repository;
    }

    public async Task<Drafter> Handle(CreateDrafterCommand request, CancellationToken cancellationToken)
    {
        var drafter = Drafter.Create(
            UserId.Create(request.UserId));

        await _repository.AddAsync(drafter, cancellationToken);

        return drafter;
    }
}
