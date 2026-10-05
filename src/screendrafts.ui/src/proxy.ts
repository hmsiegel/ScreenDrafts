// src/proxy.ts
import { auth } from "@/auth";
import { NextResponse } from "next/server";

// Auth.js stores the session in "authjs.session-token" (http) or "__Secure-authjs.session-token" (https).
// A JWT carrying both Keycloak tokens is large enough that Auth.js splits it into chunked cookies named
// "...session-token.0", "...session-token.1", so match by prefix instead of deleting one fixed name.
const SESSION_COOKIE_PREFIXES = ["authjs.session-token", "__Secure-authjs.session-token"];

const handler = auth((req) => {
  const session = req.auth;

  // If the refresh token has expired, force re-authentication.
  if (session?.error === "RefreshTokenExpired") {
    const signInUrl = new URL("/force-signin", req.url);
    signInUrl.searchParams.set("callbackUrl", req.url);

    // Clear the dead session on the redirect itself. Otherwise the next request still carries it,
    // tries to refresh a token Keycloak no longer accepts, and logs the same error again.
    const response = NextResponse.redirect(signInUrl);
    for (const cookie of req.cookies.getAll()) {
      if (SESSION_COOKIE_PREFIXES.some((prefix) => cookie.name.startsWith(prefix))) {
        response.cookies.delete(cookie.name);
      }
    }
    return response;
  }

  return NextResponse.next();
});

export { handler as proxy };

export const config = {
  // Run on all routes except Next.js internals and static files.
  matcher: ["/((?!_next/static|_next/image|favicon.ico|api/auth|force-signin).*)"],
};