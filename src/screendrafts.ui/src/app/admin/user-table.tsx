'use client';
// app/admin/user-table.tsx

import { useState, useEffect, useRef } from "react";
import { AdminUserItem, AdminRoleItem, PagedResult, fetchAdminUsers } from "@/services/admin/fetch-admin";
import ManageRolesPanel from "./manage-roles-panel";

interface UserTableProps {
  initialData: PagedResult<AdminUserItem>;
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

function Pagination({
  page, totalPages, onPage,
}: {
  page: number;
  totalPages: number;
  onPage: (p: number) => void;
}) {
  if (totalPages <= 1) return null;
  return (
    <div className="flex items-center justify-between pt-3">
      <button
        onClick={() => onPage(page - 1)}
        disabled={page <= 1}
        className="min-h-9 sm:min-h-0 font-mono text-[11px] tracking-widest uppercase px-3 py-1.5 border border-sd-ink/20 text-sd-ink disabled:opacity-30 hover:bg-sd-paper transition-colors"
      >
        ← Prev
      </button>
      <span className="font-mono text-[11px] text-sd-ink/50">
        Page {page} of {totalPages}
      </span>
      <button
        onClick={() => onPage(page + 1)}
        disabled={page >= totalPages}
        className="min-h-9 sm:min-h-0 font-mono text-[11px] tracking-widest uppercase px-3 py-1.5 border border-sd-ink/20 text-sd-ink disabled:opacity-30 hover:bg-sd-paper transition-colors"
      >
        Next →
      </button>
    </div>
  );
}

const PAGE_SIZE = 25;

export default function UserTable({ initialData, allRoles, accessToken, apiBase }: UserTableProps) {
  const [data, setData] = useState<PagedResult<AdminUserItem>>(initialData);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [managingUser, setManagingUser] = useState<AdminUserItem | null>(null);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Re-fetch whenever search or page changes
  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    const delay = search !== '' ? 300 : 0;
    debounceRef.current = setTimeout(async () => {
      setLoading(true);
      try {
        const result = await fetchAdminUsers(accessToken, search || undefined, page, PAGE_SIZE);
        setData(result);
      } catch {
        // keep current data on error
      } finally {
        setLoading(false);
      }
    }, delay);
    return () => { if (debounceRef.current) clearTimeout(debounceRef.current); };
  }, [search, page, accessToken]);

  // Reset to page 1 when search changes
  function handleSearch(value: string) {
    setSearch(value);
    setPage(1);
  }

  function handleRolesChanged(userId: string, newRoles: string[]) {
    setData(prev => ({
      ...prev,
      items: prev.items.map(u => u.publicId === userId ? { ...u, roles: newRoles } : u),
    }));
  }

  return (
    <>
      {/* Toolbar */}
      <div className="flex flex-col-reverse items-stretch gap-2 sm:flex-row sm:items-center sm:justify-between mb-3">
        <p className="font-mono text-[10px] tracking-widest text-sd-ink/50 uppercase">
          {loading ? 'Loading…' : `${data.totalCount} users`}
        </p>
        <input
          type="search"
          value={search}
          onChange={e => handleSearch(e.target.value)}
          placeholder="Search users…"
          className="border border-sd-ink/20 bg-sd-paper px-3 py-1.5 text-sd-ink text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full sm:w-[220px]"
        />
      </div>

      {/* Table from sm up. Below sm each user is a card — name, email, role badges,
          then the button — using the same markup with switched display types. */}
      <div className="border border-sd-ink/20 sm:overflow-x-auto">
        <table className="block sm:table w-full text-sm">
          <thead className="hidden sm:table-header-group">
            <tr className="bg-sd-ink text-white">
              <th className="text-left px-4 py-3 font-mono text-[10px] tracking-widest uppercase">Display Name</th>
              <th className="text-left px-4 py-3 font-mono text-[10px] tracking-widest uppercase">Email</th>
              <th className="text-left px-4 py-3 font-mono text-[10px] tracking-widest uppercase">Roles</th>
              <th className="px-4 py-3 font-mono text-[10px] tracking-widest uppercase text-right">Actions</th>
            </tr>
          </thead>
          <tbody className={`block sm:table-row-group ${loading ? 'opacity-50' : ''}`}>
            {data.items.length === 0 ? (
              <tr className="block sm:table-row">
                <td colSpan={4} className="block sm:table-cell px-4 py-6 text-center text-sd-ink/40 text-[13px] italic">
                  No users found.
                </td>
              </tr>
            ) : (
              data.items.map((user, i) => (
                <tr
                  key={user.publicId}
                  className={`flex flex-col gap-1.5 px-4 py-3 sm:table-row sm:p-0 border-t first:border-t-0 sm:first:border-t border-sd-ink/10 hover:bg-sd-paper transition-colors ${i % 2 === 1 ? 'bg-sd-ink/[0.02]' : 'bg-white'}`}
                >
                  <td className="sm:table-cell sm:px-4 sm:py-3 font-medium text-sd-ink [overflow-wrap:anywhere]">{user.displayName}</td>
                  <td className="sm:table-cell sm:px-4 sm:py-3 font-mono text-[12px] text-sd-ink/70 break-all">{user.email}</td>
                  <td className="sm:table-cell sm:px-4 sm:py-3">
                    <div className="flex flex-wrap gap-1">
                      {user.roles.map(r => <RoleBadge key={r} role={r} />)}
                    </div>
                  </td>
                  <td className="sm:table-cell sm:px-4 sm:py-3 sm:text-right pt-1">
                    <button
                      onClick={() => setManagingUser(user)}
                      className="min-h-9 sm:min-h-0 whitespace-nowrap border border-sd-ink text-sd-ink font-oswald tracking-wide uppercase text-[11px] px-3 py-1.5 hover:bg-sd-ink hover:text-sd-paper transition-colors"
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

      <Pagination
        page={page}
        totalPages={data.totalPages}
        onPage={setPage}
      />

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