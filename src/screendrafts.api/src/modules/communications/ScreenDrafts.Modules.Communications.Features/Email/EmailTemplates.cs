namespace ScreenDrafts.Modules.Communications.Features.Email;

internal static class EmailTemplates
{
  public static string HostAdded(
        string recipientName,
        string draftName,
        IReadOnlyList<string> coHostNames)
  {
    var coHostsHtml = coHostNames.Count > 0
        ? $"<p>You'll be co-hosting with: <strong>{string.Join(", ", coHostNames)}</strong></p>"
        : "<p>You are the sole host for this draft.</p>";

    return
        $"""
            {BaseLayout(
            recipientName,
            $"You've been added as a host in <strong>Screen Drafts</strong>.",
            $"""
                <p>You will be a host for the <strong>{Encode(draftName)}</strong></p> draft.
                {coHostsHtml}
                """)}
            """;
  }

  public static string ParticipantAdded(
      string recipientName,
      string draftName,
      IReadOnlyList<string> coParticipantNames)
  {
    var coParticipantsHtml = coParticipantNames.Count > 0
        ? $"<p>Other participants: <strong>{string.Join(", ", coParticipantNames.Select(Encode))}</strong></p>"
        : string.Empty;

    return
        $"""
            {BaseLayout(
            recipientName,
            $"You've been added to <strong>Screen Drafts</strong>",
            $"""
                <p>You will be drafting in the <strong>{Encode(draftName)} draft.</p> 
                {coParticipantsHtml}
                """)}
            """;
  }

  public static string CoParticipantJoined(
    string recipientName,
    string newParticipantName,
    string draftName,
    IReadOnlyList<string> allParticipantNames)
  {
    var participantsHtml = allParticipantNames.Count > 0
        ? $"<p>Current participants: <strong>{string.Join(", ", allParticipantNames.Select(Encode))}</strong></p>"
        : string.Empty;

    return BaseLayout(
        recipientName,
        $"<strong>{Encode(newParticipantName)}</strong> has joined your draft",
        $"""
            <p>Draft: <strong>{Encode(draftName)}</strong></p>
            {participantsHtml}
            """);
  }

  public static string DraftCreated(
    string recipientName,
    string draftName,
    bool isPatreon)
  {
    var patreonBadge = isPatreon
        ? "<p style=\"display:inline-block;background:#f59e0b;color:#fff;padding:2px 10px;border-radius:4px;font-size:12px;font-weight:bold;margin-bottom:16px;\">PATREON EXCLUSIVE</p>"
        : string.Empty;

    return BaseLayout(
        recipientName,
        $"A new draft has been announced: <strong>{Encode(draftName)}</strong>",
        $"""
        {patreonBadge}
        <p>A new ScreenDrafts episode is in the works. Stay tuned!</p>
        """);
  }

  public static string DraftCompleted(
      string recipientName,
      string draftName,
      bool isPatreon)
  {
    var patreonBadge = isPatreon
        ? "<p style=\"display:inline-block;background:#f59e0b;color:#fff;padding:2px 10px;border-radius:4px;font-size:12px;font-weight:bold;margin-bottom:16px;\">PATREON EXCLUSIVE</p>"
        : string.Empty;

    return BaseLayout(
        recipientName,
        $"<strong>{Encode(draftName)}</strong> is now available!",
        $"""
        {patreonBadge}
        <p>The draft has been completed and published. Go check out the results!</p>
        """);
  }

  private static string BaseLayout(string recipientName, string headline, string body) =>
      $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
          <title>ScreenDrafts</title>
        </head>
        <body style="margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f4f4;padding:32px 0;">
            <tr>
              <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:8px;overflow:hidden;">
                  <tr>
                    <td style="background:#1a1a2e;padding:24px 32px;">
                      <h1 style="margin:0;color:#ffffff;font-size:24px;letter-spacing:1px;">ScreenDrafts</h1>
                    </td>
                  </tr>
                  <tr>
                    <td style="padding:32px;">
                      <p style="margin:0 0 16px;font-size:16px;color:#333;">Hi {Encode(recipientName)},</p>
                      <p style="margin:0 0 24px;font-size:18px;color:#1a1a2e;">{headline}</p>
                      <div style="font-size:15px;color:#555;line-height:1.6;">
                        {body}
                      </div>
                    </td>
                  </tr>
                  <tr>
                    <td style="background:#f4f4f4;padding:16px 32px;text-align:center;">
                      <p style="margin:0;font-size:12px;color:#999;">
                        You received this because you were added to a ScreenDrafts draft.<br />
                        &copy; {DateTime.UtcNow.Year} ScreenDrafts
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;

  private static string Encode(string value) =>
      System.Net.WebUtility.HtmlEncode(value);
}
