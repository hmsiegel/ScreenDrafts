namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

using ScreenDrafts.Common.Application.Messaging;
using ScreenDrafts.Common.Features.Abstractions.Messaging;
using ScreenDrafts.Common.Features.Abstractions.Services;

internal sealed class CommandHandler(IDraftsRepository draftsRepository, IPublicIdGenerator publicIdGenerator)
  : ICommandHandler<Command, string>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
  {
    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Draft);
    var seriesId = SeriesId.Create(request.SeriesId);

    var series = await _draftsRepository.GetSeriesByIdAsync(seriesId, cancellationToken);

    if (series is null)
    {
      return Result.Failure<string>(DraftErrors.SeriesNotFound(request.SeriesId));
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
      var partResult = draft.AddPart(
        1,
        request.TotalPicks,
        request.TotalDrafters,
        request.TotalDrafterTeams,
        request.TotalHosts);

      if (partResult.IsFailure)
      {
        return Result.Failure<string>(partResult.Error!);
      }
    }

    _draftsRepository.Add(draft);
    return draft.PublicId;
  }
}
