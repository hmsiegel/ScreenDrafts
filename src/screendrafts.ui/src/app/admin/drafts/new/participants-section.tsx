export function ParticipantsSection({
  drafterList,
  drafterTeamList,
  selectedDrafterIds,
  selectedTeamIds,
  drafterSearch,
  teamSearch,
  participantTab,
  onToggleDrafter,
  onToggleTeam,
  onDrafterSearch,
  onTeamSearch,
  onTabChange,
}: {
  drafterList: { publicId: string; displayName: string; isRetired: boolean }[];
  drafterTeamList: { publicId: string; name: string; numberOfDrafters: number }[];
  selectedDrafterIds: Set<string>;
  selectedTeamIds: Set<string>;
  drafterSearch: string;
  teamSearch: string;
  participantTab: "drafter" | "team";
  onToggleDrafter: (id: string) => void;
  onToggleTeam: (id: string) => void;
  onDrafterSearch: (v: string) => void;
  onTeamSearch: (v: string) => void;
  onTabChange: (tab: "drafter" | "team") => void;
}) {
  const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
  const INPUT =
    "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
  const SECTION_HEADING =
    "font-oswald font-bold text-[18px] tracking-wide uppercase text-sd-ink mb-4 pb-2 border-b border-sd-ink/10";

  const filteredDrafters = drafterList.filter((d) =>
    d.displayName.toLowerCase().includes(drafterSearch.toLowerCase())
  );
  const filteredTeams = drafterTeamList.filter((t) =>
    t.name.toLowerCase().includes(teamSearch.toLowerCase())
  );

  const totalSelected = selectedDrafterIds.size + selectedTeamIds.size;

  return (
    <section>
      <h2 className={SECTION_HEADING}>Participants (Optional)</h2>

      {/* Tab toggle */}
      <div className="flex gap-0 mb-4 border border-sd-ink/20 rounded overflow-hidden w-fit">
        <button
          type="button"
          onClick={() => onTabChange("drafter")}
          className={`px-4 py-2 text-sm font-mono tracking-wide uppercase transition-colors ${
            participantTab === "drafter"
              ? "bg-sd-ink text-white"
              : "bg-white text-sd-ink/60 hover:bg-sd-ink/5"
          }`}
        >
          Individual
        </button>
        <button
          type="button"
          onClick={() => onTabChange("team")}
          className={`px-4 py-2 text-sm font-mono tracking-wide uppercase transition-colors border-l border-sd-ink/20 ${
            participantTab === "team"
              ? "bg-sd-ink text-white"
              : "bg-white text-sd-ink/60 hover:bg-sd-ink/5"
          }`}
        >
          Team
        </button>
      </div>

      <div className="border border-sd-ink/10 rounded p-4 bg-white">
        {participantTab === "drafter" ? (
          <>
            <input
              type="text"
              placeholder="Search drafters…"
              className={`${INPUT} mb-3`}
              value={drafterSearch}
              onChange={(e) => onDrafterSearch(e.target.value)}
            />
            <div className="max-h-48 overflow-y-auto space-y-1">
              {filteredDrafters.length === 0 ? (
                <p className="text-sm text-sd-ink/40 font-mono">No drafters found.</p>
              ) : (
                filteredDrafters.map((d) => (
                  <label
                    key={d.publicId}
                    className="flex items-center gap-2 px-3 py-1.5 text-sm text-sd-ink hover:bg-sd-ink/5 rounded cursor-pointer"
                  >
                    <input
                      type="checkbox"
                      checked={selectedDrafterIds.has(d.publicId)}
                      onChange={() => onToggleDrafter(d.publicId)}
                      className="accent-sd-red"
                    />
                    <span className={d.isRetired ? "text-sd-ink/40 line-through" : ""}>
                      {d.displayName}
                    </span>
                    {d.isRetired && (
                      <span className="text-[10px] font-mono text-sd-ink/40 uppercase tracking-wide">
                        retired
                      </span>
                    )}
                  </label>
                ))
              )}
            </div>
            {selectedDrafterIds.size > 0 && (
              <p className="mt-2 text-[11px] font-mono text-sd-ink/50">
                {selectedDrafterIds.size} drafter
                {selectedDrafterIds.size !== 1 ? "s" : ""} selected
              </p>
            )}
          </>
        ) : (
          <>
            <input
              type="text"
              placeholder="Search teams…"
              className={`${INPUT} mb-3`}
              value={teamSearch}
              onChange={(e) => onTeamSearch(e.target.value)}
            />
            <div className="max-h-48 overflow-y-auto space-y-1">
              {filteredTeams.length === 0 ? (
                <p className="text-sm text-sd-ink/40 font-mono">No teams found.</p>
              ) : (
                filteredTeams.map((t) => (
                  <label
                    key={t.publicId}
                    className="flex items-center gap-2 px-3 py-1.5 text-sm text-sd-ink hover:bg-sd-ink/5 rounded cursor-pointer"
                  >
                    <input
                      type="checkbox"
                      checked={selectedTeamIds.has(t.publicId)}
                      onChange={() => onToggleTeam(t.publicId)}
                      className="accent-sd-red"
                    />
                    <span>{t.name}</span>
                    <span className="text-[11px] font-mono text-sd-ink/40">
                      {t.numberOfDrafters} member
                      {t.numberOfDrafters !== 1 ? "s" : ""}
                    </span>
                  </label>
                ))
              )}
            </div>
            {selectedTeamIds.size > 0 && (
              <p className="mt-2 text-[11px] font-mono text-sd-ink/50">
                {selectedTeamIds.size} team{selectedTeamIds.size !== 1 ? "s" : ""} selected
              </p>
            )}
          </>
        )}
      </div>

      {totalSelected > 0 && (
        <p className="mt-2 text-[11px] font-mono text-sd-ink/50">
          {totalSelected} participant{totalSelected !== 1 ? "s" : ""} total (
          {selectedDrafterIds.size} individual
          {selectedDrafterIds.size !== 1 ? "s" : ""}, {selectedTeamIds.size} team
          {selectedTeamIds.size !== 1 ? "s" : ""})
        </p>
      )}
    </section>
  );
}