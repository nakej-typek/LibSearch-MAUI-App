# HW01 — Expedition 178

Druhý domácí úkol pro **PB178 Programování v jazyce C#** (FI MUNI, jaro 2026).
Autor: J. Prosecký.

Konzolová textová hra v .NET 9, ve které hráč vybere tři dobrodruhy a postupně proti sobě posílá vlny monster. Cílem úkolu bylo procvičit OOP v C#: dědičnost, rozhraní, abstrakce I/O pro testovatelnost, generika, `enum`, primary constructors a LINQ.

## Hratelnost

Hra na začátku vygeneruje šest náhodných dobrodruhů. Hráč si tři z nich vybere příkazem `start X Y Z` a postupně bojuje proti vlnám tří monster. Boj probíhá jako tahový souboj 1v1 podle pořadí v týmu; rychlejší aktér útočí první. Po vítězném boji dobrodruzi získávají XP a levelují, po prohře dostanou kompenzační XP a monstra se uzdraví.

### Příkazy

| Příkaz | Význam |
|---|---|
| `check` | vypíše aktuální vlnu monster |
| `fight` | spustí boj proti aktuální vlně |
| `info` | vypíše stav dobrodruhů a počet poražených vln |
| `sort` | umožní změnit pořadí dobrodruhů |
| `sus` | velikonoční vajíčko |
| `quit` | konec hry |

Vítězství nastává po poražení tří vln (`MaxRound = 3`).

## Architektura

- **`Actors/`** — `Entity` (základní třída pro vše, co má HP/Attack/Speed), `Adventurer` a `Monster` (dědí z `Entity`), `AttackType` a `MonsterType` enumy.
- **`Battles/`** — `IBattle` rozhraní a `Battle` implementace, `BattleResult`/`RoundResult` enumy.
- **`Game/`** — `IGame`/`Game` herní smyčka, `GameConstants` (XP per level, suffixes apod.).
- **`IO/`** — `IIO` abstrakce I/O s konkrétní konzolovou implementací `MyIio`. Díky této abstrakci jsou herní třídy testovatelné bez `Console`.

## Testy

`Expedition178.Tests/` obsahuje xUnit testy zaměřené na herní logiku (boje, leveling, parsing příkazů) s nasazením mock IIO.

```bash
dotnet test Expedition178.sln
```

## Spuštění

```bash
dotnet run --project Expedition178/Expedition178
```

## Struktura

```
Expedition178/
├── Expedition178.sln
├── Expedition178/
│   ├── Program.cs
│   ├── Actors/        (Entity, Adventurer, Monster, AttackType, MonsterType)
│   ├── Battles/       (Battle + IBattle, RoundResult, BattleResult)
│   ├── Game/          (Game + IGame, GameConstants)
│   └── IO/            (IIO + MyIio konzolová implementace, CommandOptions)
└── Expedition178.Tests/
```
