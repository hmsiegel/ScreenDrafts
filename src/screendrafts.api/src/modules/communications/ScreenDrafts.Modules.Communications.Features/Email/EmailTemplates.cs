namespace ScreenDrafts.Modules.Communications.Features.Email;

internal static class EmailTemplates
{
  // Brand tokens — mirrors :root in global.css. Single source of truth for
  // every template below; change a color here, every email picks it up.
  private const string Navy = "#0d1430";
  private const string Red = "#cb2032";
  private const string Cream = "#fbf7ee";

  private const string HeadFont = "'Oswald', 'Arial Narrow', Arial, sans-serif";
  private const string BodyFont = "'Inter', Helvetica, Arial, sans-serif";

  private const string LogoPath = "artifacts/logo.jpg";

  // Set once at startup from configuration (Communications:PublicAssetsBaseUrl) —
  // http://localhost:5000 in dev, https://screen-drafts.com in prod. Static because
  // EmailTemplates is a pure string-formatting class with no DI; the class has no
  // other environment-dependent state, so one settable property beats threading a
  // new parameter through every public method and every call site.
  public static string AssestsBasePath { get; set; } = "https://screen-drafts.com";

  private const string DraftFooter =
    "You received this because you're part of a Screen Drafts draft.";
  private const string AccountFooter =
    "You received this because someone requested an email change on your Screen Drafts account.";

  public static string HostAdded(
    string recipientName,
    string draftName,
    IReadOnlyList<string> coHostNames
  )
  {
    ArgumentNullException.ThrowIfNull(coHostNames);
    var coHostsHtml =
      coHostNames.Count > 0
        ? $"<p style=\"margin:0 0 16px;\">You'll be co-hosting with: <strong>{string.Join(", ", coHostNames.Select(Encode))}</strong></p>"
        : "<p style=\"margin:0 0 16px;\">You are the sole host for this draft.</p>";

    return BaseLayout(
      recipientName,
      "You've been added as a host on <strong>Screen Drafts</strong>.",
      $"""
      <p style="margin:0 0 16px;">You will be a host for <strong>{Encode(draftName)}</strong>.</p>
      {coHostsHtml}
      """,
      DraftFooter
    );
  }

  public static string ParticipantAdded(
    string recipientName,
    string draftName,
    IReadOnlyList<string> coParticipantNames
  )
  {
    ArgumentNullException.ThrowIfNull(coParticipantNames);
    var coParticipantsHtml =
      coParticipantNames.Count > 0
        ? $"<p style=\"margin:0;\">Other participants: <strong>{string.Join(", ", coParticipantNames.Select(Encode))}</strong></p>"
        : string.Empty;

    return BaseLayout(
      recipientName,
      "You've been added to a draft on <strong>Screen Drafts</strong>.",
      $"""
      <p style="margin:0 0 16px;">You will be drafting in <strong>{Encode(draftName)}</strong>.</p>
      {coParticipantsHtml}
      """,
      DraftFooter
    );
  }

  public static string CoParticipantJoined(
    string recipientName,
    string newParticipantName,
    string draftName,
    IReadOnlyList<string> allParticipantNames
  )
  {
    ArgumentNullException.ThrowIfNull(allParticipantNames);
    var participantsHtml =
      allParticipantNames.Count > 0
        ? $"<p style=\"margin:0;\">Current participants: <strong>{string.Join(", ", allParticipantNames.Select(Encode))}</strong></p>"
        : string.Empty;

    return BaseLayout(
      recipientName,
      $"<strong>{Encode(newParticipantName)}</strong> has joined your draft.",
      $"""
      <p style="margin:0 0 16px;">Draft: <strong>{Encode(draftName)}</strong></p>
      {participantsHtml}
      """,
      DraftFooter
    );
  }

  public static string DraftCreated(string recipientName, string draftName, bool isPatreon)
  {
    return BaseLayout(
      recipientName,
      $"A new draft has been announced: <strong>{Encode(draftName)}</strong>",
      $"""
      {PatreonBadge(isPatreon)}
      <p style="margin:0;">A new Screen Drafts episode is in the works. Stay tuned!</p>
      """,
      DraftFooter
    );
  }

  public static string DraftCompleted(string recipientName, string draftName, bool isPatreon)
  {
    return BaseLayout(
      recipientName,
      $"<strong>{Encode(draftName)}</strong> is now available!",
      $"""
      {PatreonBadge(isPatreon)}
      <p style="margin:0;">The draft has been completed and published. Go check out the results!</p>
      """,
      DraftFooter
    );
  }

  public static string ConfirmEmailChange(
    string recipientName,
    string newEmail,
    string confirmationLink
  )
  {
    return BaseLayout(
      recipientName,
      "Confirm your new email address",
      $"""
      <p style="margin:0 0 24px;">You requested to change the email on your Screen Drafts account to <strong>{Encode(
        newEmail
      )}</strong>.</p>
      {Button("Confirm email change", confirmationLink)}
      <p style="margin:24px 0 0;font-size:13px;color:#8a8a99;">If you didn't request this, ignore this email — your address won't change unless you click the button above.</p>
      """,
      AccountFooter
    );
  }

  private static string PatreonBadge(bool isPatreon) =>
    isPatreon
      ? $"""
        <p style="display:inline-block;background:{Navy};color:{Cream};padding:4px 12px;border-radius:3px;font-family:{HeadFont};font-size:12px;font-weight:700;letter-spacing:0.08em;text-transform:uppercase;margin:0 0 20px;">Patreon Exclusive</p>
        """
      : string.Empty;

  private static string Button(string label, string url) =>
    $"""
      <table cellpadding="0" cellspacing="0" role="presentation">
        <tr>
          <td style="background:{Red};border-radius:4px;">
            <a href="{Encode(
        url
      )}" style="display:inline-block;padding:12px 28px;font-family:{HeadFont};font-size:14px;font-weight:700;letter-spacing:0.06em;text-transform:uppercase;color:#ffffff;text-decoration:none;">{Encode(
        label
      )}</a>
          </td>
        </tr>
      </table>
      """;

  private static string BaseLayout(
    string recipientName,
    string headline,
    string body,
    string footerText
  ) =>
    $$"""
      <!DOCTYPE html>
      <html lang="en">
      <head>
        <meta charset="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>Screen Drafts</title>
        <style>
          @media (max-width: 620px) {
            .sd-container { width: 100% !important; }
            .sd-pad { padding-left: 20px !important; padding-right: 20px !important; }
          }
        </style>
      </head>
      <body style="margin:0;padding:0;background:{{Cream}};font-family:{{BodyFont}};">
        <table width="100%" cellpadding="0" cellspacing="0" role="presentation" style="background:{{Cream}};padding:32px 0;">
          <tr>
            <td align="center">
              <table class="sd-container" width="600" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff;border-radius:8px;overflow:hidden;border:1px solid #e8e4d8;">
                <tr>
                  <td style="background:{{Navy}};border-bottom:4px solid {{Red}};padding:20px 32px;text-align:center;">
                    <img src="{{AssestsBasePath}}/{{LogoPath}}" alt="Screen Drafts" height="52" style="height:52px;width:auto;display:inline-block;border:0;" />
                  </td>
                </tr>
                <tr>
                  <td class="sd-pad" style="padding:32px;">
                    <p style="margin:0 0 16px;font-size:15px;color:#555566;">Hi {{Encode(
        recipientName
      )}},</p>
                    <p style="margin:0 0 24px;font-family:{{HeadFont}};font-size:19px;font-weight:600;color:{{Navy}};">{{headline}}</p>
                    <div style="font-size:15px;color:#3f3f4d;line-height:1.6;">
                      {{body}}
                    </div>
                  </td>
                </tr>
                <tr>
                  <td style="background:{{Cream}};padding:16px 32px;text-align:center;border-top:1px solid #e8e4d8;">
                    <p style="margin:0;font-size:12px;color:#8a8a99;">
                      {{footerText}}<br />
                      &copy; {{DateTime.UtcNow.Year}} Screen Drafts
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

  private static string Encode(string value) => System.Net.WebUtility.HtmlEncode(value);
}
