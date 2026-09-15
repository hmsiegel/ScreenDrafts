// app/email-change/claim/page.tsx
import Image from "next/image";
import { oswald } from "@/styles/fonts";
import { env } from "@/lib/env";
import type { Metadata } from "next";
import ClaimForm from "./claim-form";

export const metadata: Metadata = { title: "Confirm Your Email" };
export const dynamic = "force-dynamic";

interface ValidationResult {
  isValid: boolean;
  userPublicId?: string;
  error?: string;
}

async function extractErrorMessage(res: Response): Promise<string> {
  try {
    const body = await res.json();
    return body?.detail ?? body?.title ?? "This link isn't valid.";
  } catch {
    return "This link isn't valid.";
  }
}

async function validateToken(apiBase: string, token: string | undefined): Promise<ValidationResult> {
  if (!token) {
    return { isValid: false, error: "This link is missing a token." };
  }

  try {
    const url = new URL(`${apiBase}/users/email-change/bootstrap/validate`);
    url.searchParams.set("token", token);

    const res = await fetch(url.toString(), { cache: "no-store" });

    if (!res.ok) {
      return { isValid: false, error: await extractErrorMessage(res) };
    }

    const data = (await res.json()) as { isValid?: boolean; userPublicId?: string };
    return { isValid: !!data.isValid, userPublicId: data.userPublicId };
  } catch {
    return { isValid: false, error: "Couldn't reach the server. Please try again in a moment." };
  }
}

// NOTE: searchParams as a Promise is the Next.js 15 App Router convention —
// if this repo is on an older Next version, drop the `await` and type it as
// a plain object instead: `{ searchParams }: { searchParams: { token?: string } }`.
export default async function ClaimPage({
  searchParams,
}: {
  searchParams: Promise<{ token?: string }>;
}) {
  const { token } = await searchParams;
  const apiBase = env.apiUrl ?? "";
  const result = await validateToken(apiBase, token);

  return (
    <div className="sd-page-bg">
      <div className="flex flex-col items-center gap-6 px-4 py-12 w-full" style={{ maxWidth: 440 }}>
        <div className="flex flex-col items-center gap-3">
          <Image
            src="/screen-drafts.jpg"
            alt="ScreenDrafts"
            width={96}
            height={96}
            className="rounded-xl shadow-lg"
          />
          <div className={`${oswald.className} text-2xl font-semibold tracking-widest text-[#fbf7ee] uppercase`}>
            Screen Drafts
          </div>
        </div>

        <div className="sd-register-card w-full">
          {result.isValid && token ? (
            <>
              <h1 className="sd-register-title">Confirm Your Email</h1>
              <p className="text-sm text-center mb-4 opacity-80">
                Enter the real email address you&apos;d like to use for your ScreenDrafts account.
                We&apos;ll send a password reset link there once it&apos;s confirmed.
              </p>
              <ClaimForm token={token} />
            </>
          ) : (
            <>
              <h1 className="sd-register-title">Link Not Valid</h1>
              <p className="text-sm text-center opacity-80">
                {result.error ?? "This link is invalid, expired, or has already been used."}
              </p>
              <p className="text-sm text-center opacity-80 mt-3">
                If you think this is a mistake, reach out on Discord or Patreon and we&apos;ll get you a new one.
              </p>
            </>
          )}
        </div>
      </div>
    </div>
  );
}