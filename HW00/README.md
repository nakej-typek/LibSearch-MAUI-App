# HW00 — MyDreamTimeTableApp

První domácí úkol pro **PB178 Programování v jazyce C#** (FI MUNI, jaro 2026).
Autor: J. Prosecký.

Konzolová aplikace v .NET 9, jejímž cílem bylo procvičit základy C#: třídy, vlastnosti, konstruktory, kolekce a práci s typy z `System` (např. `TimeOnly`, `TimeSpan`, `DayOfWeek`).

## Zadání

Vymodelovat vlastní rozvrh přednášek a seminářů — vytvořit datový model pro předmět, seminární skupinu a celkový týdenní rozvrh, a v `Main` ho naplnit ukázkovými daty.

## Doménový model

- **`Subject`** — předmět: kód, název, čas začátku/konce (z nichž se počítá `Duration`), vyučující, pole `SeminarGroup`.
- **`SeminarGroup`** — seminární skupina: číslo a seznam tutorů.
- **`Timetable`** — kontejner mapující `DayOfWeek` na seznam `Subject` (slovník), s metodami `SetDay` a `GetDaySchedule`.

## Spuštění

```bash
cd MyDreamTimeTableApp
dotnet run --project MyDreamTimeTableApp
```

Aplikace nemá interaktivní výstup — slouží jako demonstrace datového modelu, výstupy lze odkomentovat v `Program.cs`.

## Struktura

```
MyDreamTimeTableApp/
├── MyDreamTimeTableApp.sln
└── MyDreamTimeTableApp/
    ├── Program.cs         (ukázkové naplnění rozvrhu)
    ├── Timetable.cs
    ├── Subject.cs
    └── SeminarGroup.cs
```
