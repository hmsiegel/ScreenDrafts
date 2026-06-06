import { type DraftPartParticipantInfo } from "@/services/drafts/fetch-draft-parts";

interface ParticipantStatusStripProps {
  participants: DraftPartParticipantInfo[];
}

export default function ParticipantStatusStrip({ participants }: ParticipantStatusStripProps) {
  if (participants.length === 0) return null;
  return (
    <div className="flex flex-wrap gap-3 mt-4">
      {participants.map((p, i) => (
        <div key={i} className="border border-sd-ink/20 bg-white px-3 py-2 flex flex-col gap-1 min-w-[140px]">
          <p className="font-oswald font-bold text-sm uppercase tracking-wide text-sd-ink">{p.name}</p>
          <div className="flex gap-4 font-mono text-xs text-sd-ink/60">
            <span>Vetoes: <span className="text-sd-ink font-medium">{p.remainingVetoes}</span></span>
            <span>Overrides: <span className="text-sd-ink font-medium">{p.remainingOverrides}</span></span>
          </div>
        </div>
      ))}
    </div>
  );
}
