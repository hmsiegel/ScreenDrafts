import NextAuth from "next-auth";
import KeycloakProvider from "next-auth/providers/keycloak";

export const authOptions = {
   providers: [
      KeycloakProvider({
         clientId: process.env.KEYCLOAK_CLIENT_ID!,
         clientSecret: process.env.KEYCLOAK_CLIENT_SECRET!,
         issuer: process.env.KEYCLOAK_ISSUER!,
      }),
   ],
   callbacks: {
      async jwt({ token, account }: { token: any; account?: any }) {
         if ( account && account.access_token ) {
            token.accessToken = account.access_token;
         }
         return token;
      },
      async session({ session, token }: { session: any; token: any }) {
         // Type assertion to extend session with accessToken
         session.accessToken = (token as { accessToken?: string }).accessToken;
         return session;
      },
   },
   session: { strategy: "jwt" as const },
};

const handler = NextAuth(authOptions);

export { handler as GET, handler as POST };