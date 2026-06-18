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

interface AppToken extends JWT {
  accessToken?: string;
  refreshToken?: string;
  accessTokenExpiresAt?: number;
  publicId?: string;
  personPublicId?: string;
  roles?: string[];
  error?: "RefreshTokenExpired";
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

        try {
          const userRes = await fetch(`${API_BASE}/users/profile`, {
            headers: { Authorization: `Bearer ${account.access_token}` },
          });
          if (userRes.ok) {
            const user = (await userRes.json()) as { publicId?: string; personPublicId?: string };
            if (user.publicId) {
              appToken.publicId = user.publicId;
              appToken.personPublicId = user.personPublicId;

              const rolesRes = await fetch(
                `${API_BASE}/admin/users/${user.publicId}/roles`,
                { headers: { Authorization: `Bearer ${account.access_token}` } }
              );
              if (rolesRes.ok) {
                const data = (await rolesRes.json()) as { roles: string[] };
                appToken.roles = data.roles;
              }
            }
          }
        } catch {
          // Role fetching failed — proceed with empty roles.
        }

        return appToken;
      }

      // ── Subsequent calls — check expiry ──────────────────────────────────
      const expiresAt = appToken.accessTokenExpiresAt ?? 0;
      if (Date.now() <= expiresAt - REFRESH_BUFFER_MS) {
        return appToken;
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

      return {
        ...appToken,
        accessToken: refreshed.accessToken,
        refreshToken: refreshed.refreshToken,
        accessTokenExpiresAt: refreshed.accessTokenExpiresAt,
      };
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