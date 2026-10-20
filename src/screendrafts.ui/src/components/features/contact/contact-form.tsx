// src/components/features/contact/contact-form.tsx
'use client';

import { publicApiRequest } from "@/services/api";
import { env } from "@/lib/env";
import { ChangeEvent, useState } from "react";
import TurnstileWidget from "./turnstile-widget";

// Keep in step with ContactMessage.Max* and the validator on the API.
const MAX_NAME = 100;
const MAX_EMAIL = 254;
const MIN_MESSAGE = 10;
const MAX_MESSAGE = 2000;

const CATEGORIES = [
  { value: "Question", label: "Question" },
  { value: "Bug", label: "Bug report" },
  { value: "Feedback", label: "Feedback" },
  { value: "AccountHelp", label: "Account help" },
] as const;

type Status = "idle" | "sending" | "sent";

interface ContactFormProps {
  defaultName?: string;
  defaultEmail?: string;
}

function describeError(err: unknown): string {
  const text = err instanceof Error ? err.message : "";
  const status = /status (\d{3})/.exec(text)?.[1];

  switch (status) {
    case "429":
      return "You've sent several messages recently. Please try again in an hour.";
    case "400":
      return "We couldn't verify that submission. Check your details, complete the verification again and resend.";
    default:
      return "Something went wrong on our end. Please try again in a moment.";
  }
}

export default function ContactForm({ defaultName = "", defaultEmail = "" }: ContactFormProps) {
  const [form, setForm] = useState({
    name: defaultName,
    email: defaultEmail,
    category: "Question",
    message: "",
    website: "", // honeypot: real visitors never see or fill it
  });
  const [token, setToken] = useState<string | null>(null);
  const [resetSignal, setResetSignal] = useState(0);
  const [status, setStatus] = useState<Status>("idle");
  const [error, setError] = useState<string | null>(null);

  const siteKey = env.turnstileSiteKey;
  const sending = status === "sending";
  const messageLength = form.message.trim().length;
  const canSubmit =
    !sending && !!token && !!siteKey && messageLength >= MIN_MESSAGE && messageLength <= MAX_MESSAGE;

  function handleChange(
    event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) {
    setForm((prev) => ({ ...prev, [event.target.name]: event.target.value }));
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!canSubmit) return;

    setStatus("sending");
    setError(null);

    try {
      await publicApiRequest("/contact", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          name: form.name.trim(),
          email: form.email.trim(),
          category: form.category,
          message: form.message.trim(),
          turnstileToken: token,
          website: form.website,
        }),
      });
      setStatus("sent");
    } catch (err) {
      setError(describeError(err));
      setStatus("idle");
    } finally {
      // A Turnstile token works once, so a retry always needs a fresh one.
      setToken(null);
      setResetSignal((n) => n + 1);
    }
  }

  if (status === "sent") {
    return (
      <div className="flex flex-col gap-2 text-center" role="status">
        <p className="font-semibold">Message sent. Thank you.</p>
        <p className="sd-register-label">We&apos;ll reply to {form.email.trim()}.</p>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      <Field id="name" label="Name">
        <input
          id="name"
          name="name"
          type="text"
          value={form.name}
          onChange={handleChange}
          maxLength={MAX_NAME}
          autoComplete="name"
          required
          className="sd-register-input"
        />
      </Field>

      <Field id="email" label="Email">
        <input
          id="email"
          name="email"
          type="email"
          value={form.email}
          onChange={handleChange}
          maxLength={MAX_EMAIL}
          autoComplete="email"
          required
          className="sd-register-input"
        />
      </Field>

      <Field id="category" label="Topic">
        <select
          id="category"
          name="category"
          value={form.category}
          onChange={handleChange}
          className="sd-register-input"
        >
          {CATEGORIES.map((c) => (
            <option key={c.value} value={c.value}>
              {c.label}
            </option>
          ))}
        </select>
      </Field>

      <Field id="message" label="Message">
        <textarea
          id="message"
          name="message"
          value={form.message}
          onChange={handleChange}
          maxLength={MAX_MESSAGE}
          rows={6}
          required
          className="sd-register-input resize-y"
        />
        <div className="font-mono text-[10px] text-right text-slate-500">
          {form.message.length}/{MAX_MESSAGE}
        </div>
      </Field>

      {/* Honeypot. Off-screen rather than display:none, which some bots skip. */}
      <div aria-hidden="true" className="absolute -left-[9999px] h-0 w-0 overflow-hidden">
        <label htmlFor="website">Website</label>
        <input
          id="website"
          name="website"
          type="text"
          value={form.website}
          onChange={handleChange}
          tabIndex={-1}
          autoComplete="off"
        />
      </div>

      {siteKey ? (
        <TurnstileWidget siteKey={siteKey} onToken={setToken} resetSignal={resetSignal} />
      ) : (
        <div className="sd-alert sd-alert-error">
          The contact form is unavailable right now. Please try again later.
        </div>
      )}

      {error && (
        <div className="sd-alert sd-alert-error" role="alert">
          {error}
        </div>
      )}

      <button type="submit" disabled={!canSubmit} className="sd-register-btn mt-1">
        {sending ? "Sending..." : "Send Message"}
      </button>
    </form>
  );
}

function Field(props: { id: string; label: string; children: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-1">
      <label htmlFor={props.id} className="sd-register-label">
        {props.label}
      </label>
      {props.children}
    </div>
  );
}
