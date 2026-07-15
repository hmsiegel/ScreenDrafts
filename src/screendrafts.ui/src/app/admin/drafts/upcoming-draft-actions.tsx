'use client';

import { useState } from "react";
import Link from "next/link";
import {
  getDraft,
  startDraftPart,
  deleteDraft,
  restoreDraft,
  type AdminDraftListItem,
} from "@/services/admin/fetch-admin-drafts";

// Super draft type value
const SUPER_DRAFT_TYPE = 3;

interface UpcomingDraftActionsProps {
  draft: AdminDraftListItem;
  accessToken: string;
  onRefresh: () => void | Promise<void>;
}

export default function UpcomingDraftActions({
  draft,
  accessToken,
  onRefresh,
}: UpcomingDraftActionsProps) {
  const [starting, setStarting] = useState(false);
  const [confirmDelete, setConfirmDelete] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [restoring, setRestoring] = useState(false);

  async function handleStart() {
    setStarting(true);
    try {
      const detail = await getDraft(accessToken, draft.publicId);
      const parts = detail?.parts ?? [];
      for (const part of parts) {
        await startDraftPart(accessToken, draft.publicId, part.partIndex);
      }
      await onRefresh();
    } catch (err) {
      console.error("[handleStart]", err);
    } finally {
      setStarting(false);
    }
  }

  async function handleDelete() {
    setDeleting(true);
    try {
      await deleteDraft(accessToken, draft.publicId);
      await onRefresh();
    } catch (err) {
      console.error("[handleDelete]", err);
    } finally {
      setDeleting(false);
      setConfirmDelete(false);
    }
  }

  async function handleRestore() {
    setRestoring(true);
    try {
      await restoreDraft(accessToken, draft.publicId);
      await onRefresh();
    } catch (err) {
      console.error("[handleRestore]", err);
    } finally {
      setRestoring(false);
    }
  }

  const isSuper = draft.draftType === SUPER_DRAFT_TYPE;

  if (draft.isDeleted) {
    return (
      <div className="flex items-center justify-end">
        <button
          type="button"
          onClick={handleRestore}
          disabled={restoring}
          className="text-sd-blue text-sm font-medium hover:underline disabled:opacity-50"
        >
          {restoring ? "Restoring…" : "Restore"}
        </button>
      </div>
    );
  }

  if (confirmDelete) {
    return (
      <div className="flex items-center gap-2 justify-end">
        <span className="text-xs font-mono text-sd-ink/70">Confirm delete?</span>
        <button
          type="button"
          onClick={handleDelete}
          disabled={deleting}
          className="text-xs font-mono text-sd-red hover:underline disabled:opacity-50"
        >
          {deleting ? "Deleting…" : "YES"}
        </button>
        <button
          type="button"
          onClick={() => setConfirmDelete(false)}
          className="text-xs font-mono text-sd-ink/50 hover:underline"
        >
          CANCEL
        </button>
      </div>
    );
  }

  return (
    <div className="flex items-center gap-3 justify-end">
      {isSuper && (
        <Link
          href={`/admin/drafts/${draft.publicId}/pool`}
          className="text-sd-blue text-xs font-mono uppercase tracking-wide hover:underline"
        >
          Manage Pool
        </Link>
      )}
      <Link
        href={`/admin/drafts/${draft.publicId}/attendances`}
        className="text-sd-blue text-xs font-mono uppercase tracking-wide hover:underline"
      >
        Attendance
      </Link>
      <button
        type="button"
        onClick={handleStart}
        disabled={starting}
        className="bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-blue/90 disabled:opacity-50"
      >
        {starting ? "Starting…" : "Start"}
      </button>
      <Link
        href={`/admin/drafts/${draft.publicId}/edit`}
        className="text-sd-blue text-sm font-medium hover:underline"
      >
        Edit
      </Link>
      <button
        type="button"
        onClick={() => setConfirmDelete(true)}
        className="text-sd-ink/40 text-sm hover:text-sd-red"
      >
        Delete
      </button>
    </div>
  );
}