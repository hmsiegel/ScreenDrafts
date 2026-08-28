"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import {
  updateDrafterTeamName,
  addDrafterToTeam,
  removeDrafterFromTeam,
  type DrafterTeamDetail,
} from "@/services/admin/fetch-admin-drafts";
import { DrafterPicker } from "../drafter-picker";

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const BTN_SECONDARY =
  "border border-sd-ink/20 text-sd-ink font-sans text-sm px-4 py-2 hover:bg-sd-ink/5 disabled:opacity-50 transition-colors rounded";

interface Props {
  team: DrafterTeamDetail;
  accessToken: string;
}

export function EditDrafterTeamForm({ team, accessToken }: Props) {
  const router = useRouter();

  const [name, setName] = useState(team.name);
  const [members, setMembers] = useState(team.members);
  const [renaming, setRenaming] = useState(false);
  const [renameError, setRenameError] = useState<string | null>(null);
  const [pendingMemberId, setPendingMemberId] = useState<string | null>(null);
  const [memberError, setMemberError] = useState<string | null>(null);

  const nameChanged = name.trim() !== team.name && name.trim().length > 0;
  const memberIds = new Set(members.map((m) => m.publicId));

  async function handleRename(e: React.FormEvent) {
    e.preventDefault();
    if (renaming || !nameChanged) return;
    setRenaming(true);
    setRenameError(null);
    try {
      await updateDrafterTeamName(accessToken, team.publicId, name.trim());
      router.refresh();
    } catch (err) {
      setRenameError(err instanceof Error ? err.message : "Failed to rename team.");
    } finally {
      setRenaming(false);
    }
  }

  async function handleAddMember(drafter: { publicId: string; displayName: string }) {
    if (pendingMemberId) return;
    setPendingMemberId(drafter.publicId);
    setMemberError(null);
    try {
      await addDrafterToTeam(accessToken, team.publicId, drafter.publicId);
      setMembers((prev) => [...prev, drafter]);
    } catch (err) {
      setMemberError(err instanceof Error ? err.message : "Failed to add member.");
    } finally {
      setPendingMemberId(null);
    }
  }

  async function handleRemoveMember(drafterId: string) {
    if (pendingMemberId) return;
    setPendingMemberId(drafterId);
    setMemberError(null);
    try {
      await removeDrafterFromTeam(accessToken, team.publicId, drafterId);
      setMembers((prev) => prev.filter((m) => m.publicId !== drafterId));
    } catch (err) {
      setMemberError(err instanceof Error ? err.message : "Failed to remove member.");
    } finally {
      setPendingMemberId(null);
    }
  }

  return (
    <div className="space-y-8 max-w-2xl">
      <form onSubmit={handleRename} className="space-y-3">
        <label className={LABEL}>Team Name</label>
        <div className="flex items-center gap-3">
          <input
            type="text"
            className={INPUT}
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
          />
          <button
            type="submit"
            disabled={renaming || !nameChanged}
            className={`${BTN_SECONDARY} shrink-0 whitespace-nowrap`}
          >
            {renaming ? "Saving…" : "Rename"}
          </button>
        </div>
        {renameError && (
          <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
            {renameError}
          </div>
        )}
      </form>

      <div>
        <label className={LABEL}>Members ({members.length})</label>

        {members.length > 0 && (
          <div className="flex flex-wrap gap-1.5 mb-3">
            {members.map((m) => (
              <span
                key={m.publicId}
                className="inline-flex items-center gap-1 px-2 py-0.5 bg-sd-ink text-white text-[11px] font-mono rounded"
              >
                {m.displayName}
                <button
                  type="button"
                  onClick={() => handleRemoveMember(m.publicId)}
                  disabled={pendingMemberId === m.publicId}
                  className="ml-0.5 hover:text-sd-red leading-none disabled:opacity-40"
                  aria-label={`Remove ${m.displayName}`}
                >
                  ×
                </button>
              </span>
            ))}
          </div>
        )}

        {memberError && (
          <div className="mb-3 border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
            {memberError}
          </div>
        )}

        <DrafterPicker
          accessToken={accessToken}
          excludeIds={memberIds}
          onSelect={handleAddMember}
          disabled={pendingMemberId !== null}
        />
      </div>
    </div>
  );
}