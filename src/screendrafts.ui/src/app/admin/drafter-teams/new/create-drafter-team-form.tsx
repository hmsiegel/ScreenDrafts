"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { createDrafterTeam, addDrafterToTeam } from "@/services/admin/fetch-admin-drafts";
import { DrafterPicker } from "../drafter-picker";

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";

interface Props {
  accessToken: string;
}

export function CreateDrafterTeamForm({ accessToken }: Props) {
  const router = useRouter();
  const [name, setName] = useState("");
  const [selected, setSelected] = useState<{ publicId: string; displayName: string }[]>([]);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const selectedIds = new Set(selected.map((d) => d.publicId));

  function handleSelectDrafter(drafter: { publicId: string; displayName: string }) {
    setSelected((prev) => [...prev, drafter]);
  }

  function handleRemove(publicId: string) {
    setSelected((prev) => prev.filter((d) => d.publicId !== publicId));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (submitting || !name.trim()) return;
    setSubmitting(true);
    setError(null);
    try {
      const teamId = await createDrafterTeam(accessToken, name.trim());

      // Members are added one at a time after creation — DrafterTeam.Create has no
      // batch-membership path, only AddDrafter one at a time (see AddDrafterToTeamCommand).
      // If one add fails partway through, the team still exists with whichever members
      // succeeded — the edit page is where you'd finish adding the rest.
      for (const drafter of selected) {
        await addDrafterToTeam(accessToken, teamId, drafter.publicId);
      }

      router.push(`/admin/drafter-teams/${encodeURIComponent(teamId)}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create drafter team.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6 max-w-2xl">
      <div>
        <label className={LABEL}>Team Name</label>
        <input
          type="text"
          className={INPUT}
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="e.g. Screen Drafts Legends"
          required
        />
      </div>

      <div>
        <label className={LABEL}>Members</label>

        {selected.length > 0 && (
          <div className="flex flex-wrap gap-1.5 mb-3">
            {selected.map((d) => (
              <span
                key={d.publicId}
                className="inline-flex items-center gap-1 px-2 py-0.5 bg-sd-ink text-white text-[11px] font-mono rounded"
              >
                {d.displayName}
                <button
                  type="button"
                  onClick={() => handleRemove(d.publicId)}
                  className="ml-0.5 hover:text-sd-red leading-none"
                  aria-label="Remove"
                >
                  ×
                </button>
              </span>
            ))}
          </div>
        )}

        <DrafterPicker
          accessToken={accessToken}
          excludeIds={selectedIds}
          onSelect={handleSelectDrafter}
          disabled={submitting}
        />
      </div>

      {error && (
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
          {error}
        </div>
      )}

      <button type="submit" disabled={submitting || !name.trim()} className={BTN_PRIMARY}>
        {submitting ? "Creating…" : "Create Team"}
      </button>
    </form>
  );
}