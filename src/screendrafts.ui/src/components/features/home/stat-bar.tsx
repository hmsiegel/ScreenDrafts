interface Stat {
  value: string;
  label: string;
}

export default function StatBar({ stats }: { stats: Stat[] }) {
  return (
    <div
      style={{
        background: '#0d1430',
        color: '#fff',
        padding: '32px',
        display: 'grid',
        gridTemplateColumns: `repeat(${stats.length}, 1fr)`,
        gap: 24,
        alignItems: 'center',
        height: '96px'
      }}
    >
      {stats.map((stat) => (
        <div key={stat.label} className="flex flex-col">
          <div className="font-oswald font-bold text-[32px] text-light-blue leading-[1.1] tracking-[0.02em]">
            {stat.value}
          </div>
          <div className="text-[10px] tracking-[0.22em] opacity-70 mt-1">{stat.label}</div>
        </div>
      ))}
    </div>
  );
}
