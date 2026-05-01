package com.screendrafts.keycloak;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;

import org.keycloak.authentication.AuthenticationFlowContext;
import org.keycloak.authentication.AuthenticationFlowError;
import org.keycloak.authentication.Authenticator;
import org.keycloak.models.KeycloakSession;
import org.keycloak.models.RealmModel;
import org.keycloak.models.UserModel;

public class SocialRegistrationAuthenticator implements Authenticator {

  private static final String PUBLIC_ID_ATTRIBUTE = "public_id";
  private static final String SECRET_HEADER = "X-Keycloak-Secret";
  private static final HttpClient HTTP_CLIENT = HttpClient.newBuilder()
      .connectTimeout(Duration.ofSeconds(5))
      .build();

  @Override
  public void authenticate(AuthenticationFlowContext context) {
    UserModel user = context.getUser();

    // If the user already has a public_id, nothing to do.
    String existingPublicId = user.getFirstAttribute(PUBLIC_ID_ATTRIBUTE);
    if (existingPublicId != null && !existingPublicId.isBlank()) {
      context.success();
      return;
    }

    // Read config from environment variables.
    String apiUrl = System.getenv("SD_API_URL");
    String secret = System.getenv("SD_KEYCLOAK_SECRET");

    if (apiUrl == null || apiUrl.isBlank() || secret == null || secret.isBlank()) {
      context.failure(
          AuthenticationFlowError.INTERNAL_ERROR,
          context.form()
              .setError("Social registration is not configured. Contact an administrator.")
              .createErrorPage(jakarta.ws.rs.core.Response.Status.INTERNAL_SERVER_ERROR));
      return;
    }

    // Build the registration payload.
    String email = user.getEmail() != null ? user.getEmail() : "";
    String firstName = user.getFirstName() != null ? user.getFirstName() : "";
    String lastName = user.getLastName() != null ? user.getLastName() : "";
    String identityId = user.getId();

    String json = String.format(
        "{\"email\":\"%s\",\"firstName\":\"%s\",\"lastName\":\"%s\",\"identityId\":\"%s\"}",
        escapeJson(email),
        escapeJson(firstName),
        escapeJson(lastName),
        escapeJson(identityId));

    try {
      HttpRequest request = HttpRequest.newBuilder()
          .uri(URI.create(apiUrl + "/users/register/social"))
          .header("Content-Type", "application/json")
          .header(SECRET_HEADER, secret)
          .POST(HttpRequest.BodyPublishers.ofString(json))
          .timeout(Duration.ofSeconds(10))
          .build();

      HttpResponse<String> response = HTTP_CLIENT.send(
          request,
          HttpResponse.BodyHandlers.ofString());

      if (response.statusCode() == 201) {
        // Parse publicId from response body.
        // Response shape: {"publicId":"u_..."}
        String body = response.body();
        String publicId = extractPublicId(body);

        if (publicId != null && !publicId.isBlank()) {
          user.setSingleAttribute(PUBLIC_ID_ATTRIBUTE, publicId);
          context.success();
        } else {
          context.failure(
              AuthenticationFlowError.INTERNAL_ERROR,
              context.form()
                  .setError("Registration failed: invalid response from backend.")
                  .createErrorPage(jakarta.ws.rs.core.Response.Status.INTERNAL_SERVER_ERROR));
        }
      } else {
        // Backend returned an error — block the login.
        context.failure(
            AuthenticationFlowError.INTERNAL_ERROR,
            context.form()
                .setError("Registration failed. Please try again or contact support.")
                .createErrorPage(jakarta.ws.rs.core.Response.Status.INTERNAL_SERVER_ERROR));
      }

    } catch (Exception e) {
      context.failure(
          AuthenticationFlowError.INTERNAL_ERROR,
          context.form()
              .setError("Registration service unavailable. Please try again later.")
              .createErrorPage(jakarta.ws.rs.core.Response.Status.SERVICE_UNAVAILABLE));
    }
  }

  @Override
  public void action(AuthenticationFlowContext context) {
    // No user interaction required — this authenticator is fully automatic.
  }

  @Override
  public boolean requiresUser() {
    return true;
  }

  @Override
  public boolean configuredFor(KeycloakSession session, RealmModel realm, UserModel user) {
    return true;
  }

  @Override
  public void setRequiredActions(KeycloakSession session, RealmModel realm, UserModel user) {
    // No required actions needed.
  }

  @Override
  public void close() {
    // Nothing to close.
  }

  // Minimal JSON string escaper — avoids pulling in a JSON library.
  private static String escapeJson(String value) {
    if (value == null) return "";
    return value
        .replace("\\", "\\\\")
        .replace("\"", "\\\"")
        .replace("\n", "\\n")
        .replace("\r", "\\r")
        .replace("\t", "\\t");
  }

  // Minimal publicId extractor from {"publicId":"u_..."}
  // Avoids pulling in a JSON library dependency.
  private static String extractPublicId(String json) {
    if (json == null) return null;
    String key = "\"publicId\":\"";
    int start = json.indexOf(key);
    if (start == -1) return null;
    start += key.length();
    int end = json.indexOf("\"", start);
    if (end == -1) return null;
    return json.substring(start, end);
  }
}