# Implementation Plan (Temporary)

Goal: Add a reusable Common project (Postgres + EF Core migrations) and two minimal MVC web apps (Game1, Game2) with Tailwind and optional htmx, surfaced at /game1 and /game2, modeled after TodoWeb.

## 1) Solution layout

- MSCoffee.Common (Class Library)
  - Purpose: Shared DbContext, entities, configuration, migration utilities.
- MSCoffee.Game1 (ASP.NET Core MVC)
  - Purpose: Minimal Hello World UI with Tailwind/htmx.
- MSCoffee.Game2 (ASP.NET Core MVC)
  - Purpose: Minimal Hello World UI with Tailwind/htmx.
- AppHost wiring
  - Reference new projects, provide Postgres resource and connection string, health checks.
- Tailwind build
  - Reuse existing root Tailwind/PostCSS toolchain; include Game1/2 view paths in tailwind.config.js.

Notes on routing (/game1, /game2):
- Easiest for dev: run Game1 and Game2 on separate ports (default) and link from dashboard.
- To serve both under single origin paths /game1 and /game2:
  - Add a lightweight Gateway (YARP) project to reverse-proxy: /game1 -> Game1, /game2 -> Game2, or
  - Set PathBase (ASPNETCORE_PATHBASE) for each app and front with a gateway or reverse proxy. This plan includes a Gateway as an optional step (section 7).

## 2) Common project (Postgres + EF Core)

- Create project: class library targeting net9.0.
- Packages (NuGet):
  - Npgsql.EntityFrameworkCore.PostgreSQL
  - Microsoft.EntityFrameworkCore.Design
  - Microsoft.Extensions.Options.ConfigurationExtensions (if needed)
- Contents:
  - DbContext (e.g., CoffeeDbContext) with one seed entity (e.g., MigrationHistory or SampleEntity) to validate pipeline.
  - Entity type configuration files.
  - IServiceCollection extension: AddCommonData(this IServiceCollection, IConfiguration) that:
    - Reads connection string ("Postgres" or "DefaultConnection").
    - Registers DbContext with Npgsql and sensible defaults (EnableRetryOnFailure, snake_case, etc.).
  - IDesignTimeDbContextFactory<CoffeeDbContext> to make migrations work from the library.
  - Optional: a small migrator helper (ApplyMigrationsAsync) to run db.Database.Migrate() on app start for consuming apps.

- Migrations strategy:
  - Store migrations in MSCoffee.Common (simplest) using the design-time factory.
  - Migration commands will target the Common project with a startup project to resolve dependencies (AppHost or a small temporary console) — see section 6.

## 3) Postgres resource in AppHost

- Add a Postgres container/resource in AppHost using Aspire Hosting APIs.
  - Expose connection string via binding and reference it from Game1/Game2.
  - Optionally configure volume for persistent data in dev.
- Emit connection string to dependent services via environment (ConnectionStrings:Postgres) or similar.
- Add a health check for the Postgres endpoint.

## 4) Game1 and Game2 web apps

- Create two MVC projects: MSCoffee.Game1, MSCoffee.Game2 (net9.0).
- Reference: MSCoffee.ServiceDefaults, MSCoffee.Common.
- Register Common DbContext via AddCommonData(Configuration).
- Minimal UI:
  - Layout imports Tailwind CSS (from built artifact) and optionally htmx via CDN.
  - HomeController with Index action returning a view with a Tailwind-styled “Hello from Game 1/2” banner.
- Routing:
  - Conventional MVC routing.
  - If using a Gateway: apps can remain at “/”; Gateway maps /game1 and /game2.
  - If not using a Gateway: set ASPNETCORE_PATHBASE=/game1 (Game1) and /game2 (Game2) via AppHost env; ensure app.UsePathBase.

## 5) Tailwind/htmx integration

- Update existing tailwind.config.js content paths to include:
  - src/Game1/Views/**/*.cshtml
  - src/Game2/Views/**/*.cshtml
- Ensure CSS build picks up Game1/2.
- htmx (optional): include via CDN script tag in _Layout.

## 6) EF Core migrations workflow

- Ensure MSCoffee.Common has:
  - DbContext, design-time factory, and an appsettings.Development.json (optional) with a placeholder connection string for design-time.
- Commands (reference, adjust paths as needed):
  - Add migration (startup = Game1 or AppHost):
    - dotnet ef migrations add InitialCreate -p MSCoffee.Common -s src/Game1 -o Migrations
  - Update database (requires Postgres running via Aspire or external instance):
    - dotnet ef database update -p MSCoffee.Common -s src/Game1
- On app start (Game1/Game2), call migrator helper to apply pending migrations automatically in dev. Guard with environment check (Development).

## 7) Optional: Gateway (path-based routing)

- Create MSCoffee.Gateway (ASP.NET Core with YARP):
  - Map /game1 -> Game1 service HTTP endpoint
  - Map /game2 -> Game2 service HTTP endpoint
  - Serve a simple landing page linking to both.
- AppHost: add gateway project, depend on Game1 and Game2, configure clusters from service discovery.

## 8) AppHost wiring

- Add projects to AppHost with WithReference to Postgres resource (and optionally Gateway).
- Set environment for PathBase if skipping Gateway.
- Define HTTP health checks for each app.

## 9) Solution and project references

- Add all new projects to MSCoffee.sln.
- For Game1/Game2, add project references to:
  - MSCoffee.ServiceDefaults
  - MSCoffee.Common

## 10) Verification checklist

- Build succeeds for all projects.
- AppHost starts; Postgres container reachable; dashboard links visible.
- Database created; migrations applied (check via psql or logs).
- Game1 shows Hello page; Game2 shows Hello page.
- Tailwind styles render correctly; htmx script loads (no logic required now).
- If Gateway enabled: /game1 and /game2 paths work from a single origin.

## 11) Follow-ups (later)

- Add proper entities and repositories into Common.
- Seed data and integration tests targeting Testcontainers for Postgres.
- CI task for running migrations and smoke tests.
- Observability: add OpenTelemetry exporters if desired (Aspire friendly).
