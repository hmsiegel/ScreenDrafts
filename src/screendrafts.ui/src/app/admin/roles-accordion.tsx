'use client';

import { useState } from "react";
import { AdminRoleItem } from "@/services/admin/fetch-admin";

interface RolesAccordionProps {
  roles: AdminRoleItem[];
  accessToken: string | undefined;
  apiBase: string;
}

interface RoleRowProps {
  role: AdminRoleItem;
  accessToken: string | undefined;
  apiBase: string;
}

function RoleRow({ role, accessToken, apiBase }: RoleRowProps) {
  const [open, setOpen] = useState(false);
  const [permissions, setPermissions] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [loaded, setLoaded] = useState(false);

  async function toggle() {
    if (!open && !loaded) {
      setLoading(true);
      try {
        const res = await fetch(
          `${apiBase}/admin/roles/${encodeURIComponent(role.name)}/permissions`,
          {
            headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
          }
        );
        if (res.ok) {
          const data = await res.json() as { permissions?: string[] };
          setPermissions(data.permissions ?? []);
        }
        setLoaded(true);
      } catch {
        setLoaded(true);
      } finally {
        setLoading(false);
      }
    }
    setOpen(o => !o);
  }

  const isAdmin = role.name.toLowerCase().includes('admin');

  return (
    <div className="border-b border-sd-ink/10 last:border-0">
      <button
        onClick={toggle}
        className="w-full flex items-center justify-between px-4 py-3.5 hover:bg-sd-paper/60 transition-colors text-left"
      >
        <div className="flex items-center gap-3">
          <span className={`font-mono text-xs px-2 py-0.5 rounded-full text-white ${isAdmin ? 'bg-sd-red' : 'bg-sd-blue'}`}>
            {role.name}
          </span>
          {role.description && (
            <span className="text-[12px] text-sd-ink/50">{role.description}</span>
          )}
        </div>
        <span className="text-sd-ink/40 text-[18px] leading-none select-none">{open ? '−' : '+'}</span>
      </button>

      {open && (
        <div className="px-4 pb-4">
          {loading ? (
            <p className="text-[12px] text-sd-ink/40 italic">Loading permissions…</p>
          ) : permissions.length === 0 ? (
            <p className="text-[12px] text-sd-ink/40 italic">No permissions defined for this role.</p>
          ) : (
            <div className="flex flex-wrap gap-1.5">
              {permissions.map(perm => (
                <span key={perm} className="font-mono text-[11px] px-2 py-0.5 bg-sd-ink/5 border border-sd-ink/15 text-sd-ink rounded">
                  {perm}
                </span>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default function RolesAccordion({ roles, accessToken, apiBase }: RolesAccordionProps) {
  if (roles.length === 0) {
    return <p className="text-[13px] text-sd-ink/40 italic">No roles defined.</p>;
  }

  return (
    <div className="border border-sd-ink/15 divide-y divide-sd-ink/10">
      {roles.map(role => (
        <RoleRow key={role.name} role={role} accessToken={accessToken} apiBase={apiBase} />
      ))}
    </div>
  );
}
