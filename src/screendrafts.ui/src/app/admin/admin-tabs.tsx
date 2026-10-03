'use client';
// app/admin/admin-tabs.tsx

import { useState } from "react";

type Tab = "USERS" | "PASSWORD RESET" | "ROLES & PERMISSIONS" | "EMAIL MIGRATION";

const TABS: Tab[] = ["USERS", "PASSWORD RESET", "ROLES & PERMISSIONS", "EMAIL MIGRATION"];

interface Props {
  usersPanel: React.ReactNode;
  passwordPanel: React.ReactNode;
  rolesPanel: React.ReactNode;
  emailMigrationPanel: React.ReactNode;
}

export default function AdminTabs({ usersPanel, passwordPanel, rolesPanel, emailMigrationPanel }: Props) {
  const [active, setActive] = useState<Tab>("USERS");

  return (
    <div>
      {/* Four tabs need ~640px; on phones and small tablets they scroll sideways. */}
      <div className="flex gap-0 border-b border-sd-ink/10 mb-6 overflow-x-auto overscroll-x-contain [scrollbar-width:none] -mx-4 px-4 sm:mx-0 sm:px-0">
        {TABS.map((tab) => (
          <button
            key={tab}
            onClick={() => setActive(tab)}
            className={`shrink-0 whitespace-nowrap font-oswald font-semibold text-sm tracking-widest uppercase px-4 sm:px-5 py-3 transition-colors ${
              active === tab
                ? "text-sd-ink border-b-[3px] border-sd-red"
                : "text-sd-ink/50 hover:text-sd-ink"
            }`}
          >
            {tab}
          </button>
        ))}
      </div>

      {active === "USERS" && usersPanel}
      {active === "PASSWORD RESET" && passwordPanel}
      {active === "ROLES & PERMISSIONS" && rolesPanel}
      {active === "EMAIL MIGRATION" && emailMigrationPanel}
    </div>
  );
}