// src/auth.ts
import NextAuth, { type DefaultSession } from "next-auth";
import KeycloakProvider from "next-auth/providers/keycloak";
import type { JWT } from "next-auth/jwt";

declare module "next-auth" {
  interface Session {
    accessToken?: string | undefined;
    publicId: string | undefined;
    personPublicId: string | undefined;
    roles: string[];
    user: DefaultSession["user"];
    error?: "RefreshTokenExpired";
  }
}

const API_BASE = process.env.NEXT_PUBLIC_API_URL;
const REFRESH_BUFFER_MS = 60 * 1000;

// Registration -> Guest role is asynchronous (outbox + inbox hops between the
// Users and Administration modules, roughly 10-15s). Someone who signs in
// before it lands gets empty roles. Re-check for a short window after sign-in
// instead of caching that empty result until the next sign-in.
const IDENTITY_RETRY_INTERVAL_MS = 5 * 1000;
const IDENTITY_RETRY_WINDOW_MS = 2 * 60 * 1000;

interface AppToken extends JWT {
  accessToken?: string;
  refreshToken?: string;
  accessTokenExpiresAt?: number;
  publicId?: string;
  personPublicId?: string;
  roles?: string[];
  signedInAt?: number;
  identityCheckedAt?: number;
  error?: "RefreshTokenExpired";
}

interface Identity {
  publicId?: string | undefined;
  personPublicId?: string | undefined;
  roles?: string[] | undefined;
}

async function refreshAccessToken(refreshToken: string): Promise<{
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: number;
} | null> {
  try {
    const url = `${process.env.KEYCLOAK_ISSUER}/protocol/openid-connect/token`;
    const res = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body: new URLSearchParams({
        grant_type: "refresh_token",
        client_id: process.env.KEYCLOAK_CLIENT_ID!,
        client_secret: process.env.KEYCLOAK_CLIENT_SECRET!,
        refresh_token: refreshToken,
      }).toString(),
    });

    if (!res.ok) {
      console.error("[auth] Token refresh failed:", res.status, await res.text());
      return null;
    }

    const data = await res.json() as {
      access_token: string;
      refresh_token: string;
      expires_in: number;
    };

    return {
      accessToken: data.access_token,
      refreshToken: data.refresh_token,
      accessTokenExpiresAt: Date.now() + data.expires_in * 1000,
    };
  } catch (e) {
    console.error("[auth] Token refresh error:", e);
    return null;
  }
}

/**
 * Loads the user's profile and roles from the API. Returns only what it could
 * fetch, so callers can merge it over an existing token without wiping fields
 * a failed call didn't touch. Failures are logged, never thrown.
 */
async function loadIdentity(accessToken: string): Promise<Identity> {
  const identity: Identity = {};

  try {
    const userRes = await fetch(`${API_BASE}/users/profile`, {
      headers: { Authorization: `Bearer ${accessToken}` },
    });

    if (!userRes.ok) {
      // This used to fail silently — now logs exactly what came back, which
      // is the first thing to check if menus disappear again. A 404 here
      // specifically means /users/profile couldn't resolve the JWT's
      // identity to a users.users row.
      console.error(
        "[auth] /users/profile failed:",
        userRes.status,
        await userRes.text()
      );
    }

    if (userRes.ok) {
      const user = (await userRes.json()) as { publicId?: string; personPublicId?: string };
      if (user.publicId) {
        identity.publicId = user.publicId;
        identity.personPublicId = user.personPublicId;

        const rolesRes = await fetch(
          `${API_BASE}/admin/users/${user.publicId}/roles`,
          { headers: { Authorization: `Bearer ${accessToken}` } }
        );
        if (rolesRes.ok) {
          const data = (await rolesRes.json()) as { roles: string[] };
          identity.roles = data.roles;
        } else {
          // Same visibility for the roles call.
          console.error(
            "[auth] /admin/users/{publicId}/roles failed:",
            rolesRes.status,
            await rolesRes.text()
          );
        }
      } else {
        // Profile came back OK but had no publicId — worth knowing that
        // distinct case too.
        console.error("[auth] /users/profile returned no publicId:", user);
      }
    }
  } catch (e) {
    // Log what actually threw, instead of discarding it.
    console.error("[auth] Role fetching threw:", e);
  }

  return identity;
}

/**
 * If the token has no roles yet (brand-new account whose Guest role hasn't
 * landed), re-fetch identity — throttled, and only for a short window after
 * sign-in. Tokens without signedInAt (issued before this existed) never retry.
 */
async function completeIdentity(token: AppToken): Promise<AppToken> {
  if (token.publicId && token.roles?.length) return token;
  if (!token.accessToken) return token;

  const now = Date.now();
  if (now - (token.signedInAt ?? 0) > IDENTITY_RETRY_WINDOW_MS) return token;
  if (now - (token.identityCheckedAt ?? 0) < IDENTITY_RETRY_INTERVAL_MS) return token;

  const identity = await loadIdentity(token.accessToken);
  return { ...token, ...identity, identityCheckedAt: now };
}

export const { handlers, auth, signIn, signOut } = NextAuth({
  secret: process.env.NEXTAUTH_SECRET,
  providers: [
    KeycloakProvider({
      clientId: process.env.KEYCLOAK_CLIENT_ID!,
      clientSecret: process.env.KEYCLOAK_CLIENT_SECRET!,
      issuer: process.env.KEYCLOAK_ISSUER!,
      authorization: { params: { prompt: "login" } },
    }),
  ],
  events: {
    // Signing out only clears the Auth.js cookie; the Keycloak SSO session
    // would survive, and the next sign-in (prompt=login) lands on "Please
    // re-authenticate" for the old user, with no way to switch accounts.
    // Ending the Keycloak session server-side with the refresh token fixes
    // that — no browser redirect, id_token, or post-logout redirect URI needed.
    async signOut(message) {
      if (!("token" in message) || !message.token) return;

      const refreshToken = (message.token as AppToken).refreshToken;
      if (!refreshToken) return;

      try {
        const res = await fetch(
          `${process.env.KEYCLOAK_ISSUER}/protocol/openid-connect/logout`,
          {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: new URLSearchParams({
              client_id: process.env.KEYCLOAK_CLIENT_ID!,
              client_secret: process.env.KEYCLOAK_CLIENT_SECRET!,
              refresh_token: refreshToken,
            }).toString(),
          }
        );

        if (!res.ok) {
          console.error("[auth] Keycloak logout failed:", res.status, await res.text());
        }
      } catch (e) {
        console.error("[auth] Keycloak logout error:", e);
      }
    },
  },
  callbacks: {
    async jwt({ token, account }) {
      const appToken = token as AppToken;

      // ── Initial sign-in ─────────────────────────────────────────────────
      if (account?.access_token) {
        appToken.accessToken = account.access_token;
        appToken.refreshToken = account.refresh_token as string | undefined;
        appToken.accessTokenExpiresAt = account.expires_at
          ? account.expires_at * 1000
          : Date.now() + ((account.expires_in as number) ?? 300) * 1000;
        appToken.signedInAt = Date.now();
        appToken.identityCheckedAt = Date.now();

        Object.assign(appToken, await loadIdentity(account.access_token));

        return appToken;
      }

      // ── Subsequent calls — check expiry ──────────────────────────────────
      const expiresAt = appToken.accessTokenExpiresAt ?? 0;
      if (Date.now() <= expiresAt - REFRESH_BUFFER_MS) {
        return completeIdentity(appToken);
      }

      // ── Refresh ──────────────────────────────────────────────────────────
      const refreshToken = appToken.refreshToken;
      if (!refreshToken) {
        console.warn("[auth] No refresh token — session will expire.");
        return appToken;
      }

      const refreshed = await refreshAccessToken(refreshToken);
      if (!refreshed) {
        // Refresh token expired or invalid — force re-authentication.
        return { ...appToken, accessToken: undefined, error: "RefreshTokenExpired" };
      }

      return completeIdentity({
        ...appToken,
        accessToken: refreshed.accessToken,
        refreshToken: refreshed.refreshToken,
        accessTokenExpiresAt: refreshed.accessTokenExpiresAt,
      });
    },

    async session({ session, token }) {
      const appToken = token as AppToken;
      session.accessToken = appToken.accessToken;
      session.publicId = appToken.publicId;
      session.personPublicId = appToken.personPublicId;
      session.roles = appToken.roles ?? [];
      session.error = appToken.error;
      return session;
    },
  },
});