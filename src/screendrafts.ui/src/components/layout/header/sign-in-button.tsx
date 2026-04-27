'use client';

import { signIn } from "next-auth/react";

export default function SignInButton() {
  return (
    <button
      onClick={() => signIn('keycloak', { callbackUrl: '/dashboard' })}
      className="bg-sd-blue text-white px-[18px] py-2.5 rounded font-oswald font-medium text-sm tracking-[0.14em] hover:bg-blue-700 transition-colors"
    >
      SIGN IN
    </button>
  );
}
