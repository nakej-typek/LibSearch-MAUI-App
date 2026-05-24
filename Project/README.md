# LibSearch

Semestrální projekt pro **PB178 Programování v jazyce C#**, FI MUNI, jaro 2026.
Autor: J. Prosecký, UČO 550554.

Desktopová MAUI aplikace pro **sémantické vyhledávání pasáží v textech**. Uživatel nahraje `.txt` soubor, ten je odeslán do lokálně běžícího REST API (FastAPI + ChromaDB) pro indexaci pomocí vector embeddings, a v aplikaci pak může pokládat dotazy v přirozeném jazyce, ukládat zajímavé pasáže s poznámkami a tagy, prohlížet historii vyhledávání a exportovat výsledky.

## Funkční rozsah

1. **Autentizace** — registrace, přihlášení, odhlášení. Lokální uživatelé, hesla hashovaná BCryptem (BCrypt.Net-Next).
2. **Knihovna** — upload `.txt`, přejmenování, smazání, zobrazení listu vlastních dokumentů.
3. **Sémantické vyhledávání** — dotaz v přirozeném jazyce nad jedním dokumentem, výsledky jsou pasáže seřazené podle relevance (cosine distance).
4. **Reader** — celý text dokumentu po chuncích, klik na výsledek scrolluje a zvýrazní odpovídající chunk.
5. **Uložené pasáže** — uložení výsledku s volitelnou poznámkou a tagy (čárkou oddělené).
6. **Historie vyhledávání** — každý dotaz je zalogován; tlačítkem "Rerun" lze dotaz znovu spustit.
7. **Filtry** — na History i Saved Passages: dokument, datum (toggle on/off), tagy (jen na Saved), free-text contains.
8. **Export** — uložené pasáže lze exportovat do `.txt`, `.md`, `.json`. Soubor se uloží do `Dokumenty\`.
9. **Stats** — celkový počet dotazů, dotazy dnes, počet uložených pasáží, top 5 nejčastěji prohledávaných dokumentů.

## Architektura

- `LibSearch.sln` se single MAUI projektem `LibSearch.App` (target `net9.0-windows10.0.19041.0`).
- MVVM pattern: `Views/` (XAML), `ViewModels/` (CommunityToolkit.Mvvm s `[ObservableProperty]` a `[RelayCommand]`), `Services/`, `Models/Entities/`.
- DI přes built-in `Microsoft.Extensions.DependencyInjection` v `MauiProgram.cs`.
- Persistence: SQLite přes EF Core (`AppDbContext` v `Data/`). DB soubor: `%LocalAppData%\<AppId>\libsearch.db`.
- HTTP klient pro API: `HttpClient` přes `IHttpClientFactory` (`Microsoft.Extensions.Http`).
- Navigace: MAUI Shell, routy registrované v `AppShell.xaml.cs`.

### Async correctness

Všechna I/O používá `async Task`. Žádné `.Result` / `.Wait()` / `async void` (kromě event handlerů). HTTP volání akceptují a předávají `CancellationToken`. SearchViewModel vlastní `CancellationTokenSource`, který cancelluje předchozí běžící dotaz při novém searchi.

## Prerekvizity

- **.NET 9 SDK** s nainstalovaným MAUI workloadem (`dotnet workload install maui-windows`).
- **Docker Desktop** pro běh API serveru.
- Windows 10 1809+ (build 17763+).

## API server (LibSearch backend)

Aplikace volá REST API, které musí běžet lokálně, nebo dosažitelně přes síť (např. Tailscale). Backend je vlastní projekt — FastAPI + ChromaDB + Czech embedding model, autor: Jakub Mazel.

Spuštění serveru (typicky na stejném stroji nebo na lokálním serveru v LAN/tailnetu):

```bash
cd path/to/project-api-server
docker compose up -d
curl http://localhost:8080/test-connection/
# -> {"result":"OK"}
```

Endpointy používané klientem:
- `GET /test-connection/`
- `POST /ingest/` — body `{collection, document_id, text}` → indexace dokumentu
- `GET /query/` — body `{collection, prompt}` → semantic search
- `DELETE /collection/{name}` — odstranění kolekce při smazání dokumentu

### Base URL klienta

V `Services/LibSearchOptions.cs` je defaultní URL `http://localhost:8080`. Pokud server neběží lokálně (např. na jiném stroji přes Tailscale), změň hodnotu před buildem aplikace:

```csharp
public string BaseUrl { get; set; } = "http://archlinux:8080";
```

## Spuštění aplikace

V Rider / Visual Studio otevři `LibSearch.sln` a spusť `LibSearch.App` (Windows Machine).

Nebo z CLI:

```bash
dotnet build LibSearch.sln
dotnet run --project LibSearch.App
```

## První použití

1. Spusť API server (`docker compose up -d` v adresáři backendu).
2. Spusť LibSearch aplikaci.
3. Klikni "Create account", zadej username (min. 3 znaky) a heslo (min. 6 znaků).
4. V Library klikni "Upload .txt", vyber soubor — proběhne kopie do interního úložiště a ingest na API.
5. Klikni na dokument v seznamu — otevře se Reader.
6. Vlevo je celý text po chuncích, vpravo searchbar — napiš dotaz a stiskni Enter / Search.
7. Klikni "Show in text" u výsledku, scroll skočí a chunk se zvýrazní žlutě.
8. Klikni "Save" u výsledku, zadej poznámku a tagy, klikni Save.
9. Z Library: tlačítka History / Saved / Stats / Logout.

## Schema changes (vývojářům)

Aplikace volá `db.Database.EnsureCreated()` při startu. Pro 2denní termín jsme záměrně nevytvořili migrations — pokud změníš entity, smaž `%LocalAppData%\<AppId>\libsearch.db` a po dalším startu se vytvoří nová schéma. Pro produkci by bylo nutné nahradit `EnsureCreated` za `Database.MigrateAsync()` a generovat migrations přes `dotnet ef migrations add`.

## Adresářová struktura

```
LibSearch.App/
├── Converters/                    (XAML IValueConverter implementace)
├── Data/AppDbContext.cs           (EF Core DbContext + OnModelCreating)
├── Models/Entities/               (POCOs / EF entity)
├── Services/                      (auth, library, HTTP klient, export, stats, history, saved)
├── ViewModels/                    (MVVM s CommunityToolkit.Mvvm)
├── Views/                         (XAML pages a reusable ContentViews)
├── App.xaml(.cs)
├── AppShell.xaml(.cs)             (Shell + route registrations)
├── MauiProgram.cs                 (DI registrace)
└── LibSearch.App.csproj
```

## AI usage disclosure

Při vývoji projektu byl použit AI asistent (Anthropic Claude) pro scaffolding kódu, návrh struktury a urychlení rutinních úkolů (definice entit, boilerplate ViewModelů, XAML templates). Veškerý vygenerovaný kód byl autorem zkontrolován, upraven a integrován. AI nepsalo projekt autonomně — návrh celkové architektury, výběr knihoven, definice funkčního scopu, ladění a integrace s vlastním backendem jsou autorská práce. Tato poznámka splňuje požadavek kurzu na disclosure použití generativních AI nástrojů.
