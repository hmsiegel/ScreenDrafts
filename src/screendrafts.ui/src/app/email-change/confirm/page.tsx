// app/email-change/confirm/page.tsx
import Image from "next/image";
import { oswald } from "@/styles/fonts";
import type { Metadata } from "next";
import ConfirmAction from "./confirm-action";

export const metadata: Metadata = { title: "Confirm Email Change" };
export const dynamic = "force-dynamic";

// NOTE: same Next.js-version caveat as the bootstrap claim page — searchParams
// as a Promise is the Next 15 convention; drop the `await` if this repo is on
// an older version.
export default async function ConfirmEmailChangePage({
  searchParams,
}: {
  searchParams: Promise<{ token?: string }>;
}) {
  const { token } = await searchParams;

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
          {token ? (
            <ConfirmAction token={token} />
          ) : (
            <>
              <h1 className="sd-register-title">Link Not Valid</h1>
              <p className="text-sm text-center opacity-80">
                This link is missing a token. Check that you copied the whole link from your email.
              </p>
            </>
          )}
        </div>
      </div>
    </div>
  );
}