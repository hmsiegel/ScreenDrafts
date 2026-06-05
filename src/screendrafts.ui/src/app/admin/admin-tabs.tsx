'use client';

import { useState } from "react";

type Tab = "USERS" | "PASSWORD RESET" | "ROLES & PERMISSIONS";

const TABS: Tab[] = ["USERS", "PASSWORD RESET", "ROLES & PERMISSIONS"];

interface Props {
  usersPanel: React.ReactNode;
  passwordPanel: React.ReactNode;
  rolesPanel: React.ReactNode;
}

export default function AdminTabs({ usersPanel, passwordPanel, rolesPanel }: Props) {
  const [active, setActive] = useState<Tab>("USERS");

  return (
    <div>
      <div className="flex gap-0 border-b border-sd-ink/10 mb-6">
        {TABS.map((tab) => (
          <button
            key={tab}
            onClick={() => setActive(tab)}
            className={`font-oswald font-semibold text-sm tracking-widest uppercase px-5 py-3 transition-colors ${
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
    </div>
  );
}
