'use client';

import { useState, useEffect, useRef } from "react";
import { AdminUserItem, AdminRoleItem } from "@/services/admin/fetch-admin";
import ManageRolesPanel from "./manage-roles-panel";

interface UserTableProps {
  initialUsers: AdminUserItem[];
  allRoles: AdminRoleItem[];
  accessToken: string | undefined;
  apiBase: string;
}

function RoleBadge({ role }: { role: string }) {
  const isAdmin = role.toLowerCase().includes('admin');
  return (
    <span className={`font-mono text-xs px-2 py-0.5 rounded-full text-white ${isAdmin ? 'bg-sd-red' : 'bg-sd-blue'}`}>
      {role}
    </span>
  );
}

export default function UserTable({ initialUsers, allRoles, accessToken, apiBase }: UserTableProps) {
  const [users, setUsers] = useState<AdminUserItem[]>(initialUsers);
  const [search, setSearch] = useState('');
  const [searching, setSearching] = useState(false);
  const [managingUser, setManagingUser] = useState<AdminUserItem | null>(null);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (!search.trim()) {
      setUsers(initialUsers);
      return;
    }
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(async () => {
      setSearching(true);
      try {
        const url = new URL(`${apiBase}/admin/users`);
        url.searchParams.set('search', search);
        const res = await fetch(url.toString(), {
          headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
        });
        if (res.ok) {
          const data = await res.json() as AdminUserItem[];
          setUsers(data);
        }
      } catch {
        // keep current results on error
      } finally {
        setSearching(false);
      }
    }, 300);

    return () => { if (debounceRef.current) clearTimeout(debounceRef.current); };
  }, [search, initialUsers, accessToken, apiBase]);

  function handleRolesChanged(userId: string, newRoles: string[]) {
    setUsers(prev => prev.map(u => u.publicId === userId ? { ...u, roles: newRoles } : u));
  }

  return (
    <>
      {/* Search */}
      <div className="flex items-center justify-between mb-3">
        <p className="font-mono text-[10px] tracking-widest text-sd-ink/50 uppercase">
          {searching ? 'Searching…' : `${users.length} users`}
        </p>
        <input
          type="search"
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Search users…"
          className="border border-sd-ink/20 bg-sd-paper px-3 py-1.5 text-sd-ink text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-[220px]"
        />
      </div>

      {/* Table */}
      <div className="border border-sd-ink/20 overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-sd-ink text-white">
              <th className="text-left px-4 py-3 font-mono text-[10px] tracking-widest uppercase">Display Name</th>
              <th className="text-left px-4 py-3 font-mono text-[10px] tracking-widest uppercase">Email</th>
              <th className="text-left px-4 py-3 font-mono text-[10px] tracking-widest uppercase">Roles</th>
              <th className="px-4 py-3 font-mono text-[10px] tracking-widest uppercase text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            {users.length === 0 ? (
              <tr>
                <td colSpan={4} className="px-4 py-6 text-center text-sd-ink/40 text-[13px] italic">
                  No users found.
                </td>
              </tr>
            ) : (
              users.map((user, i) => (
                <tr
                  key={user.publicId}
                  className={`border-t border-sd-ink/10 hover:bg-sd-paper transition-colors ${i % 2 === 1 ? 'bg-sd-ink/[0.02]' : 'bg-white'}`}
                >
                  <td className="px-4 py-3 font-medium text-sd-ink">{user.displayName}</td>
                  <td className="px-4 py-3 font-mono text-[12px] text-sd-ink/70">{user.email}</td>
                  <td className="px-4 py-3">
                    <div className="flex flex-wrap gap-1">
                      {user.roles.map(r => <RoleBadge key={r} role={r} />)}
                    </div>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <button
                      onClick={() => setManagingUser(user)}
                      className="border border-sd-ink text-sd-ink font-oswald tracking-wide uppercase text-[11px] px-3 py-1.5 hover:bg-sd-ink hover:text-sd-paper transition-colors"
                    >
                      Manage Roles ›
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {managingUser && (
        <ManageRolesPanel
          user={managingUser}
          allRoles={allRoles}
          accessToken={accessToken}
          apiBase={apiBase}
          onClose={() => setManagingUser(null)}
          onRolesChanged={handleRolesChanged}
        />
      )}
    </>
  );
}
