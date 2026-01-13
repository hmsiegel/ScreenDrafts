namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Restore;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request>
{
  public override void Configure()
  {
    Post(CampaignRoutes.Restore);
    Description(b => b
      .WithName(DraftsOpenApi.Names.Campaigns_RestoreCampaign)
      .WithTags(DraftsOpenApi.Tags.Campaigns)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound));
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var command = new Command(req.PublicId);

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

internal sealed record Request
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}

internal sealed record Command(string PublicId) : Common.Features.Abstractions.Messaging.ICommand;

internal sealed class Validator : AbstractValidator<Command>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .WithMessage("PublicId is required.");
  }
}

internal sealed class CommandHandler(ICampaignsRepository campaignsRepository)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command>
{
  private readonly ICampaignsRepository _campaignsRepository = campaignsRepository;

  public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
  {
    var campaign = await _campaignsRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (campaign is null)
    {
      return Result.Failure(CampaignErrors.NotFound(request.PublicId));
    }

    var restoreResult = campaign.Restore();

    if (restoreResult.IsFailure)
    {
      return Result.Failure(restoreResult.Errors);
    }

    _campaignsRepository.Update(campaign);

    return Result.Success();
  }
}
