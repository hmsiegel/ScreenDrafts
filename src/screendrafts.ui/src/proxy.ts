// src/proxy.ts
import { auth } from "@/auth";
import { NextResponse } from "next/server";

const handler = auth((req) => {
  const session = req.auth;

  // If the refresh token has expired, force re-authentication.
  if (session?.error === "RefreshTokenExpired") {
    const signInUrl = new URL("/api/auth/signin", req.url);
    signInUrl.searchParams.set("callbackUrl", req.url);
    return NextResponse.redirect(signInUrl);
  }

  return NextResponse.next();
});

export { handler as proxy };

export const config = {
  // Run on all routes except Next.js internals and static files.
  matcher: ["/((?!_next/static|_next/image|favicon.ico|api/auth).*)"],
};