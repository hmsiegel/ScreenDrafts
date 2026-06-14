// src/middleware.ts
import { auth } from "@/auth";
import { NextResponse } from "next/server";

export default auth((req) => {
  const session = req.auth;

  // If the refresh token has expired, force re-authentication.
  if (session?.error === "RefreshTokenExpired") {
    const signInUrl = new URL("/api/auth/signin", req.url);
    signInUrl.searchParams.set("callbackUrl", req.url);
    return NextResponse.redirect(signInUrl);
  }

  return NextResponse.next();
});

export const config = {
  // Run on all routes except Next.js internals and static files.
  matcher: ["/((?!_next/static|_next/image|favicon.ico|api/auth).*)"],
};