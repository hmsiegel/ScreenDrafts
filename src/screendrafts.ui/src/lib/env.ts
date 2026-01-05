export const env = {
   keycloak: {
      baseUrl: process.env.NEXT_PUBLIC_KEYCLOAK_BASE_URL,
      realm: process.env.NEXT_PUBLIC_KEYCLOAK_REALM,
      clientId: process.env.NEXT_PUBLIC_KEYCLOAK_CLIENT_ID,
      redirectUri: process.env.NEXT_PUBLIC_KEYCLOAK_REDIRECT_URI,
   },
   apiUrl: process.env.NEXT_PUBLIC_API_URL,
};