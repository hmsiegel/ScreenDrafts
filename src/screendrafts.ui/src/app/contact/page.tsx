// src/app/contact/page.tsx
import type { Metadata } from "next";
import { auth } from "@/auth";
import ContactForm from "@/components/features/contact/contact-form";

export const metadata: Metadata = {
  title: "Contact",
  description: "Report a bug, ask a question or share feedback about Screen Drafts.",
};

export default async function ContactPage() {
  const session = await auth();

  return (
    <main className="page-x py-10 lg:py-14 flex justify-center">
      <div className="sd-register-card w-full" style={{ maxWidth: 560 }}>
        <h1 className="sd-register-title">Contact Us</h1>
        <p className="text-sm text-slate-600 mb-5 text-center">
          Found a bug, have a question or want to share feedback? Send a note and we&apos;ll
          get back to you by email.
        </p>
        <ContactForm
          defaultName={session?.user?.name ?? ""}
          defaultEmail={session?.user?.email ?? ""}
        />
      </div>
    </main>
  );
}
