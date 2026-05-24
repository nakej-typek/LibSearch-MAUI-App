# HW02 — CrossStichDrawer

Třetí domácí úkol pro **PB178 Programování v jazyce C#** (FI MUNI, jaro 2026).
Autor: J. Prosecký.

Desktopová **.NET MAUI** aplikace (`net10.0-windows10.0.19041.0`, volitelně iOS/MacCatalyst) pro návrh vzorů pro křížkovou výšivku. Cílem úkolu bylo procvičit MAUI, **MVVM pattern** (CommunityToolkit.Mvvm), data binding, XAML, custom layouty a práci se souborovým systémem.

## Funkce

1. **Nový vzor** — uživatel zadá šířku a výšku v buňkách a vytvoří se prázdná mřížka.
2. **Výběr barvy** — `Picker` s nabídkou DMC ("Floss") barev načtených z CSV (`Resources/Raw/threadcolors_dmc_rgb.csv`).
3. **Kreslení** — klik na buňku ji obarví aktivní barvou; opakovaný klik stejnou barvou ji vymaže (toggle).
4. **Uložení / načtení** — vzor lze uložit do textového formátu `.csp` / `.txt` a později načíst zpět.

## Formát souboru

Textový, řádkový, čitelný:

```
SIZE;<width>;<height>
CELL;<row>;<column>;<dmcFloss>
CELL;<row>;<column>;<dmcFloss>
...
```

Ukládají se jen vybarvené buňky — prázdné se vynechávají. Při načítání se floss kód mapuje na `ThreadColor` z paletky.

## Architektura (MVVM)

- **`Models/`** — `Pattern` (2D mřížka `PatternCell`), `PatternCell` (souřadnice + nullable `ThreadColor`), `ThreadColor` (DMC floss kód + RGB).
- **`ViewModel/MainViewModel.cs`** — `ObservableObject` s `[ObservableProperty]` a `[RelayCommand]` (CommunityToolkit.Mvvm). Drží aktuální vzor, výběr barvy, rozměry a commandy pro New / Save / Load.
- **`View/MainPage.xaml`** — UI: toolbar (rozměry, Picker barev, Save/Load), `ScrollView` s dynamicky generovanou mřížkou tlačítek.
- **`Services/`**
  - `PatternParser` — serializace `Pattern` ↔ string (CSV-like).
  - `PatternFileManager` — wrap nad parserem pro práci se soubory.
  - `ThreadColorCsvLoader` — parsuje balíkovaný CSV s DMC barvami.

## Prerekvizity

- **.NET 10 SDK** s MAUI workloadem (`dotnet workload install maui`).
- Pro Windows build: Windows 10 1809+ a Windows App SDK.

## Spuštění

V Rider / Visual Studio otevři `CrossStichDrawer.sln` a spusť `CrossStichDrawer` (Windows Machine).

Z CLI:

```bash
dotnet build CrossStichDrawer.sln
dotnet build CrossStichDrawer/CrossStichDrawer.csproj -t:Run -f net10.0-windows10.0.19041.0
```

## Struktura

```
CrossStichDrawer/
├── CrossStichDrawer.sln
└── CrossStichDrawer/
    ├── App.xaml(.cs), AppShell.xaml(.cs), MauiProgram.cs
    ├── Models/      (Pattern, PatternCell, ThreadColor)
    ├── ViewModel/   (MainViewModel)
    ├── View/        (MainPage.xaml + code-behind)
    ├── Services/    (PatternParser, PatternFileManager, ThreadColorCsvLoader)
    ├── Resources/   (DMC barvy CSV, ikony, fonty, splash)
    └── Platforms/   (per-platform startup)
```
