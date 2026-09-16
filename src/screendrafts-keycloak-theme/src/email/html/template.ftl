<#--
  This file has been claimed for ownership from @keycloakify/email-native version 260007.0.0.
  To relinquish ownership and restore this file to its original content, run the following command:

  $ npx keycloakify own --path "email/html/template.ftl" --revert
-->

<#--
  Ownership taken via: npx keycloakify own --path "email/html/template.ftl"

  Screen Drafts brand shell for every email Keycloak sends directly
  (password reset, email verification, event notifications, etc.) —
  mirrors BaseLayout in
  ScreenDrafts.Modules.Communications.Features.Email.EmailTemplates.cs
  so Keycloak's own emails and the Communications module's emails look
  like the same product.

  Every content template (password-reset.ftl, email-verification.ftl, the
  event-*.ftl ones) is a thin passthrough — the actual paragraph/link text
  comes from messages/messages_en.properties as a single HTML blob, not
  from those files. That's why the <style> block below styles bare tags
  (p, a, strong, ul/li) instead of classes: it's the only styling hook
  available without editing every locale's properties file.

  Logo URL is a hardcoded absolute path (no build-time config exists for
  FreeMarker templates the way EmailTemplates.cs has AssetsBaseUrl) —
  points at the production domain, so it won't resolve when testing
  against a local Keycloak unless that path is also reachable there.
-->
<#macro emailLayout>
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>Screen Drafts</title>
  <style>
    body { margin:0; padding:0; background:#fbf7ee; font-family: Helvetica, Arial, sans-serif; }
    p { margin:0 0 16px; font-size:15px; color:#3f3f4d; line-height:1.6; }
    a { color:#cb2032; font-weight:700; text-decoration:none; }
    a:hover { text-decoration:underline; }
    strong { color:#0d1430; }
    ul, ol { margin:0 0 16px; padding-left:20px; color:#3f3f4d; font-size:15px; line-height:1.6; }
    @media (max-width: 620px) {
      .sd-container { width: 100% !important; }
      .sd-pad { padding-left: 20px !important; padding-right: 20px !important; }
    }
  </style>
</head>
<body>
  <table width="100%" cellpadding="0" cellspacing="0" role="presentation" style="background:#fbf7ee;padding:32px 0;">
    <tr>
      <td align="center">
        <table class="sd-container" width="600" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff;border-radius:8px;overflow:hidden;border:1px solid #e8e4d8;">
          <tr>
            <td style="background:#0d1430;border-bottom:4px solid #cb2032;padding:20px 32px;text-align:center;">
              <img src="https://screen-drafts.com/artifacts/logo.jpg" alt="Screen Drafts" height="52" style="height:52px;width:auto;display:inline-block;border:0;" />
            </td>
          </tr>
          <tr>
            <td class="sd-pad" style="padding:32px;">
              <#nested>
            </td>
          </tr>
          <tr>
            <td style="background:#fbf7ee;padding:16px 32px;text-align:center;border-top:1px solid #e8e4d8;">
              <p style="margin:0;font-size:12px;color:#8a8a99;">&copy; ${.now?string("yyyy")} Screen Drafts</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>
</#macro>
