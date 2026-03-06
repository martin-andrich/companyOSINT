# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Description

company-OSINT.com is a B2B web project with a large database of company names and metadata.
Users can register and log in. Through their account, they can filter the database according to various criteria and then purchase records with suitable potential B2B customers.
An MCP server is provided for AI agents, which they can use to filter and purchase records themselves.
The website layout is in a modern SaaS style.

Users without an active subscription can search for companies, but can only see their name and address, without any further metadata.
The web subscription costs EUR 49 per month and allows full web search including all metadata.
The MCP subscription costs EUR 99 per month and allows web search and MCP use including all metadata.
Subscriptions are automatically renewed for one month unless canceled. Cancellation is possible at any time without notice.

## Build & Run Commands

- **Build solution:** `dotnet build companyOSINT.slnx`
- **Run web API:** `dotnet run --project companyOSINT.Web`
- **Run worker:** `dotnet run --project companyOSINT.Worker`
- **Run import:** `dotnet run --project companyOSINT.Import`
- **Docker:** `docker compose up --build`

No test project exists yet. When adding tests, use `dotnet test` at the solution level.

## Architecture

This is a .NET 10 solution (`companyOSINT.slnx`) following Clean Architecture with six projects:

```
companyOSINT.Domain          → (no references — pure C# classes)
companyOSINT.Application     → Domain
companyOSINT.Infrastructure  → Application, Domain
companyOSINT.Web             → Application, Infrastructure
companyOSINT.Worker          → Domain  (communicates via HTTP with Web API)
companyOSINT.Import          → Domain  (entity types only)
```

- **companyOSINT.Domain** — Pure class library with no NuGet packages. Contains all entity classes (`Entities/`), DTOs (`Dtos/`), and shared data structures (`Common/` — pagination types, shared DTOs).
- **companyOSINT.Application** — Service layer with interfaces and service implementations. References `Microsoft.EntityFrameworkCore` for the `IApplicationDbContext` interface (DbSet properties + SaveChangesAsync). No MediatR, no Repository pattern, no AutoMapper — simple service interfaces for CRUD operations.
- **companyOSINT.Infrastructure** — Data access layer. Contains `ApplicationDbContext` (implements `IApplicationDbContext`), uses `Npgsql.EntityFrameworkCore.PostgreSQL`. DI registration via `AddInfrastructure()` extension method.
- **companyOSINT.Web** — ASP.NET Core Web API (OpenAPI enabled). Thin controllers that delegate all logic to Application services. Runs on `http://localhost:8085` in development.
- **companyOSINT.Worker** — .NET Generic Host with two `BackgroundService` workers that enrich companies and websites via the Web API.
- **companyOSINT.Import** — Console app that bulk-imports company + contact data from SQLite (`handelsregister.db`) into PostgreSQL using `COPY` (binary import).

Both Web and Worker have Dockerfiles and are defined as services in `compose.yaml`.

### Key Design Decisions

| Pattern | Why not used |
|---|---|
| MediatR / CQRS | Over-engineering for CRUD-heavy API — service methods are clearer and more debuggable |
| Generic Repository | DbContext is already Unit of Work + Repository — a wrapper adds no value |
| AutoMapper | Manual mappings with ~15 DTOs are clearer and more transparent |
| Domain Events | No cross-aggregate side effects in the codebase |

### Project Structure

**Application layer:**
```
companyOSINT.Application/
  Interfaces/
    IApplicationDbContext.cs    — DbSet properties + SaveChangesAsync
    ICompanyService.cs, IWebsiteService.cs, ISectorService.cs, IDomainToSkipService.cs
  Services/
    CompanyService.cs, WebsiteService.cs, SectorService.cs, DomainToSkipService.cs
  DependencyInjection.cs       — AddApplication() extension method
```

**Infrastructure layer:**
```
companyOSINT.Infrastructure/
  Data/
    ApplicationDbContext.cs     — implements IApplicationDbContext
  DependencyInjection.cs       — AddInfrastructure(connectionString) extension method
```

**Domain layer:**
```
companyOSINT.Domain/
  Entities/
    BaseEntity.cs, Company.cs, Contact.cs, Website.cs,
    Sector.cs, Software.cs, Tool.cs, DomainToSkip.cs
  Common/
    PaginatedResult.cs, CursorPage.cs, CompanyNameDto.cs, CompanyListDto.cs
  Dtos/
    Companies/   — CompanyPatchDto
    Websites/    — WebsiteCreateDto, WebsitePatchDto, SoftwareCreateDto, ToolCreateDto
    Sectors/     — SectorCreateDto
    Domains/     — DomainToSkipCreateDto
```

### Data Model

**Core entities** (all inherit from `BaseEntity` with `Id`, `DateCreated`, `DateModified`):

- **BaseEntity** — `Id` (Guid, `[DatabaseGenerated(None)]` — must be set manually), `DateCreated`, `DateModified` (auto-set by `ApplicationDbContext.SaveChanges` override).

- **Sector** — A standardized industry sector.
  - `Name` — Sector name in German (e.g. "Maschinenbau", "IT-Dienstleistungen & Beratung"). Unique index.
  - ~65 sectors seeded at database creation (based on WZ 2008, practical grouping)
  - New sectors can be created via API when the LLM suggests one not in the list

- **Company** — A registered company with name, address, registrar info, sector, activity.
  - `Name`, `FederalState`, `RegisteredOffice`, `RegisteredAddress`, `Registrar`, `RegisterArt`, `RegisterNummer` — All nullable strings, imported from Handelsregister
  - `SectorId` (nullable FK → Sector, `DeleteBehavior.SetNull`) — Standardized industry classification, set during enrichment
  - `Activity` — Short German description of the company's business, set during enrichment
  - `DateLastChecked` (nullable) — `null` means unprocessed, non-null means the company has been checked for websites
  - Has many `Contact`s (officers/representatives)
  - Has many `Website`s (1:N — a company can have multiple websites)
  - Navigation: `Sector` (nullable reference)

- **Contact** — Officers/representatives linked to a company via `CompanyId` FK.
  - `Name`, `FirstName`, `LastName`, `MaidenName` — Name fields
  - `Title`, `Position`, `Type` — Role information
  - `City`, `Flag` — Location and flags
  - `StartDate`, `EndDate`, `Dismissed` — Strings (imported format, not parsed to DateTime)
  - `ReferenceNo` — Reference number from source data
  - Navigation: `Company`

- **Website** — A website belonging to a company.
  - `UrlWebsite`, `UrlImprint` — URLs found during enrichment
  - `IsSubdomain` — Boolean indicating if the URL is a subdomain
  - `HttpResponseCode`, `IpAddress`, `SslValid`, `ConsentManagerFound`, `RequestsWithoutConsent`, `CookiesWithoutConsent`, `AverageTimeToFirstByte` — Technical metadata (populated by WebsiteEnrichmentWorker)
  - Has many `Software` (1:N — detected CMS/frameworks/platforms)
  - Has many `Tool` (1:N — detected tools/services)
  - `DateLastChecked` (nullable) — When the website's technical metadata was last checked

- **Software** — A detected software/CMS/framework on a website.
  - `Name` — Software name (e.g. "WordPress", "Shopware")
  - `Version` — Detected version string (empty if unknown)
  - `FoundAt` — URL where the software was detected
  - `WebsiteId` (FK) — Parent website

- **Tool** — A detected tool/service on a website.
  - `Name` — Tool name
  - `FoundAt` — URL where the tool was detected
  - `WebsiteId` (FK) — Parent website

- **DomainToSkip** — Blacklisted domain that should be excluded from search results.
  - `Domain` — Domain name (unique index)

### Database Configuration

**Indexes** (defined in `ApplicationDbContext.OnModelCreating`):
- `IX_Companies_DateLastChecked` — For querying unchecked companies
- `IX_Websites_DateLastChecked` — For querying websites needing enrichment
- `IX_Sectors_Name` (unique) — Enforces sector name uniqueness
- `IX_DomainsToSkip_Domain` (unique) — Enforces domain uniqueness
- `IX_Software_WebsiteId`, `IX_Tools_WebsiteId` — FK indexes

**Seed data:**
- 65 German industry sectors with deterministic GUIDs (MD5 hash of `"sector:{name}"`)
- ~40 blacklisted domains (social media, company registries, classifieds) with deterministic GUIDs (MD5 hash of domain name)
- All seed entities use fixed timestamp: `2025-01-01T00:00:00Z`

**Auto-timestamps:**
- `SaveChanges`/`SaveChangesAsync` overridden in `ApplicationDbContext`
- Sets `DateCreated` on Added entities, `DateModified` on Added + Modified entities (UTC)

### Web API Endpoints

Controllers are thin delegators — all business logic lives in `Application/Services/`.

**`CompaniesController` (`/api/companies`) → `ICompanyService`:**

- `GET /api/companies` — Paginated list with filters (name, federalState, registrar, search, sectorId)
- `GET /api/companies/{id}` — Single company with contacts and websites (includes both via `Include`)
- `POST /api/companies` — Create company
- `PUT /api/companies/{id}` — Full update
- `PATCH /api/companies/{id}` — Partial update (accepts `CompanyPatchDto`: `SectorId`, `Activity`, `DateLastChecked`)
- `DELETE /api/companies/{id}` — Delete company
- `GET /api/companies/next-to-check` — Returns first company with `DateLastChecked == null` (filtered to Dresden), or `204 No Content`
- `GET /api/companies/names-to-check` — Cursor-paginated list of companies where `DateLastChecked == null`, projected to `CompanyNameDto` (Id, Name, RegisteredOffice). Used by the Worker's matching cache.

**`SectorsController` (`/api/sectors`) → `ISectorService`:**

- `GET /api/sectors` — List all sectors sorted by name
- `GET /api/sectors/{id}` — Single sector
- `POST /api/sectors` — Create sector (accepts `SectorCreateDto`: `Name`). Returns `409 Conflict` if name exists.
- `DELETE /api/sectors/{id}` — Delete sector (sets `SectorId` to null on affected companies)

**`WebsitesController` (`/api/websites`) → `IWebsiteService`:**

- `GET /api/websites?companyId={id}` — List websites for a company (includes Software + Tools)
- `GET /api/websites/{id}` — Single website (includes Software + Tools)
- `POST /api/websites` — Create website (accepts `WebsiteCreateDto`: `CompanyId`, `UrlWebsite`, `UrlImprint`)
- `GET /api/websites/next-to-enrich` — Returns next website needing enrichment (`DateLastChecked == null` or older than 30 days, `UrlWebsite` must be non-empty, prioritizes null over old dates), or `204 No Content`
- `PATCH /api/websites/{id}` — Partial update (accepts `WebsitePatchDto`: `HttpResponseCode`, `IpAddress`, `SslValid`, `AverageTimeToFirstByte`, `DateLastChecked`, `ConsentManagerFound`, `RequestsWithoutConsent`, `CookiesWithoutConsent`)
- `DELETE /api/websites/{id}` — Delete website (cascade-deletes Software + Tools)
- `GET /api/websites/{id}/software` — List software for a website
- `PUT /api/websites/{id}/software` — Replace all software for a website (accepts `List<SoftwareCreateDto>`: `Name`, `Version`, `FoundAt`)
- `GET /api/websites/{id}/tools` — List tools for a website
- `PUT /api/websites/{id}/tools` — Replace all tools for a website (accepts `List<ToolCreateDto>`: `Name`, `FoundAt`)

**`DomainsToSkipController` (`/api/domianstoskip`) → `IDomainToSkipService`:**

- `GET /api/domainstoskip` — List all domains sorted by name
- `GET /api/domainstoskip/{id}` — Single domain
- `POST /api/domainstoskip` — Create domain (accepts `DomainToSkipCreateDto`: `Domain`). Returns `409 Conflict` if exists.
- `DELETE /api/domainstoskip/{id}` — Delete domain

### Web API Configuration

**Authentication:**
- All `/api/*` endpoints require `X-API-Key` header (`ApiKeyMiddleware`)
- Configured via `ApiKey` in appsettings or environment variables
- Returns 401 Unauthorized if key is missing/wrong, 500 if `ApiKey` not configured on server
- Development key: `dev-api-key-12345` in `appsettings.Development.json`

**Program.cs middleware stack:**
- `AddInfrastructure(connectionString)` + `AddApplication()` — DI registration
- `AddControllers()` with `ReferenceHandler.IgnoreCycles` — prevents infinite loops on circular navigation properties
- `AddOpenApi()` + `AddSwaggerGen()` — API documentation
- Auto-migration: `db.Database.MigrateAsync()` on startup
- `X-Robots-Tag: noindex, nofollow` header on all responses
- `UseSwagger()` + `UseSwaggerUI()` — Swagger UI at `/swagger` (all environments)
- `MapOpenApi()` — OpenAPI schema (Development only)
- `UseHttpsRedirection()` → `UseMiddleware<ApiKeyMiddleware>()` → `MapControllers()`

**Port configuration:**
- Development: `http://localhost:8085` (launchSettings.json)
- Docker: Port `8080` (EXPOSE in Dockerfile)

### Worker

The worker project contains two `BackgroundService`s:

1. **`CompanyEnrichmentWorker`** — Finds websites for companies using Serper (Google Search) + Ollama pipeline.
2. **`WebsiteEnrichmentWorker`** — Enriches website metadata (HTTP response, SSL, consent manager, software detection). Detects multiple software/CMS per website and stores them as `Software` child entities.

**NuGet packages:**
- `HtmlAgilityPack` — HTML parsing for text extraction and Impressum link finding
- `PuppeteerSharp` — Headless Chromium for consent manager detection
- `Nager.PublicSuffix` — Domain suffix parsing for third-party request tracking
- `OpenAI` SDK — Used for Ollama integration (local LLM via OpenAI-compatible API)
- `Microsoft.Extensions.Hosting` + `Microsoft.Extensions.Http`

**Project structure:**
```
companyOSINT.Worker/
  Program.cs                      — .env loading + DI registration only
  CompanyEnrichmentWorker.cs      — Slim orchestrator (BackgroundService)
  WebsiteEnrichmentWorker.cs      — Website metadata enrichment (BackgroundService)
  Services/
    CompanyApiClient              — HTTP wrapper for Web API calls
    SerperSearchService           — Google search via Serper API + URL blacklisting
    WebScrapingService            — HTML fetching, text extraction, Impressum link finding
    OllamaService                 — LLM confidence scoring + Sector/Activity extraction (uses standardized sector list)
    CompanyMatchingService        — Impressum cross-matching with cascading name-matching tiers
    SectorCacheService            — In-memory cache of standardized sectors, loaded from API at startup
    WebsiteCheckService           — DNS resolution, SSL validation, HTTP metadata collection
    ConsentCheckService           — Headless Chromium consent manager detection + third-party tracking
  Detection/
    DetectionEngine               — Rule-based software/CMS/tool detection (two-pass: independent then dependent)
    Descriptors                   — Static list of all detector definitions (software + tools)
    DetectorDescriptor            — record(Name, Kind, Rules, RequiresParent?)
    DetectionRule                 — record(Type, Pattern, HeaderName?) with 4 rule types
    DetectionResult               — record(Software, Tools)
  Matching/
    NameNormalizer                — Normalization (legal form removal, umlauts, punctuation)
    ColognePhonetics              — Kölner Phonetik algorithm for German phonetic encoding
    TrigramSimilarity             — Trigram generation + Jaccard similarity
  Models/
    EnrichmentResult              — record(UrlWebsite, SectorId?, Activity, UrlImprint?)
    SectorDto                     — record(Id, Name) for API communication
    FetchResult                   — record(PageText, ImpressumUrl?)
    SoftwareDetection             — record(Name, Version, FoundAt)
    WebsiteCheckResult            — record(HttpResponseCode, IpAddress?, SslValid, AverageTimeToFirstByte, HtmlBody?, ResponseHeaders)
    ConsentCheckResult            — record(ConsentManagerFound, RequestsWithoutConsent, CookiesWithoutConsent)
    WebsitePatchRequest           — record for PATCH data
    SerperModels                  — SerperResponse, SerperOrganicResult
```

Each service has an interface (`I*`) and is registered as singleton in DI. All HTTP calls go through `IHttpClientFactory` with named clients (`"Api"`, `"Serper"`, `"WebFetch"`).

**HTTP client factory (3 named clients):**
- `"Api"` — Web API client with `X-API-Key` header. Base URL from `ApiBaseUrl` env var.
- `"Serper"` — Serper Google Search API with `X-API-KEY` header. Base URL: `https://google.serper.dev/`
- `"WebFetch"` — Generic website crawler. 15s timeout, max 1MB response, Chrome User-Agent, `Accept-Language: de-DE,de,en-US,en`, automatic decompression (gzip/deflate/brotli).

**Main loop (`CompanyEnrichmentWorker.ExecuteAsync`):**
1. At startup: loads sector cache via `SectorCacheService.RefreshCacheAsync` and matching cache via `CompanyMatchingService.RefreshCacheAsync` (both refresh every 60 min)
2. `GET /api/companies/next-to-check` — fetches the next company where `DateLastChecked == null`
3. If `204 No Content`: waits 30s, then retries
4. If company returned: runs `EnrichCompanyAsync`
5. If website found: `POST /api/websites` creates a Website entity
6. Always: `PATCH /api/companies/{id}` sets `SectorId`, `Activity`, `DateLastChecked`
7. On error: waits 10s, then retries

**Main loop (`WebsiteEnrichmentWorker.ExecuteAsync`):**
1. At startup: `ConsentCheckService.InitializeAsync()` — downloads Chromium + public suffix list
2. `GET /api/websites/next-to-enrich` — fetches next website needing enrichment
3. If `204 No Content`: waits 30s, then retries
4. `WebsiteCheckService.CheckAsync(url)` — DNS resolution, SSL check, HTTP fetch (status + TTFB + HTML body + response headers)
5. If HTML body present: `DetectionEngine.DetectAll(html, headers, url)` — rule-based software/tool detection
6. If HTTP 2xx: `ConsentCheckService.CheckAsync(url)` — headless Chromium consent check (third-party requests, cookies, CMP detection)
7. `PATCH /api/websites/{id}` — sends all metadata (status, IP, SSL, TTFB, consent data)
8. If software detected: `PUT /api/websites/{id}/software`
9. If tools detected: `PUT /api/websites/{id}/tools`
10. On error: waits 10s, then retries

**Enrichment pipeline (`EnrichCompanyAsync`):**
1. `SerperSearchService.BuildSearchQuery` — company name + city → search query
2. `SerperSearchService.SearchAsync` — POST to Serper API, returns up to 10 organic results
3. For each search result (with pre-filtering via `IsPdfUrl` and `IsBlacklistedUrl` — ~40 blacklisted domains):
   - `WebScrapingService.FetchAndExtractTextAsync` — fetches HTML (15s timeout, max 1MB, Chrome UA), strips script/style/noscript, converts to plaintext (max 8000 chars), scans `<a>` tags for Impressum links
   - If Impressum link found: `WebScrapingService.FetchPlainTextAsync` fetches the Impressum page
   - **Cross-matching** (`TryCrossMatchAsync`): checks if the Impressum matches any OTHER company in the cache (see below). If yes: creates a Website entity for the matched company, extracts Sector + Activity, PATCHes the matched company, removes it from cache.
   - **Hard Impressum check** (`CompanyMatchingService.CheckImpressumMatch`): if the primary company's name appears in the Impressum → confidence = 1.0, Ollama skipped
   - Otherwise: `OllamaService.GetConfidenceScoreAsync` — rates 0.0–1.0 whether the page is the company's own official homepage
   - If confidence > 0.6: `OllamaService.ExtractSectorAndActivityAsync` picks a sector from the standardized list (or suggests new), `SectorCacheService.GetOrCreateSectorAsync` resolves to ID, `WebScrapingService.NormalizeToBaseUrl` reduces URL to scheme+domain → returns result
4. If no result exceeds the threshold: returns empty result (DateLastChecked still gets set, no Website created)

**Impressum cross-matching (`CompanyMatchingService`):**

The matching service loads all unchecked company names (`DateLastChecked == null`) from the API into an in-memory cache and builds three indexes. When an Impressum is fetched, `FindMatchInImpressum` checks it against all cached companies using a cascading approach — fast/cheap tiers first, fuzzy/expensive only as fallback:

1. **Normalized exact match** — extracts candidate company names from the Impressum (segments containing legal form suffixes like GmbH, UG, AG), normalizes them (lowercase, ä→ae, remove punctuation), and looks them up in a dictionary index.
2. **Cologne Phonetics** — encodes the candidate using Kölner Phonetik (German phonetic algorithm) and checks against a phonetic code index. Handles spelling variants like Müller/Mueller/Miller.
3. **Trigram similarity** — computes 3-character sliding windows and Jaccard similarity. Threshold > 0.6.

If a match is found for a different company, `TryCrossMatchAsync` creates a Website entity via `POST /api/websites`, extracts Sector + Activity from the already-fetched page text (no extra HTTP call) via Ollama, resolves the sector name to a standardized SectorId via `SectorCacheService`, then PATCHes the matched company with SectorId, Activity, and DateLastChecked. This saves a full Serper API call when that company comes up for processing later.

**Detection Engine (`Detection/`):**

Rule-based system for identifying software/CMS platforms and tools from website HTML and HTTP headers.

- **Two-pass detection:** First pass detects independent software (WordPress, Joomla, etc.); second pass detects dependent software (e.g. WooCommerce only if WordPress was detected).
- **Four rule types:** `MetaGenerator` (regex match on `<meta name="generator">`), `HtmlContains` (case-insensitive substring), `HeaderExists` (checks header presence), `HeaderContains` (checks header value substring).
- **First-match-wins** per descriptor — stops after first matching rule.

Currently detected:
- **Software (11):** WordPress, WooCommerce (requires WordPress), Joomla, Drupal, TYPO3, Contao, Magento, Shopware, PrestaShop, OpenCart, nopCommerce
- **Tools (5):** Google Analytics, Google Fonts, Cloudflare, Matomo, Facebook Pixel

**Consent check (`ConsentCheckService`):**

Uses headless Chromium (PuppeteerSharp) to detect consent managers and track third-party activity without user consent.

- **Third-party request tracking** — intercepts all network requests; counts requests to domains different from the website's registered domain (parsed via Nager.PublicSuffix)
- **Cookie counting** — counts cookies set without user consent
- **Consent manager detection** (three methods):
  1. Script domain blacklist — ~11 known CMP domains (cookiebot.com, usercentrics.eu, borlabs.io, etc.)
  2. DOM selectors — ~22 CSS selectors for common CMP banners (#onetrust-banner-sdk, #usercentrics-root, etc.)
  3. JavaScript APIs — detects `window.__tcfapi`, `window.__gpp`, `window.__cmp` (TCF, GPP, CMP standards)
- `InitializeAsync()` downloads public suffix list + Chromium on first startup

**Key methods by service:**

| `CompanyApiClient` | Purpose |
|---|---|
| `GetNextToCheckAsync` | Fetch next unchecked company (or null on 204) |
| `GetNextWebsiteToEnrichAsync` | Fetch next website for enrichment (or null on 204) |
| `PatchCompanyAsync` | PATCH company with sectorId, activity, dateLastChecked |
| `PatchWebsiteAsync` | PATCH website with enrichment metadata |
| `GetSectorsAsync` | GET all sectors from API |
| `CreateSectorAsync` | POST new sector (with 409 conflict handling) |
| `CreateWebsiteAsync` | POST new Website entity for a company |
| `GetNamesToCheckBatchedAsync` | Load all unchecked company names for matching cache |
| `ReplaceSoftwareAsync` | PUT detected software list for a website |
| `ReplaceToolsAsync` | PUT detected tools list for a website |

| `SerperSearchService` | Purpose |
|---|---|
| `BuildSearchQuery` | Company name + city → search query |
| `SearchAsync` | Serper API call → `List<SerperOrganicResult>` |
| `IsBlacklistedUrl` | Check URL against ~40 blacklisted domains (incl. subdomain handling) |
| `IsPdfUrl` | Check for `.pdf` suffix |

| `WebScrapingService` | Purpose |
|---|---|
| `FetchAndExtractTextAsync` | URL → `FetchResult(PageText, ImpressumUrl?)` |
| `FetchPlainTextAsync` | URL → plaintext (max 8000 chars) |
| `NormalizeToBaseUrl` | Full URL → `scheme://host` |

| `OllamaService` | Purpose |
|---|---|
| `GetConfidenceScoreAsync` | Page text + optional Impressum → confidence score (0.0–1.0) |
| `ExtractSectorAndActivityAsync` | Page text + sector list → `(SectorName, Activity)` via JSON extraction |

| `CompanyMatchingService` | Purpose |
|---|---|
| `RefreshCacheAsync` | Load company names from API, build 3 matching indexes |
| `FindMatchInImpressum` | Cascading 3-tier match against Impressum text |
| `CheckImpressumMatch` | Hard check: company name in Impressum text |
| `RemoveFromCache` | Remove enriched company from all indexes |

| `SectorCacheService` | Purpose |
|---|---|
| `RefreshCacheAsync` | Load all sectors from API into in-memory cache |
| `FindSectorId` | Case-insensitive lookup of sector name → Guid |
| `GetSectorList` | Formatted sector list for LLM prompt |
| `GetOrCreateSectorAsync` | Resolve name to ID; creates via API if not found |

| `WebsiteCheckService` | Purpose |
|---|---|
| `CheckAsync` | URL → `WebsiteCheckResult` (DNS, SSL, HTTP status, TTFB, HTML body, response headers). 30s timeout, Chrome UA, max 5 redirects. |

| `ConsentCheckService` | Purpose |
|---|---|
| `InitializeAsync` | Download public suffix list + Chromium binary |
| `CheckAsync` | URL → `ConsentCheckResult` (CMP found, third-party requests, cookies). Headless Chromium. |

| `DetectionEngine` | Purpose |
|---|---|
| `DetectAll` | HTML + headers + URL → `DetectionResult` (software list, tools list). Two-pass, first-match-wins. |

**Configuration (env vars / `.env` file):**
- `ApiBaseUrl` — Web API URL (default: `https://www.company-osint.com`, Docker: `http://companyOSINT.web:8445`)
- `ApiKey` — API key for authenticating with the Web API (required, passed as `X-API-Key` header)
- `OllamaUrl` — Ollama endpoint (default: `http://192.168.12.117:11434/v1/`, 5min timeout)
- `OllamaModel` — Ollama model (default: `gpt-oss:20b`)
- `SerperApiKey` / `SERPER_API_KEY` — API key for Serper (required)

The worker loads `.env` from the solution root at startup (existing env vars take precedence).

### Processing Semantics

**Company processing flag (`Company.DateLastChecked`):**
- `null` — Not yet checked for websites. Will be picked up by the CompanyEnrichmentWorker.
- Non-null — Already checked. The worker will skip it.

**Website enrichment flag (`Website.DateLastChecked`):**
- `null` — Website found but technical metadata not yet collected. Will be picked up by WebsiteEnrichmentWorker.
- Non-null — Technical metadata already collected. Re-checked after 30 days.

**Sector / Activity (on Company):**
- `SectorId`: FK to `Sector` table. ~65 standardized German sectors seeded at DB creation. New sectors auto-created by Worker if LLM suggests one not in list. `null` if unknown.
- `Activity`: Short German description of the company's business. Empty string if unknown.

### Import (`companyOSINT.Import`)

Console app that migrates company + contact records from local SQLite (`handelsregister.db`) to PostgreSQL. Uses `NpgsqlBinaryImporter` (COPY) for high-performance bulk loading in batches of 50,000. Converts SQLite integer IDs to deterministic GUIDs via `IntToGuid()`. Only imports "currently registered" companies.

**Usage:** `dotnet run --project companyOSINT.Import [sqlite-path] [postgres-connection-string]`
- Default sqlite-path: `handelsregister.db`
- Default connection string: `Host=localhost;Database=companyOSINT;Username=postgres;Password=postgres`
- Schema must be created via migrations before import: `dotnet ef database update --project companyOSINT.Web --startup-project companyOSINT.Web`

### Docker & Deployment

**compose.yaml** (development):
- `postgres` — PostgreSQL with default credentials
- `companyOSINT.web` — Web API on port 8445 (internal)
- `companyOSINT.worker` — Worker, depends on web

**compose.prod.yaml** (production):
- Service names: `findACompany-web`, `findACompany-worker`, `findACompany-import`
- Web port: `127.0.0.1:8085` → internal `8080`
- PostgreSQL: `127.0.0.1:18085` with health checks (`pg_isready`, 5s intervals)
- Worker + Import use profiles: `--profile worker`, `--profile import`
- Restart policy: `unless-stopped`
- Import mounts `handelsregister.db` as read-only volume

**Worker Dockerfile:** Installs Chromium system dependencies (~30 packages) for PuppeteerSharp.

**Environment variables** (`.env.example`):
- `POSTGRES_PASSWORD`, `API_KEY` — Required for production
- `SERPER_API_KEY` — Required for worker
- `OLLAMA_URL`, `OLLAMA_MODEL` — Optional, defaults in code

## Key Conventions

- Target framework: `net10.0` with nullable reference types and implicit usings enabled
- Solution format: `.slnx` (XML-based, not classic `.sln`)
- `handelsregister.db` is a local SQLite database file (not committed via .gitignore intent — currently untracked)
- EF Core migrations live in `companyOSINT.Web/Migrations/`, create with: `dotnet ef migrations add <Name> --project companyOSINT.Web --startup-project companyOSINT.Web`
- Database: PostgreSQL (Npgsql.EntityFrameworkCore.PostgreSQL), auto-migrated at Web API startup
- DI registration: `builder.Services.AddInfrastructure(connectionString)` + `builder.Services.AddApplication()` in `Program.cs`
- All Application services are registered as Scoped
- All Worker services are registered as Singleton
- Use German umlauts such as ä ö ü