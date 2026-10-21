'use client';
// app/admin/role-filter.tsx

import { useEffect, useRef, useState } from "react";
import { AdminRoleItem } from "@/services/admin/fetch-admin";

interface RoleFilterProps {
  allRoles: AdminRoleItem[];
  selected: string[];
  onChange: (roles: string[]) => void;
}

export default function RoleFilter({ allRoles, selected, onChange }: RoleFilterProps) {
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement | null>(null);

  // Close on outside click or Escape.
  useEffect(() => {
    if (!open) return;
    function onPointerDown(e: PointerEvent) {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) setOpen(false);
    }
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === 'Escape') setOpen(false);
    }
    document.addEventListener('pointerdown', onPointerDown);
    document.addEventListener('keydown', onKeyDown);
    return () => {
      document.removeEventListener('pointerdown', onPointerDown);
      document.removeEventListener('keydown', onKeyDown);
    };
  }, [open]);

  function toggle(name: string) {
    onChange(selected.includes(name) ? selected.filter(r => r !== name) : [...selected, name]);
  }

  const label = selected.length === 0 ? 'All roles' : `Roles (${selected.length})`;

  return (
    <div ref={rootRef} className="relative w-full sm:w-auto">
      <button
        type="button"
        onClick={() => setOpen(o => !o)}
        aria-haspopup="listbox"
        aria-expanded={open}
        className="min-h-9 sm:min-h-0 w-full sm:w-[160px] flex items-center justify-between gap-2 border border-sd-ink/20 bg-sd-paper px-3 py-1.5 text-sd-ink text-sm rounded-sm focus:outline-hidden focus:ring-2 focus:ring-sd-blue"
      >
        <span className="truncate">{label}</span>
        <span aria-hidden="true" className="text-sd-ink/50 text-[10px]">▾</span>
      </button>

      {open && (
        <div className="absolute left-0 right-0 sm:left-auto sm:right-0 sm:w-[220px] top-full mt-1 z-10 bg-white border border-sd-ink/20 shadow-lg">
          <ul role="listbox" aria-multiselectable="true" className="max-h-[260px] overflow-y-auto py-1">
            {allRoles.map(role => (
              <li key={role.name} role="option" aria-selected={selected.includes(role.name)}>
                <label className="flex items-center gap-3 px-3 py-2 min-h-9 text-sm text-sd-ink cursor-pointer hover:bg-sd-paper">
                  <input
                    type="checkbox"
                    checked={selected.includes(role.name)}
                    onChange={() => toggle(role.name)}
                  />
                  {role.name}
                </label>
              </li>
            ))}
          </ul>
          <button
            type="button"
            onClick={() => onChange([])}
            disabled={selected.length === 0}
            className="w-full text-left px-3 py-2 border-t border-sd-ink/10 font-mono text-[10px] tracking-widest uppercase text-sd-ink/60 hover:text-sd-ink disabled:opacity-30"
          >
            Clear
          </button>
        </div>
      )}
    </div>
  );
}
