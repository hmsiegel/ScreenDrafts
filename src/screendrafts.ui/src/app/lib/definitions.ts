import { env } from "./env";

export const definitions = {
  loginUrl: `${env.keycloak.baseUrl}/realms/${env.keycloak.realm}/protocol/openid-connect/auth` +
    `?client_id=${env.keycloak.clientId}` +
    `&response_type=code` +
    `&scope=openid` +
    `&redirect_uri=${env.keycloak.redirectUri}`
};