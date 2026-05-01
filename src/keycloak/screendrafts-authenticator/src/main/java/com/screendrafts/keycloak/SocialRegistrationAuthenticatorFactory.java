package com.screendrafts.keycloak;

import java.util.Collections;
import java.util.List;

import org.keycloak.Config;
import org.keycloak.authentication.Authenticator;
import org.keycloak.authentication.AuthenticatorFactory;
import org.keycloak.models.AuthenticationExecutionModel;
import org.keycloak.models.KeycloakSession;
import org.keycloak.models.KeycloakSessionFactory;
import org.keycloak.provider.ProviderConfigProperty;

public class SocialRegistrationAuthenticatorFactory  implements AuthenticatorFactory{
   public static final String PROVIDER_ID = "screendrafts-social-registration";

  private static final SocialRegistrationAuthenticator SINGLETON =
      new SocialRegistrationAuthenticator();

  private static final AuthenticationExecutionModel.Requirement[] REQUIREMENT_CHOICES = {
      AuthenticationExecutionModel.Requirement.REQUIRED
  };

  @Override
  public String getId() {
    return PROVIDER_ID;
  }

  @Override
  public String getDisplayType() {
    return "ScreenDrafts Social Registration";
  }

  @Override
  public String getReferenceCategory() {
    return "social-registration";
  }

  @Override
  public String getHelpText() {
    return "Registers social login users in the ScreenDrafts backend and sets their public_id attribute.";
  }

  @Override
  public boolean isConfigurable() {
    return false;
  }

  @Override
  public AuthenticationExecutionModel.Requirement[] getRequirementChoices() {
    return REQUIREMENT_CHOICES;
  }

  @Override
  public boolean isUserSetupAllowed() {
    return false;
  }

  @Override
  public List<ProviderConfigProperty> getConfigProperties() {
    return Collections.emptyList();
  }

  @Override
  public Authenticator create(KeycloakSession session) {
    return SINGLETON;
  }

  @Override
  public void init(Config.Scope config) {
    // No initialization needed.
  }

  @Override
  public void postInit(KeycloakSessionFactory factory) {
    // No post-initialization needed.
  }

  @Override
  public void close() {
    // Nothing to close.
  }
}
