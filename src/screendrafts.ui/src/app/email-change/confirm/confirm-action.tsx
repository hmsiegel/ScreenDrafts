// app/email-change/confirm/confirm-action.tsx
'use client';

import { publicApiRequest } from "@/services/api";
import { useEffect, useState } from "react";

interface ConfirmActionProps {
  token: string;
}

type State = 'loading' | 'success' | 'error';

export default function ConfirmAction({ token }: ConfirmActionProps) {
  const [state, setState] = useState<State>('loading');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function confirm() {
      try {
        await publicApiRequest("/users/email-change/confirm", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ token }),
        });
        if (!cancelled) setState('success');
      } catch (err) {
        if (!cancelled) {
          setError((err as Error).message ?? "An error occurred");
          setState('error');
        }
      }
    }

    confirm();
    return () => {
      cancelled = true;
    };
  }, [token]);

  if (state === 'loading') {
    return (
      <>
        <h1 className="sd-register-title">Confirming...</h1>
        <p className="text-sm text-center opacity-80">One moment while we update your email.</p>
      </>
    );
  }

  if (state === 'success') {
    return (
      <>
        <h1 className="sd-register-title">Email Confirmed</h1>
        <p className="text-sm text-center opacity-80">
          Your email has been updated. Use it to sign in going forward.
        </p>
      </>
    );
  }

  return (
    <>
      <h1 className="sd-register-title">Link Not Valid</h1>
      <p className="text-sm text-center opacity-80">
        {error ?? "This link is invalid, expired, or has already been used."}
      </p>
      <p className="text-sm text-center opacity-80 mt-3">
        Request a new email change from your profile page and try again.
      </p>
    </>
  );
}