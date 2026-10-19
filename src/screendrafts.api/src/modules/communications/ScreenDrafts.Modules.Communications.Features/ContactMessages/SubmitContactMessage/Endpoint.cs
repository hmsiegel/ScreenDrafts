namespace ScreenDrafts.Modules.Communications.Features.ContactMessages.SubmitContactMessage;

internal sealed class Endpoint : ScreenDraftsEndpoint<SubmitContactMessageRequest>
{
  public override void Configure()
  {
    Post(CommunicationsRoutes.Contact);
    Description(x =>
    {
      x.WithTags(CommunicationsOpenApi.Tags.Contact)
        .WithName(CommunicationsOpenApi.Names.Contact_SubmitMessage)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status429TooManyRequests);
    });
    AllowAnonymous();
    Options(x => x.RequireRateLimiting(CommunicationsRateLimits.Contact));
  }

  public override async Task HandleAsync(SubmitContactMessageRequest req, CancellationToken ct)
  {
    // Honeypot: real users never see this field. Pretend success so bots learn nothing.
    if (!string.IsNullOrWhiteSpace(req.Website))
    {
      await this.SendNoContentAsync(Result.Success(), ct);
      return;
    }

    var command = new SubmitContactMessageCommand
    {
      Name = req.Name,
      Email = req.Email,
      Category = req.Category,
      Message = req.Message,
      TurnstileToken = req.TurnstileToken,
      UserPublicId = User.Identity?.IsAuthenticated == true ? User.GetUserPublicId() : null,
      RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
