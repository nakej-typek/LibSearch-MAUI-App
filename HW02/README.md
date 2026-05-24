# HW02 — CrossStichDrawer

Third homework for **PB178 Programming in C#** (FI MUNI, spring 2026).
Author: J. Prosecký.

A **.NET MAUI** desktop application (`net10.0-windows10.0.19041.0`, optionally iOS / MacCatalyst) for designing cross-stitch patterns. The goal of the assignment was to practice MAUI, the **MVVM pattern** (CommunityToolkit.Mvvm), data binding, XAML, custom layouts and file system access.

## Features

1. **New pattern** — the user enters width and height in cells and an empty grid is created.
2. **Color selection** — `Picker` listing DMC ("Floss") colors loaded from a CSV (`Resources/Raw/threadcolors_dmc_rgb.csv`).
3. **Drawing** — clicking a cell paints it with the active color; clicking again with the same color clears it (toggle).
4. **Save / load** — a pattern can be saved to a text-based `.csp` / `.txt` file and later loaded back.

## File format

Plain-text, line-oriented, human-readable:

```
SIZE;<width>;<height>
CELL;<row>;<column>;<dmcFloss>
CELL;<row>;<column>;<dmcFloss>
...
```

Only colored cells are persisted — empty ones are omitted. On load the floss code is mapped back to a `ThreadColor` from the palette.

## Architecture (MVVM)

- **`Models/`** — `Pattern` (2D grid of `PatternCell`), `PatternCell` (coordinates + nullable `ThreadColor`), `ThreadColor` (DMC floss code + RGB).
- **`ViewModel/MainViewModel.cs`** — `ObservableObject` with `[ObservableProperty]` and `[RelayCommand]` (CommunityToolkit.Mvvm). Holds the current pattern, selected color, dimensions and commands for New / Save / Load.
- **`View/MainPage.xaml`** — UI: toolbar (dimensions, color picker, Save/Load), `ScrollView` with a dynamically generated grid of buttons.
- **`Services/`**
  - `PatternParser` — serializes `Pattern` ↔ string (CSV-like).
  - `PatternFileManager` — wraps the parser for file IO.
  - `ThreadColorCsvLoader` — parses the bundled DMC color CSV.

## Prerequisites

- **.NET 10 SDK** with the MAUI workload (`dotnet workload install maui`).
- For Windows builds: Windows 10 1809+ and Windows App SDK.

## Run

In Rider / Visual Studio open `CrossStichDrawer.sln` and run `CrossStichDrawer` (Windows Machine).

From the CLI:

```bash
dotnet build CrossStichDrawer.sln
dotnet build CrossStichDrawer/CrossStichDrawer.csproj -t:Run -f net10.0-windows10.0.19041.0
```

## Layout

```
CrossStichDrawer/
├── CrossStichDrawer.sln
└── CrossStichDrawer/
    ├── App.xaml(.cs), AppShell.xaml(.cs), MauiProgram.cs
    ├── Models/      (Pattern, PatternCell, ThreadColor)
    ├── ViewModel/   (MainViewModel)
    ├── View/        (MainPage.xaml + code-behind)
    ├── Services/    (PatternParser, PatternFileManager, ThreadColorCsvLoader)
    ├── Resources/   (DMC color CSV, icons, fonts, splash)
    └── Platforms/   (per-platform startup)
```
