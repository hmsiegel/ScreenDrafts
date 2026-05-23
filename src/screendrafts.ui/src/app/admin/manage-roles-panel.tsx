'use client';

import { useState } from "react";
import { AdminUserItem, AdminRoleItem } from "@/services/admin/fetch-admin";

interface ManageRolesPanelProps {
  user: AdminUserItem;
  allRoles: AdminRoleItem[];
  accessToken: string | undefined;
  apiBase: string;
  onClose: () => void;
  onRolesChanged: (userId: string, roles: string[]) => void;
}

export default function ManageRolesPanel({
  user,
  allRoles,
  accessToken,
  apiBase,
  onClose,
  onRolesChanged,
}: ManageRolesPanelProps) {
  const [roles, setRoles] = useState<string[]>(user.roles);
  const [selectedRole, setSelectedRole] = useState('');
  const [toast, setToast] = useState<{ type: 'success' | 'error'; message: string } | null>(null);
  const [busy, setBusy] = useState(false);

  const available = allRoles.filter(r => !roles.includes(r.name));
  const headers = accessToken ? { Authorization: `Bearer ${accessToken}` } : {};

  function showToast(type: 'success' | 'error', message: string) {
    setToast({ type, message });
    setTimeout(() => setToast(null), 3000);
  }

  async function addRole() {
    if (!selectedRole || busy) return;
    const prev = roles;
    setRoles(r => [...r, selectedRole]);
    setSelectedRole('');
    setBusy(true);
    try {
      const res = await fetch(
        `${apiBase}/admin/roles/${encodeURIComponent(selectedRole)}/users/${encodeURIComponent(user.publicId)}`,
        { method: 'POST', headers }
      );
      if (!res.ok) throw new Error(`${res.status}`);
      onRolesChanged(user.publicId, [...prev, selectedRole]);
      showToast('success', `Added role "${selectedRole}".`);
    } catch {
      setRoles(prev);
      showToast('error', `Failed to add role "${selectedRole}".`);
    } finally {
      setBusy(false);
    }
  }

  async function removeRole(roleName: string) {
    if (busy) return;
    const prev = roles;
    setRoles(r => r.filter(x => x !== roleName));
    setBusy(true);
    try {
      const res = await fetch(
        `${apiBase}/admin/roles/${encodeURIComponent(roleName)}/users/${encodeURIComponent(user.publicId)}`,
        { method: 'DELETE', headers }
      );
      if (!res.ok) throw new Error(`${res.status}`);
      onRolesChanged(user.publicId, prev.filter(x => x !== roleName));
      showToast('success', `Removed role "${roleName}".`);
    } catch {
      setRoles(prev);
      showToast('error', `Failed to remove role "${roleName}".`);
    } finally {
      setBusy(false);
    }
  }

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-sd-ink/30 z-40"
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Panel */}
      <aside className="fixed right-0 top-0 bottom-0 w-[420px] bg-sd-paper border-l border-sd-ink z-50 flex flex-col shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-5 border-b border-sd-ink/10 bg-sd-ink">
          <div>
            <p className="font-oswald font-bold text-[18px] text-white leading-tight">{user.displayName}</p>
            <p className="font-mono text-[11px] text-light-blue mt-0.5">{user.email}</p>
          </div>
          <button
            onClick={onClose}
            className="text-white/60 hover:text-white text-[24px] leading-none"
            aria-label="Close panel"
          >
            ×
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto px-6 py-6 space-y-7">
          {/* Current roles */}
          <section>
            <p className="font-mono text-[10px] tracking-widest text-sd-ink/50 uppercase mb-3">Current Roles</p>
            {roles.length === 0 ? (
              <p className="text-[13px] text-sd-ink/40 italic">No roles assigned.</p>
            ) : (
              <div className="flex flex-wrap gap-2">
                {roles.map(role => (
                  <span
                    key={role}
                    className={`inline-flex items-center gap-1.5 font-mono text-xs px-2.5 py-1 rounded-full text-white ${
                      role.toLowerCase().includes('admin') ? 'bg-sd-red' : 'bg-sd-blue'
                    }`}
                  >
                    {role}
                    <button
                      onClick={() => removeRole(role)}
                      disabled={busy}
                      className="leading-none opacity-70 hover:opacity-100 text-[14px]"
                      aria-label={`Remove ${role}`}
                    >
                      ×
                    </button>
                  </span>
                ))}
              </div>
            )}
          </section>

          {/* Add role */}
          <section>
            <p className="font-mono text-[10px] tracking-widest text-sd-ink/50 uppercase mb-3">Add Role</p>
            <div className="flex gap-2">
              <select
                value={selectedRole}
                onChange={e => setSelectedRole(e.target.value)}
                className="flex-1 border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded"
              >
                <option value="">Select a role…</option>
                {available.map(r => (
                  <option key={r.name} value={r.name}>{r.name}</option>
                ))}
              </select>
              <button
                onClick={addRole}
                disabled={!selectedRole || busy}
                className="bg-sd-blue text-white font-oswald tracking-wide uppercase px-4 py-2 hover:bg-sd-blue/90 disabled:opacity-40 transition-colors rounded"
              >
                ADD
              </button>
            </div>
          </section>
        </div>

        {/* Toast */}
        {toast && (
          <div className={`px-6 py-3 text-[13px] font-mono ${toast.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-50 text-sd-red'}`}>
            {toast.message}
          </div>
        )}
      </aside>
    </>
  );
}
