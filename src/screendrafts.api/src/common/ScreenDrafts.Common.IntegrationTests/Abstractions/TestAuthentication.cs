using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ScreenDrafts.Common.Abstractions.Authentication;
using ScreenDrafts.Modules.Audit.Domain;

namespace ScreenDrafts.Common.IntegrationTests.Abstractions;

/// <summary>
/// Opt-in HTTP test doubles for modules whose integration tests need to exercise the real
/// authorization pipeline (401 / 403 / 200) without standing up Keycloak or touching another
/// module's schema.
/// <para>
/// A request authenticates by sending <c>Authorization: Test perm1,perm2</c>. The permissions
/// become <c>permission</c> claims directly, so <c>CustomClaimsTransformation</c> sees them and
/// never looks them up in the Users schema. No header means no identity, which the pipeline
/// answers with 401. An identity whose permissions do not include the endpoint's policy is
/// answered with 403.
/// </para>
/// </summary>
public static class TestAuthentication
{
  public const string Scheme = "Test";

  // CustomClaimsTransformation looks permissions up in the Users schema when a principal has
  // no permission claim at all. A "no permissions" user therefore still carries one claim that
  // matches no real policy, so the lookup is skipped and the policy check fails with 403.
  private const string NoPermissionsPlaceholder = "test:no-permissions";

  public static IServiceCollection AddTestAuthentication(this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services
      .AddAuthentication(options =>
      {
        options.DefaultScheme = Scheme;
        options.DefaultAuthenticateScheme = Scheme;
        options.DefaultChallengeScheme = Scheme;
        options.DefaultForbidScheme = Scheme;
      })
      .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(Scheme, _ => { });

    return services;
  }

  /// <summary>
  /// Replaces the audit writer so HTTP requests do not need the audit schema.
  /// </summary>
  public static IServiceCollection AddNoOpAuditWriter(this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.RemoveAll<IAuditWriteService>();
    services.AddSingleton<IAuditWriteService, NoOpAuditWriteService>();

    return services;
  }

  public static void AuthenticateWith(this HttpClient client, params string[] permissions)
  {
    ArgumentNullException.ThrowIfNull(client);

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      Scheme,
      string.Join(',', permissions)
    );
  }

  private sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
  ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
  {
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
      // Parsed by hand: AuthenticationHeaderValue.TryParse treats the comma in
      // "Test a,b" as a header-value separator and rejects the whole value.
      var prefix = TestAuthentication.Scheme;

      if (
        !Request.Headers.TryGetValue("Authorization", out var header)
        || header.ToString() is not { } raw
        || !raw.StartsWith(prefix, StringComparison.Ordinal)
        || (raw.Length > prefix.Length && raw[prefix.Length] != ' ')
      )
      {
        return Task.FromResult(AuthenticateResult.NoResult());
      }

      var permissions = raw[prefix.Length..]
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .DefaultIfEmpty(NoPermissionsPlaceholder);

      var userId = Guid.NewGuid();

      var claims = new List<Claim>
      {
        new(ClaimTypes.NameIdentifier, userId.ToString()),
        new(CustomClaims.Sub, userId.ToString()),
        new(CustomClaims.PublicId, "u_integrationtest0001"),
      };

      claims.AddRange(permissions.Select(p => new Claim(CustomClaims.Permission, p)));

      var identity = new ClaimsIdentity(claims, TestAuthentication.Scheme);
      var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), TestAuthentication.Scheme);

      return Task.FromResult(AuthenticateResult.Success(ticket));
    }
  }

  private sealed class NoOpAuditWriteService : IAuditWriteService
  {
    public Task WriteHttpLogAsync(HttpAuditLog log, CancellationToken cancellationToken = default) =>
      Task.CompletedTask;

    public Task WriteDomainEventLogAsync(
      DomainEventAuditLog log,
      CancellationToken cancellationToken = default
    ) => Task.CompletedTask;

    public Task WriteAuthLogAsync(AuthAuditLog log, CancellationToken cancellationToken = default) =>
      Task.CompletedTask;
  }
}
