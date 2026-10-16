// app/about/credits.ts
export type Credit = {
  name: string;
  href: string;
  role: string;
  /** File name (no extension) in public/logos. Omit to show a letter tile. */
  logo?: string;
};

export type CreditGroup = {
  title: string;
  credits: Credit[];
};

export const CREDIT_GROUPS: CreditGroup[] = [
  {
    title: "FRONTEND",
    credits: [
      { name: "Next.js", href: "https://nextjs.org", role: "App framework", logo: "nextdotjs" },
      { name: "React", href: "https://react.dev", role: "UI library", logo: "react" },
      { name: "TypeScript", href: "https://www.typescriptlang.org", role: "Typed JavaScript", logo: "typescript" },
      { name: "Tailwind CSS", href: "https://tailwindcss.com", role: "Styling", logo: "tailwindcss" },
      { name: "Auth.js", href: "https://authjs.dev", role: "Sign-in sessions" },
      { name: "TanStack Query", href: "https://tanstack.com/query", role: "Data fetching and caching", logo: "reactquery" },
      { name: "date-fns", href: "https://date-fns.org", role: "Date handling", logo: "datefns" },
      { name: "SignalR", href: "https://dotnet.microsoft.com/apps/aspnet/signalr", role: "Live draft updates" },
      { name: "Roboto", href: "https://fonts.google.com/specimen/Roboto", role: "Body typeface", logo: "googlefonts" },
      { name: "Oswald", href: "https://fonts.google.com/specimen/Oswald", role: "Display typeface", logo: "googlefonts" },
    ],
  },
  {
    title: "BACKEND",
    credits: [
      { name: ".NET", href: "https://dotnet.microsoft.com", role: "Runtime and web framework", logo: "dotnet" },
      { name: "FastEndpoints", href: "https://fast-endpoints.com", role: "API endpoints" },
      { name: "Entity Framework Core", href: "https://learn.microsoft.com/ef/core", role: "Writes" },
      { name: "Dapper", href: "https://github.com/DapperLib/Dapper", role: "Reads" },
      { name: "Npgsql", href: "https://www.npgsql.org", role: "PostgreSQL driver" },
      { name: "MediatR", href: "https://github.com/jbogard/MediatR", role: "Commands, queries, events" },
      { name: "MassTransit", href: "https://masstransit.io", role: "Messaging" },
      { name: "Quartz.NET", href: "https://www.quartz-scheduler.net", role: "Background jobs" },
      { name: "DbUp", href: "https://dbup.readthedocs.io", role: "Database migrations" },
      { name: "FluentValidation", href: "https://docs.fluentvalidation.net", role: "Request validation" },
      { name: "Ardalis.SmartEnum", href: "https://github.com/ardalis/SmartEnum", role: "Enumerations" },
      { name: "Polly", href: "https://www.pollydocs.org", role: "Resilience" },
      { name: "MailKit", href: "https://github.com/jstedfast/MailKit", role: "Email" },
      { name: "NSwag", href: "https://github.com/RicoSuter/NSwag", role: "Typed API client" },
      { name: "YARP", href: "https://microsoft.github.io/reverse-proxy", role: "Reverse proxy" },
      { name: "Serilog", href: "https://serilog.net", role: "Logging" },
      { name: "OpenTelemetry", href: "https://opentelemetry.io", role: "Tracing", logo: "opentelemetry" },
    ],
  },
  {
    title: "INFRASTRUCTURE",
    credits: [
      { name: "PostgreSQL", href: "https://www.postgresql.org", role: "Database", logo: "postgresql" },
      { name: "Redis", href: "https://redis.io", role: "Cache", logo: "redis" },
      { name: "RabbitMQ", href: "https://www.rabbitmq.com", role: "Message broker", logo: "rabbitmq" },
      { name: "Keycloak", href: "https://www.keycloak.org", role: "Identity server", logo: "keycloak" },
      { name: "HashiCorp Vault", href: "https://www.vaultproject.io", role: "Secrets", logo: "vault" },
      { name: "Seq", href: "https://datalust.co/seq", role: "Log search" },
      { name: "Caddy", href: "https://caddyserver.com", role: "Web server", logo: "caddy" },
      { name: "Docker", href: "https://www.docker.com", role: "Containers", logo: "docker" },
      { name: "GitHub Actions", href: "https://github.com/features/actions", role: "Build and deploy", logo: "githubactions" },
      { name: "Cloudflare", href: "https://www.cloudflare.com", role: "CDN and storage", logo: "cloudflare" },
      { name: "Hetzner", href: "https://www.hetzner.com", role: "Hosting", logo: "hetzner" },
    ],
  },
  {
    title: "DATA AND SERVICES",
    credits: [
      { name: "TMDb", href: "https://www.themoviedb.org", role: "Film data and images", logo: "themoviedatabase" },
      { name: "OMDb API", href: "https://www.omdbapi.com", role: "Film data fallback" },
      { name: "IGDB", href: "https://www.igdb.com", role: "Game data", logo: "igdb" },
      { name: "Zoom Video SDK", href: "https://developers.zoom.us/docs/video-sdk", role: "Live draft video", logo: "zoom" },
    ],
  },
];
