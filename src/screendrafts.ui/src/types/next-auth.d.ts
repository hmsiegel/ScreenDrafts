import "next-auth";

declare module "next-auth" {
  interface Session {
    accessToken?: string;
    roles: string[];
    user: {
      name?: string | null;
      email?: string | null;
      image?: string | null;
    };
  }
}
