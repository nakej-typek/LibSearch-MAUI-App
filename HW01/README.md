# HW01 — Expedition 178

Second homework for **PB178 Programming in C#** (FI MUNI, spring 2026).
Author: J. Prosecký.

A .NET 9 text-based console game in which the player picks three adventurers and fights successive waves of monsters. The goal of the assignment was to practice OOP in C#: inheritance, interfaces, I/O abstraction for testability, generics, `enum`s, primary constructors and LINQ.

## Gameplay

At startup the game generates six random adventurers. The player picks three with `start X Y Z` and fights successive waves of three monsters. Each fight is a turn-based 1v1 sequence in team order; the faster actor strikes first. After a won fight adventurers gain XP and may level up; after a lost fight they receive a compensation XP and the monsters are healed.

### Commands

| Command | Effect |
|---|---|
| `check` | print the current wave of monsters |
| `fight` | start a fight against the current wave |
| `info` | print adventurer stats and the number of cleared waves |
| `sort` | reorder the adventurers |
| `sus` | easter egg |
| `quit` | quit the game |

The player wins after clearing three waves (`MaxRound = 3`).

## Architecture

- **`Actors/`** — `Entity` (base class for anything with HP/Attack/Speed), `Adventurer` and `Monster` (derived), `AttackType` and `MonsterType` enums.
- **`Battles/`** — `IBattle` interface and `Battle` implementation, `BattleResult` / `RoundResult` enums.
- **`Game/`** — `IGame` / `Game` game loop, `GameConstants` (XP per level, suffixes, etc.).
- **`IO/`** — `IIO` I/O abstraction with a concrete console implementation `MyIio`. This abstraction makes the game logic unit-testable without touching `Console`.

## Tests

`Expedition178.Tests/` contains xUnit tests focused on game logic (fights, leveling, command parsing) using a mock IIO.

```bash
dotnet test Expedition178.sln
```

## Run

```bash
dotnet run --project Expedition178/Expedition178
```

## Layout

```
Expedition178/
├── Expedition178.sln
├── Expedition178/
│   ├── Program.cs
│   ├── Actors/        (Entity, Adventurer, Monster, AttackType, MonsterType)
│   ├── Battles/       (Battle + IBattle, RoundResult, BattleResult)
│   ├── Game/          (Game + IGame, GameConstants)
│   └── IO/            (IIO + MyIio console implementation, CommandOptions)
└── Expedition178.Tests/
```
