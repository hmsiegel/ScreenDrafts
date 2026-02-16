namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed class CreateDraftCommandHandler(IDraftRepository draftsRepository, IPublicIdGenerator publicIdGenerator, ISeriesRepository seriesRepository)
  : ICommandHandler<CreateDraftCommand, string>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;
  private readonly ISeriesRepository _seriesRepository = seriesRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(CreateDraftCommand request, CancellationToken cancellationToken)
  {
    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Draft);
    var seriesId = SeriesId.Create(request.SeriesId);

    var series = await _seriesRepository.GetByIdAsync(seriesId, cancellationToken);

    if (series is null)
    {
      return Result.Failure<string>(SeriesErrors.SeriesNotFound(request.SeriesId));
    }

    var result = Draft.Create(
      title: new Title(request.Title),
      publicId: publicId,
      draftType: DraftType.FromValue(request.DraftType),
      series: series!);

    if (result.IsFailure)
    {
      return await Task.FromResult(Result.Failure<string>(result.Error!));
    }

    var draft = result.Value;

    if (request.AutoCreateFirstPart)
    {
      var partResult = draft.AddPart(1, request.MinPosition, request.MaxPosition);

      if (partResult.IsFailure)
      {
        return Result.Failure<string>(partResult.Error!);
      }
    }

    _draftsRepository.Add(draft);
    return draft.PublicId;
  }
}



