// app/admin/email-bootstrap-panel.tsx
'use client';

import { useState, useEffect, useRef, useCallback } from "react";
import type { EmailBootstrapTokenResponse, EmailBootstrapCandidateItem } from "@/lib/dto";

interface EmailBootstrapPanelProps {
  accessToken: string | undefined;
  apiBase: string;
}

type Status = { type: 'success' | 'error'; message: string } | null;

interface CandidatesPage {
  items: EmailBootstrapCandidateItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// NOTE: the claim page route below is provisional — confirmed as of the last
// pass as /email-change/claim, but double check it still matches if the
// frontend route changed since.
const CLAIM_PATH = "/email-change/claim";

function buildClaimLink(token: string): string {
  const origin = typeof window !== "undefined" ? window.location.origin : "";
  return `${origin}${CLAIM_PATH}?token=${encodeURIComponent(token)}`;
}

function csvEscape(value: string): string {
  if (/[",\n]/.test(value)) {
    return `"${value.replace(/"/g, '""')}"`;
  }
  return value;
}

function buildCsv(rows: EmailBootstrapTokenResponse[]): string {
  const header = [
    "userPublicId",
    "firstName",
    "lastName",
    "currentEmail",
    "isPatreon",
    "expiresAt",
    "claimLink",
  ];

  const lines = rows.map((r) =>
    [
      r.userPublicId ?? "",
      r.firstName ?? "",
      r.lastName ?? "",
      r.currentEmail ?? "",
      r.isPatreon ? "yes" : "no",
      r.expiresAt ? new Date(r.expiresAt).toISOString() : "",
      r.token ? buildClaimLink(r.token) : "",
    ]
      .map((v) => csvEscape(String(v)))
      .join(",")
  );

  return [header.join(","), ...lines].join("\n");
}

function downloadCsv(rows: EmailBootstrapTokenResponse[], batchLabel: string) {
  const csv = buildCsv(rows);
  const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  const suffix = batchLabel ? `-${batchLabel}` : "";
  a.href = url;
  a.download = `email-bootstrap-tokens${suffix}.csv`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

const PAGE_SIZE = 25;

export default function EmailBootstrapPanel({ accessToken, apiBase }: EmailBootstrapPanelProps) {
  // Candidate list — who still needs migrating
  const [candidates, setCandidates] = useState<CandidatesPage | null>(null);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [loadingCandidates, setLoadingCandidates] = useState(false);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Generate form
  const [batchLabel, setBatchLabel] = useState("");
  const [expiryHours, setExpiryHours] = useState(72);
  const [results, setResults] = useState<EmailBootstrapTokenResponse[] | null>(null);
  const [generating, setGenerating] = useState(false);
  const [status, setStatus] = useState<Status>(null);
  const [copiedToken, setCopiedToken] = useState<string | null>(null);

  const fetchCandidates = useCallback(async () => {
    setLoadingCandidates(true);
    try {
      const url = new URL(`${apiBase}/users/email-change/bootstrap/candidates`);
      if (search) url.searchParams.set("search", search);
      url.searchParams.set("page", String(page));
      url.searchParams.set("pageSize", String(PAGE_SIZE));

      const res = await fetch(url.toString(), {
        headers: {
          ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        },
        cache: "no-store",
      });
      if (!res.ok) throw new Error(`${res.status}`);
      const data = (await res.json()) as Omit<CandidatesPage, "totalPages">;
      setCandidates({ ...data, totalPages: Math.ceil(data.totalCount / data.pageSize) });
    } catch {
      // keep whatever we had on error
    } finally {
      setLoadingCandidates(false);
    }
  }, [apiBase, accessToken, search, page]);

  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    const delay = search !== "" ? 300 : 0;
    debounceRef.current = setTimeout(fetchCandidates, delay);
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search, page]);

  function handleSearch(value: string) {
    setSearch(value);
    setPage(1);
  }

  function toggleSelected(publicId: string) {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(publicId)) next.delete(publicId);
      else next.add(publicId);
      return next;
    });
  }

  function toggleSelectAllVisible() {
    if (!candidates) return;
    const visibleIds = candidates.items.map((c) => c.userPublicId!);
    const allSelected = visibleIds.every((id) => selectedIds.has(id));
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (allSelected) {
        visibleIds.forEach((id) => next.delete(id));
      } else {
        visibleIds.forEach((id) => next.add(id));
      }
      return next;
    });
  }

  async function generate(userPublicIds: string[] | undefined) {
    if (generating) return;
    setGenerating(true);
    setStatus(null);

    try {
      const res = await fetch(`${apiBase}/users/email-change/bootstrap/generate`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        },
        body: JSON.stringify({
          userPublicIds,
          batchLabel: batchLabel || undefined,
          expiryHours,
        }),
      });

      if (!res.ok) throw new Error(`${res.status}`);

      const data = (await res.json()) as EmailBootstrapTokenResponse[];
      setResults(data);
      setStatus({
        type: "success",
        message: `Generated ${data.length} token${data.length === 1 ? "" : "s"}.`,
      });
      setSelectedIds(new Set());
      fetchCandidates();
    } catch {
      setStatus({ type: "error", message: "Failed to generate tokens. Please try again." });
    } finally {
      setGenerating(false);
    }
  }

  async function copyLink(token: string) {
    await navigator.clipboard.writeText(buildClaimLink(token));
    setCopiedToken(token);
    setTimeout(() => setCopiedToken((t) => (t === token ? null : t)), 2000);
  }

  const allVisibleSelected =
    !!candidates && candidates.items.length > 0 && candidates.items.every((c) => selectedIds.has(c.userPublicId!));

  return (
    <div className="space-y-8">
      {/* Candidates list */}
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <p className="font-mono text-[10px] tracking-widest text-sd-ink/50 uppercase">
            {loadingCandidates ? "Loading…" : `${candidates?.totalCount ?? 0} users still need migrating`}
          </p>
          <input
            type="search"
            value={search}
            onChange={(e) => handleSearch(e.target.value)}
            placeholder="Search name or email…"
            className="border border-sd-ink/20 bg-sd-paper px-3 py-1.5 text-sd-ink text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-[220px]"
          />
        </div>

        <div className="border border-sd-ink/20 overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-sd-ink text-white">
                <th className="px-4 py-3 w-[36px]">
                  <input
                    type="checkbox"
                    checked={allVisibleSelected}
                    onChange={toggleSelectAllVisible}
                    aria-label="Select all visible"
                  />
                </th>
                <th className="text-left px-4 py-3 font-mono text-[10px] tracking-widest uppercase">Name</th>
                <th className="text-left px-4 py-3 font-mono text-[10px] tracking-widest uppercase">Current Email</th>
                <th className="text-left px-4 py-3 font-mono text-[10px] tracking-widest uppercase">Patreon</th>
                <th className="text-left px-4 py-3 font-mono text-[10px] tracking-widest uppercase">Token Status</th>
              </tr>
            </thead>
            <tbody className={loadingCandidates ? "opacity-50" : ""}>
              {!candidates || candidates.items.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-4 py-6 text-center text-sd-ink/40 text-[13px] italic">
                    {candidates ? "No users need migrating." : "Loading…"}
                  </td>
                </tr>
              ) : (
                candidates.items.map((c, i) => (
                  <tr
                    key={c.userPublicId}
                    className={`border-t border-sd-ink/10 hover:bg-sd-paper transition-colors ${
                      i % 2 === 1 ? "bg-sd-ink/[0.02]" : "bg-white"
                    }`}
                  >
                    <td className="px-4 py-3">
                      <input
                        type="checkbox"
                        checked={selectedIds.has(c.userPublicId!)}
                        onChange={() => toggleSelected(c.userPublicId!)}
                        aria-label={`Select ${c.firstName} ${c.lastName}`}
                      />
                    </td>
                    <td className="px-4 py-3 font-medium text-sd-ink">
                      {c.firstName} {c.lastName}
                    </td>
                    <td className="px-4 py-3 font-mono text-[12px] text-sd-ink/70">{c.currentEmail}</td>
                    <td className="px-4 py-3">
                      {c.isPatreon ? (
                        <span className="font-mono text-xs px-2 py-0.5 rounded-full text-white bg-sd-red">
                          Patreon
                        </span>
                      ) : (
                        <span className="text-sd-ink/30 text-[12px]">—</span>
                      )}
                    </td>
                    <td className="px-4 py-3 font-mono text-[12px] text-sd-ink/70">
                      {c.hasActiveToken
                        ? `Issued — expires ${c.tokenExpiresAt ? new Date(c.tokenExpiresAt).toLocaleString() : "?"}`
                        : "Not yet generated"}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {candidates && candidates.totalPages > 1 && (
          <div className="flex items-center justify-between pt-1">
            <button
              onClick={() => setPage((p) => p - 1)}
              disabled={page <= 1}
              className="font-mono text-[11px] tracking-widest uppercase px-3 py-1.5 border border-sd-ink/20 text-sd-ink disabled:opacity-30 hover:bg-sd-paper transition-colors"
            >
              ← Prev
            </button>
            <span className="font-mono text-[11px] text-sd-ink/50">
              Page {page} of {candidates.totalPages}
            </span>
            <button
              onClick={() => setPage((p) => p + 1)}
              disabled={page >= candidates.totalPages}
              className="font-mono text-[11px] tracking-widest uppercase px-3 py-1.5 border border-sd-ink/20 text-sd-ink disabled:opacity-30 hover:bg-sd-paper transition-colors"
            >
              Next →
            </button>
          </div>
        )}
      </div>

      {/* Generate form */}
      <div className="space-y-4 border-t border-sd-ink/10 pt-6">
        <div className="flex gap-4">
          <div className="flex-1">
            <label className="block font-mono text-[10px] tracking-widest text-sd-ink/50 uppercase mb-1">
              Batch Label
            </label>
            <input
              type="text"
              value={batchLabel}
              onChange={(e) => setBatchLabel(e.target.value)}
              placeholder="e.g. patreon-2026-09"
              className="w-full border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded"
            />
          </div>
          <div className="w-[140px]">
            <label className="block font-mono text-[10px] tracking-widest text-sd-ink/50 uppercase mb-1">
              Expiry (hours)
            </label>
            <input
              type="number"
              min={1}
              value={expiryHours}
              onChange={(e) => setExpiryHours(Number(e.target.value))}
              className="w-full border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded"
            />
          </div>
        </div>

        <div className="flex gap-3">
          <button
            onClick={() => generate(Array.from(selectedIds))}
            disabled={generating || selectedIds.size === 0}
            className="bg-sd-red text-white font-oswald tracking-wide uppercase px-4 py-2 hover:bg-sd-red/90 disabled:opacity-50 transition-colors"
          >
            {generating ? "GENERATING…" : `GENERATE FOR SELECTED (${selectedIds.size})`}
          </button>
          <button
            onClick={() => generate(undefined)}
            disabled={generating}
            className="border border-sd-ink text-sd-ink font-oswald tracking-wide uppercase px-4 py-2 hover:bg-sd-ink hover:text-sd-paper disabled:opacity-50 transition-colors"
          >
            {generating ? "GENERATING…" : "GENERATE FOR ALL UNCLAIMED"}
          </button>
        </div>

        {status && (
          <p className={`text-[13px] ${status.type === "success" ? "text-green-700" : "text-sd-red"}`}>
            {status.message}
          </p>
        )}
      </div>

      {/* Results */}
      {results && results.length > 0 && (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <p className="font-mono text-[10px] tracking-widest text-sd-ink/50 uppercase">
              {results.length} token{results.length === 1 ? "" : "s"}
            </p>
            <button
              onClick={() => downloadCsv(results, batchLabel)}
              className="border border-sd-ink text-sd-ink font-oswald tracking-wide uppercase text-[11px] px-3 py-1.5 hover:bg-sd-ink hover:text-sd-paper transition-colors"
            >
              Download CSV
            </button>
          </div>

          <div className="border border-sd-ink/20 overflow-x-auto max-h-[500px] overflow-y-auto">
            <table className="w-full text-sm">
              <thead className="sticky top-0">
                <tr className="bg-sd-ink text-white">
                  <th className="text-left px-4 py-3 font-mono text-[10px] tracking-widest uppercase">Name</th>
                  <th className="text-left px-4 py-3 font-mono text-[10px] tracking-widest uppercase">Current Email</th>
                  <th className="text-left px-4 py-3 font-mono text-[10px] tracking-widest uppercase">Patreon</th>
                  <th className="text-left px-4 py-3 font-mono text-[10px] tracking-widest uppercase">Expires</th>
                  <th className="px-4 py-3 font-mono text-[10px] tracking-widest uppercase text-right">Link</th>
                </tr>
              </thead>
              <tbody>
                {results.map((r, i) => (
                  <tr
                    key={r.userPublicId ?? i}
                    className={`border-t border-sd-ink/10 ${i % 2 === 1 ? "bg-sd-ink/[0.02]" : "bg-white"}`}
                  >
                    <td className="px-4 py-3 font-medium text-sd-ink">
                      {r.firstName} {r.lastName}
                    </td>
                    <td className="px-4 py-3 font-mono text-[12px] text-sd-ink/70">{r.currentEmail}</td>
                    <td className="px-4 py-3">
                      {r.isPatreon ? (
                        <span className="font-mono text-xs px-2 py-0.5 rounded-full text-white bg-sd-red">
                          Patreon
                        </span>
                      ) : (
                        <span className="text-sd-ink/30 text-[12px]">—</span>
                      )}
                    </td>
                    <td className="px-4 py-3 font-mono text-[12px] text-sd-ink/70">
                      {r.expiresAt ? new Date(r.expiresAt).toLocaleString() : "—"}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <button
                        onClick={() => r.token && copyLink(r.token)}
                        className="font-mono text-[11px] text-sd-blue hover:underline"
                      >
                        {copiedToken === r.token ? "Copied!" : "Copy link"}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}