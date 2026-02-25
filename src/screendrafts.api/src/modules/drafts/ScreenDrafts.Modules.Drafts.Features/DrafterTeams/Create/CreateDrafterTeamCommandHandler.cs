namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Create;

internal sealed class CreateDrafterTeamCommandHandler(
  IDrafterTeamRepository drafterTeamRepository,
  IPublicIdGenerator publicIdGenerator)
  : ICommandHandler<CreateDrafterTeamCommand, string>
{
  private readonly IDrafterTeamRepository _drafterTeamRepository = drafterTeamRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(CreateDrafterTeamCommand request, CancellationToken cancellationToken)
  {
    var exists = await _drafterTeamRepository.ExistsByNameAsync(request.Name, cancellationToken);

    if (exists)
    {
      return Result.Failure<string>(DrafterTeamErrors.NameInUse(request.Name));
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DrafterTeam);

    var result = DrafterTeam.Create(request.Name, publicId);

    if (result.IsFailure)
    {
      return Result.Failure<string>(result.Error!);
    }

    var drafterTeam = result.Value;

    _drafterTeamRepository.Add(drafterTeam);

    return Result.Success(drafterTeam.PublicId);
  }
}
