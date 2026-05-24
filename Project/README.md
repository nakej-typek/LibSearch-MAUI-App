# LibSearch

Semestral project for **PB178 Programming in C#**, FI MUNI, spring 2026.
Author: J. Prosecký, UČO 550554.

A **.NET MAUI** desktop application for **semantic passage search in text**. The user uploads a `.txt` file, which is sent to a locally running REST API (FastAPI + ChromaDB + the Czech embedding model `retromae-small-cs`) for indexing via vector embeddings. The user can then ask natural-language questions, save interesting passages with notes and tags, browse their search history and export the results.

Unlike a full-text search, LibSearch matches by **meaning**, not by string occurrence — a query like "where does the hero first feel fear?" can find the relevant passage even when the word "fear" never appears.

## Feature scope

1. **Authentication** — register, login, logout. Local users only, passwords hashed with BCrypt (`BCrypt.Net-Next`).
2. **Library** — upload `.txt`, rename, delete, list of the user's own documents. Files stored under `%LocalAppData%\<AppId>\library\{userId}\{docId}.txt`.
3. **Semantic search** — natural-language query over a single document; results are passages sorted by relevance (cosine distance). Each document gets its own Chroma collection `user{userId}-doc{docId}`.
4. **Reader** — full document text rendered chunk-by-chunk; clicking a result scrolls to and highlights the corresponding chunk.
5. **Saved passages** — save a result with an optional note and tags (comma-separated).
6. **Search history** — every query is logged (timestamp, document, prompt, result count); a "Rerun" button re-executes the query.
7. **Filters** — on both History and Saved Passages: document, date (togglable), tags (Saved only), free-text contains.
8. **Export** — saved passages can be exported to `.txt`, `.md`, `.json`. The file is written to `Documents\`.
9. **Stats** — total query count, queries today, number of saved passages, top 5 most-searched documents.

## Architecture

```
LibSearch.sln
└── LibSearch.App                  (MAUI, target: net9.0-windows10.0.19041.0)
    ├── App.xaml(.cs), AppShell.xaml(.cs)
    ├── MauiProgram.cs             (DI registrations)
    ├── Converters/                (XAML IValueConverter implementations)
    ├── Data/AppDbContext.cs       (EF Core SQLite, OnModelCreating)
    ├── Models/Entities/           (User, TextDocument, SearchHistoryItem,
    │                               SavedPassage, Tag, SavedPassageTag)
    ├── Services/                  (auth, session, library, HTTP client, export, stats)
    ├── ViewModels/                (CommunityToolkit.Mvvm)
    ├── Views/                     (XAML pages and reusable ContentViews)
    ├── Platforms/Windows/
    └── Resources/
```

**Key choices:**

- **MVVM pattern** — `Views/` (XAML, code-behind only for trivial wiring), `ViewModels/` with `CommunityToolkit.Mvvm` (`[ObservableProperty]`, `[RelayCommand]`), `Services/` injected via constructor. No business logic in code-behind.
- **DI** via the built-in `Microsoft.Extensions.DependencyInjection` in `MauiProgram.cs` — VMs registered as transient, services (auth, library, HTTP client) as singleton/scoped as appropriate.
- **Persistence** — SQLite via EF Core, `AppDbContext` in `Data/`. DB file: `%LocalAppData%\<AppId>\libsearch.db`. Unique index on `User.Username` and a composite `(OwnerId, Name)` on `Tag`.
- **HTTP client** — `HttpClient` via `IHttpClientFactory` (`Microsoft.Extensions.Http`), wrapped behind `ILibSearchClient`.
- **Navigation** — MAUI Shell, routes registered in `AppShell.xaml.cs`.

### Async correctness

Proper async usage was an explicit assignment requirement and the syllabus demands it be "correct":

- All I/O is `async Task` / `async Task<T>`. No `.Result` / `.Wait()` / `.GetAwaiter().GetResult()`.
- No `async void` outside UI event handlers that immediately delegate to a command.
- HTTP calls accept and forward a `CancellationToken`.
- `SearchViewModel` owns a `CancellationTokenSource` that cancels the previous in-flight query when a new search starts or the page disappears.
- EF Core calls use `ToListAsync` / `FirstOrDefaultAsync` / `SaveChangesAsync`.
- File IO uses `File.ReadAllTextAsync` / `WriteAllTextAsync`.

## Prerequisites

- **.NET 9 SDK** with the MAUI workload installed (`dotnet workload install maui-windows`).
- **Docker Desktop** for running the API server.
- Windows 10 1809+ (build 17763+).

## API server (LibSearch backend)

The app calls a REST API that must be reachable locally or over the network (e.g. via Tailscale). The backend is a separate project (FastAPI + ChromaDB + Czech embedding model `retromae-small-cs`, author: Jakub Mazel) — LibSearch is its desktop client.

Starting the server:

```bash
cd path/to/project-api-server
docker compose up -d
curl http://localhost:8080/test-connection/
# -> {"result":"OK"}
```

Endpoints used by the client:

| Method | Endpoint | Purpose |
|---|---|---|
| `GET` | `/test-connection/` | health check |
| `POST` | `/ingest/` | body `{collection, document_id, text}` → chunk + embed + store in Chroma |
| `GET` | `/query/` | body `{collection, prompt}` → semantic search, returns ranked passages |
| `DELETE`| `/collection/{name}` | drop a collection when its document is deleted |

### Client base URL

`Services/LibSearchOptions.cs` defaults to `http://localhost:8080`. If the server runs on a different host (e.g. another machine over Tailscale), edit before building:

```csharp
public string BaseUrl { get; set; } = "http://archlinux:8080";
```

## Running the app

In Rider / Visual Studio open `LibSearch.sln` and launch `LibSearch.App` (Windows Machine).

From the CLI:

```bash
dotnet build LibSearch.sln
dotnet run --project LibSearch.App
```

## First-time use

1. Start the API server (`docker compose up -d` in the backend directory).
2. Start the LibSearch app.
3. Click "Create account", enter a username (3+ chars) and password (6+ chars).
4. In the Library click "Upload .txt", pick a file — the file is copied into internal storage and ingested via the API (this can take a few seconds; a progress indicator is shown).
5. Click a document in the list — the Reader opens.
6. The full text is on the left (chunked), the search bar is on the right — type a query and hit Enter / Search.
7. Click "Show in text" on a result, the scroll position jumps and the chunk is highlighted in yellow.
8. Click "Save" on a result, enter a note and tags, click Save.
9. From the Library: buttons for History / Saved / Stats / Logout.

## Schema changes (for developers)

The app calls `db.Database.EnsureCreated()` at startup. For a 2-day deadline we deliberately did not generate EF migrations — if you change entities, delete `%LocalAppData%\<AppId>\libsearch.db` and a fresh schema will be created on the next start. For a production setup `EnsureCreated` would be replaced with `Database.MigrateAsync()` and migrations generated via `dotnet ef migrations add`.

## Course scope

The project only uses techniques covered in PB178: console IO, collections, generics, delegates, pattern matching, streams, MAUI events/layout, MVVM, threads/tasks/async, EF Core, LINQ. No Reactive Extensions, MediatR, AutoMapper or other DI container beyond the built-in `Microsoft.Extensions.DependencyInjection`.

## AI usage disclosure

During the development of this project an AI assistant (Anthropic Claude) was used for code scaffolding, structural suggestions and speeding up routine work (entity definitions, ViewModel boilerplate, XAML templates). All generated code was reviewed, adjusted and integrated by the author. The AI did not produce the project autonomously — the overall architectural design, library selection, functional scope, debugging and integration with the custom backend are the author's own work. This note satisfies the course requirement to disclose the use of generative AI tools.
