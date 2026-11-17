// next.config.ts
import type { NextConfig } from "next";

const SESSION_COOKIES = [
  "authjs.session-token",
  "authjs.session-token.0",
  "__Secure-authjs.session-token",
  "__Secure-authjs.session-token.0",
];

const nextConfig: NextConfig = {
  async rewrites() {
    return {
      beforeFiles: [
        {
          source: "/media/:id",
          missing: SESSION_COOKIES.map((key) => ({ type: "cookie" as const, key })),
          destination: "/media-public/:id",
        },
      ],
    };
  },
};

export default nextConfig;