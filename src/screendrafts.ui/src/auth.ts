import NextAuth from "next-auth";
import KeycloakProvider from "next-auth/providers/keycloak";

declare module "next-auth" {
  interface Session {
    accessToken?: string;
    roles: string[];
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
    }),
  ],
  callbacks: {
    async jwt({ token, account }) {
      if (account?.access_token) {
        (token as Record<string, unknown>).accessToken = account.access_token;

        // First login: fetch backend publicId then roles.
        // TODO: confirm the user-lookup endpoint with the backend team.
        try {
          const userRes = await fetch(`${API_BASE}/users/me`, {
            headers: { Authorization: `Bearer ${account.access_token}` },
          });
          if (userRes.ok) {
            const user = await userRes.json() as { publicId?: string };
            if (user.publicId) {
              const rolesRes = await fetch(
                `${API_BASE}/admin/users/${user.publicId}/roles`,
                { headers: { Authorization: `Bearer ${account.access_token}` } }
              );
              if (rolesRes.ok) {
                const data = await rolesRes.json() as { roles: string[] };
                (token as Record<string, unknown>).roles = data.roles;
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
      const t = token as Record<string, unknown>;
      session.accessToken = t.accessToken as string | undefined;
      session.roles = (t.roles as string[] | undefined) ?? [];
      return session;
    },
  },
});
