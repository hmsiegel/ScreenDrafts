'use client';

import { useState, useEffect, useRef } from "react";
import { AdminUserItem } from "@/services/admin/fetch-admin";

interface PasswordResetCardProps {
  accessToken: string | undefined;
  apiBase: string;
}

export default function PasswordResetCard({ accessToken, apiBase }: PasswordResetCardProps) {
  const [search, setSearch] = useState('');
  const [results, setResults] = useState<AdminUserItem[]>([]);
  const [selected, setSelected] = useState<AdminUserItem | null>(null);
  const [searching, setSearching] = useState(false);
  const [sending, setSending] = useState(false);
  const [status, setStatus] = useState<{ type: 'success' | 'error'; message: string } | null>(null);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (!search.trim()) { setResults([]); return; }
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(async () => {
      setSearching(true);
      try {
        const url = new URL(`${apiBase}/admin/users`);
        url.searchParams.set('search', search);
        const res = await fetch(url.toString(), {
          headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
        });
        if (res.ok) setResults(await res.json() as AdminUserItem[]);
      } catch {
        // ignore
      } finally {
        setSearching(false);
      }
    }, 300);
    return () => { if (debounceRef.current) clearTimeout(debounceRef.current); };
  }, [search, accessToken, apiBase]);

  function selectUser(user: AdminUserItem) {
    setSelected(user);
    setSearch('');
    setResults([]);
    setStatus(null);
  }

  async function sendReset() {
    if (!selected || sending) return;
    setSending(true);
    setStatus(null);
    try {
      const res = await fetch(
        `${apiBase}/admin/users/${encodeURIComponent(selected.publicId)}/password-reset`,
        {
          method: 'POST',
          headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
        }
      );
      if (!res.ok) throw new Error(`${res.status}`);
      setStatus({ type: 'success', message: `Reset email sent to ${selected.email}.` });
    } catch {
      setStatus({ type: 'error', message: 'Failed to send reset email. Please try again.' });
    } finally {
      setSending(false);
    }
  }

  return (
    <div className="space-y-5">
      {!selected ? (
        <div className="relative">
          <label className="block font-mono text-[10px] tracking-widest text-sd-ink/50 uppercase mb-1">
            Find User by Name or Email
          </label>
          <input
            type="search"
            value={search}
            onChange={e => { setSearch(e.target.value); setSelected(null); setStatus(null); }}
            placeholder="Search…"
            className="border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full"
          />
          {(results.length > 0 || searching) && (
            <div className="absolute left-0 right-0 top-full mt-1 bg-white border border-sd-ink/20 shadow-lg z-10 max-h-[200px] overflow-y-auto">
              {searching && (
                <p className="px-4 py-2 text-[12px] text-sd-ink/40 italic">Searching…</p>
              )}
              {results.map(user => (
                <button
                  key={user.publicId}
                  onClick={() => selectUser(user)}
                  className="w-full text-left px-4 py-2.5 hover:bg-sd-paper border-b border-sd-ink/5 last:border-0 transition-colors"
                >
                  <p className="text-[13px] font-medium text-sd-ink">{user.displayName}</p>
                  <p className="font-mono text-[11px] text-sd-ink/50">{user.email}</p>
                </button>
              ))}
            </div>
          )}
        </div>
      ) : (
        <div className="space-y-4">
          <div className="flex items-center justify-between p-4 border border-sd-ink/10 bg-sd-paper/50">
            <div>
              <p className="font-medium text-sd-ink">{selected.displayName}</p>
              <p className="font-mono text-[12px] text-sd-ink/50">{selected.email}</p>
            </div>
            <button
              onClick={() => { setSelected(null); setStatus(null); }}
              className="text-[12px] font-mono text-sd-ink/40 hover:text-sd-ink underline"
            >
              change
            </button>
          </div>
          <button
            onClick={sendReset}
            disabled={sending}
            className="bg-sd-red text-white font-oswald tracking-wide uppercase px-4 py-2 hover:bg-sd-red/90 disabled:opacity-50 transition-colors"
          >
            {sending ? 'SENDING…' : 'SEND PASSWORD RESET EMAIL'}
          </button>
        </div>
      )}

      {status && (
        <p className={`text-[13px] ${status.type === 'success' ? 'text-green-700' : 'text-sd-red'}`}>
          {status.message}
        </p>
      )}
    </div>
  );
}
