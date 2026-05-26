import NextAuth, { type DefaultSession } from "next-auth";
import KeycloakProvider from "next-auth/providers/keycloak";

declare module "next-auth" {
  interface Session {
    accessToken?: string | undefined;
    publicId: string | undefined;
    roles: string[];
    user: DefaultSession["user"];
  }
}

const API_BASE = process.env.NEXT_PUBLIC_API_URL;

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
      if (account?.access_token) {
        token["accessToken"] = account.access_token;

        try {
          const userRes = await fetch(`${API_BASE}/users/profile`, {
            headers: { Authorization: `Bearer ${account.access_token}` },
          });
          if (userRes.ok) {
            const user = (await userRes.json()) as { publicId?: string };
            if (user.publicId) {
              token["publicId"] = user.publicId;

              const rolesRes = await fetch(
                `${API_BASE}/admin/users/${user.publicId}/roles`,
                { headers: { Authorization: `Bearer ${account.access_token}` } }
              );
              if (rolesRes.ok) {
                const data = (await rolesRes.json()) as { roles: string[] };
                token["roles"] = data.roles;
              }
            }
          }
        } catch {
          // Role fetching failed — proceed with empty roles.
        }
      }
      return token;
    },
    async session({ session, token }) {
      session.accessToken = token["accessToken"] as string | undefined;
      session.publicId = token["publicId"] as string | undefined;
      session.roles = (token["roles"] as string[] | undefined) ?? [];
      return session;
    },
  },
});