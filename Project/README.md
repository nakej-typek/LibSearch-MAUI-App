# LibSearch

Semestrální projekt pro **PB178 Programování v jazyce C#**, FI MUNI, jaro 2026.
Autor: J. Prosecký, UČO 550554.

Desktopová **.NET MAUI** aplikace pro **sémantické vyhledávání pasáží v textech**. Uživatel nahraje `.txt` soubor, ten je odeslán do lokálně běžícího REST API (FastAPI + ChromaDB + český embedding model `retromae-small-cs`) pro indexaci pomocí vector embeddings, a v aplikaci pak může pokládat dotazy v přirozeném jazyce, ukládat zajímavé pasáže s poznámkami a tagy, prohlížet historii vyhledávání a exportovat výsledky.

Na rozdíl od fulltextového vyhledávače LibSearch hledá podle **významu**, ne podle shody slov — dotaz "kde se hrdina poprvé bojí?" najde pasáž popisující strach i bez výskytu slova "strach".

## Funkční rozsah

1. **Autentizace** — registrace, přihlášení, odhlášení. Lokální uživatelé, hesla hashovaná BCryptem (`BCrypt.Net-Next`).
2. **Knihovna** — upload `.txt`, přejmenování, smazání, seznam vlastních dokumentů. Soubory pod `%LocalAppData%\<AppId>\library\{userId}\{docId}.txt`.
3. **Sémantické vyhledávání** — dotaz v přirozeném jazyce nad jedním dokumentem; výsledky jsou pasáže seřazené podle relevance (cosine distance). Každý dokument = vlastní Chroma kolekce `user{userId}-doc{docId}`.
4. **Reader** — celý text dokumentu po chuncích, klik na výsledek scrolluje a zvýrazní odpovídající chunk.
5. **Uložené pasáže** — uložení výsledku s volitelnou poznámkou a tagy (čárkou oddělené).
6. **Historie vyhledávání** — každý dotaz je zalogován (timestamp, dokument, prompt, počet výsledků); tlačítko "Rerun" dotaz znovu spustí.
7. **Filtry** — na History i Saved Passages: dokument, datum (toggle on/off), tagy (jen na Saved), free-text contains.
8. **Export** — uložené pasáže lze exportovat do `.txt`, `.md`, `.json`. Cílová cesta `Dokumenty\`.
9. **Stats** — celkový počet dotazů, dotazy dnes, počet uložených pasáží, top 5 nejčastěji prohledávaných dokumentů.

## Architektura

```
LibSearch.sln
└── LibSearch.App                  (MAUI, target: net9.0-windows10.0.19041.0)
    ├── App.xaml(.cs), AppShell.xaml(.cs)
    ├── MauiProgram.cs             (DI registrace)
    ├── Converters/                (XAML IValueConverter implementace)
    ├── Data/AppDbContext.cs       (EF Core SQLite, OnModelCreating)
    ├── Models/Entities/           (User, TextDocument, SearchHistoryItem,
    │                               SavedPassage, Tag, SavedPassageTag)
    ├── Services/                  (auth, session, library, HTTP klient, export, stats)
    ├── ViewModels/                (CommunityToolkit.Mvvm)
    ├── Views/                     (XAML stránky a reusable ContentViews)
    ├── Platforms/Windows/
    └── Resources/
```

**Klíčové volby:**

- **MVVM pattern** — `Views/` (XAML, code-behind jen pro triviální wiring), `ViewModels/` s `CommunityToolkit.Mvvm` (`[ObservableProperty]`, `[RelayCommand]`), `Services/` injectované konstruktorem. Žádná business logika v code-behind.
- **DI** přes built-in `Microsoft.Extensions.DependencyInjection` v `MauiProgram.cs` — VMs registrovány jako transient, služby (auth, library, HTTP klient) jako singleton/scoped podle potřeby.
- **Persistence** — SQLite přes EF Core, `AppDbContext` v `Data/`. DB soubor: `%LocalAppData%\<AppId>\libsearch.db`. Unique index na `User.Username` a kompozit `(OwnerId, Name)` na `Tag`.
- **HTTP klient** — `HttpClient` přes `IHttpClientFactory` (`Microsoft.Extensions.Http`), zabalený do `ILibSearchClient`.
- **Navigace** — MAUI Shell, routy registrované v `AppShell.xaml.cs`.

### Async correctness

Async kód byl explicitní požadavek zadání a sylabus jej vyžaduje "korektně":

- Všechna I/O používá `async Task` / `async Task<T>`. Žádné `.Result` / `.Wait()` / `.GetAwaiter().GetResult()`.
- Žádné `async void` mimo UI event handlery, které okamžitě delegují na command.
- HTTP volání akceptují a předávají `CancellationToken`.
- `SearchViewModel` vlastní `CancellationTokenSource`, který cancelluje předchozí běžící dotaz při startu nového searche nebo při odchodu ze stránky.
- EF Core volání jsou `ToListAsync` / `FirstOrDefaultAsync` / `SaveChangesAsync`.
- File IO přes `File.ReadAllTextAsync` / `WriteAllTextAsync`.

## Prerekvizity

- **.NET 9 SDK** s nainstalovaným MAUI workloadem (`dotnet workload install maui-windows`).
- **Docker Desktop** pro běh API serveru.
- Windows 10 1809+ (build 17763+).

## API server (LibSearch backend)

Aplikace volá REST API, které musí běžet lokálně nebo dostupně po síti (např. Tailscale). Backend je samostatný projekt (FastAPI + ChromaDB + Czech embedding model `retromae-small-cs`, autor: Jakub Mazel) — LibSearch je jeho desktopový klient.

Spuštění serveru:

```bash
cd path/to/project-api-server
docker compose up -d
curl http://localhost:8080/test-connection/
# -> {"result":"OK"}
```

Endpointy používané klientem:

| Metoda | Endpoint | Účel |
|---|---|---|
| `GET` | `/test-connection/` | health-check |
| `POST` | `/ingest/` | body `{collection, document_id, text}` → chunking + embedding + uložení do Chromy |
| `GET` | `/query/` | body `{collection, prompt}` → semantic search, vrací seřazené pasáže |
| `DELETE`| `/collection/{name}` | odstranění kolekce při smazání dokumentu |

### Base URL klienta

V `Services/LibSearchOptions.cs` je defaultní URL `http://localhost:8080`. Pokud server neběží lokálně (např. na jiném stroji přes Tailscale), uprav před buildem:

```csharp
public string BaseUrl { get; set; } = "http://archlinux:8080";
```

## Spuštění aplikace

V Rider / Visual Studio otevři `LibSearch.sln` a spusť `LibSearch.App` (Windows Machine).

Z CLI:

```bash
dotnet build LibSearch.sln
dotnet run --project LibSearch.App
```

## První použití

1. Spusť API server (`docker compose up -d` v adresáři backendu).
2. Spusť LibSearch aplikaci.
3. Klikni "Create account", zadej username (min. 3 znaky) a heslo (min. 6 znaků).
4. V Library klikni "Upload .txt", vyber soubor — proběhne kopie do interního úložiště a ingest na API (může pár vteřin trvat, indikátor progressu je viditelný).
5. Klikni na dokument v seznamu — otevře se Reader.
6. Vlevo je celý text po chuncích, vpravo searchbar — napiš dotaz a stiskni Enter / Search.
7. Klikni "Show in text" u výsledku, scroll skočí a chunk se zvýrazní žlutě.
8. Klikni "Save" u výsledku, zadej poznámku a tagy, klikni Save.
9. Z Library: tlačítka History / Saved / Stats / Logout.

## Schema changes (vývojářům)

Aplikace volá `db.Database.EnsureCreated()` při startu. Pro 2denní termín jsme záměrně nevytvářeli EF migrations — pokud změníš entity, smaž `%LocalAppData%\<AppId>\libsearch.db` a po dalším startu se vytvoří nová schéma. Pro produkční nasazení by bylo nutné nahradit `EnsureCreated` za `Database.MigrateAsync()` a generovat migrations přes `dotnet ef migrations add`.

## Rozsah kurzu

Projekt používá pouze techniky pokryté v PB178: console IO, kolekce, generika, delegáty, pattern matching, streams, MAUI events/layout, MVVM, threads/tasks/async, EF Core, LINQ. Žádný Reactive Extensions, MediatR, AutoMapper ani jiný DI container mimo built-in `Microsoft.Extensions.DependencyInjection`.

## AI usage disclosure

Při vývoji projektu byl použit AI asistent (Anthropic Claude) pro scaffolding kódu, návrh struktury a urychlení rutinních úkolů (definice entit, boilerplate ViewModelů, XAML templates). Veškerý vygenerovaný kód byl autorem zkontrolován, upraven a integrován. AI nepsalo projekt autonomně — návrh celkové architektury, výběr knihoven, definice funkčního scopu, ladění a integrace s vlastním backendem jsou autorská práce. Tato poznámka splňuje požadavek kurzu na disclosure použití generativních AI nástrojů.
