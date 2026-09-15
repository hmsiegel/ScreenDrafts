// app/email-change/claim/claim-form.tsx
'use client';

import { publicApiRequest } from "@/services/api";
import { ChangeEvent, useState } from "react";

interface ClaimFormProps {
  token: string;
}

export default function ClaimForm({ token }: ClaimFormProps) {
  const [email, setEmail] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [succeeded, setSucceeded] = useState(false);

  function handleChange(event: ChangeEvent<HTMLInputElement>) {
    setEmail(event.target.value);
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setIsLoading(true);
    setError(null);

    try {
      await publicApiRequest("/users/email-change/bootstrap/claim", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ token, newEmail: email }),
      });
      setSucceeded(true);
    } catch (err) {
      setError((err as Error).message ?? "An error occurred");
    } finally {
      setIsLoading(false);
    }
  }

  if (succeeded) {
    return (
      <div className="text-center flex flex-col gap-2">
        <p>
          Your email has been updated to <strong>{email}</strong>.
        </p>
        <p className="sd-register-label">Check that inbox for a link to set your password.</p>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      <div className="flex flex-col gap-1">
        <label htmlFor="newEmail" className="sd-register-label">
          New Email Address
        </label>
        <input
          id="newEmail"
          name="newEmail"
          type="email"
          value={email}
          onChange={handleChange}
          autoComplete="off"
          required
          className="sd-register-input"
        />
      </div>

      {error && (
        <div className="sd-alert sd-alert-error">
          {error}
        </div>
      )}

      <button
        type="submit"
        disabled={isLoading}
        className="sd-register-btn mt-1"
      >
        {isLoading ? "Confirming..." : "Confirm Email"}
      </button>
    </form>
  );
}