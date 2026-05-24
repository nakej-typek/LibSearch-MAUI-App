# HW00 — MyDreamTimeTableApp

First homework for **PB178 Programming in C#** (FI MUNI, spring 2026).
Author: J. Prosecký.

A .NET 9 console application whose goal was to practice C# fundamentals: classes, properties, constructors, collections and working with built-in types from `System` (e.g. `TimeOnly`, `TimeSpan`, `DayOfWeek`).

## Assignment

Model a personal weekly class schedule — design a data model for a subject, a seminar group and the overall weekly timetable, and populate it with sample data in `Main`.

## Domain model

- **`Subject`** — a subject: code, name, start/end time (used to compute `Duration`), teacher, array of `SeminarGroup`s.
- **`SeminarGroup`** — a seminar group: number and a list of tutors.
- **`Timetable`** — a container mapping `DayOfWeek` to a list of `Subject`s (dictionary-backed), with `SetDay` and `GetDaySchedule`.

## Run

```bash
cd MyDreamTimeTableApp
dotnet run --project MyDreamTimeTableApp
```

The app has no interactive output — it serves as a demonstration of the data model; sample outputs can be uncommented in `Program.cs`.

## Layout

```
MyDreamTimeTableApp/
├── MyDreamTimeTableApp.sln
└── MyDreamTimeTableApp/
    ├── Program.cs         (sample timetable population)
    ├── Timetable.cs
    ├── Subject.cs
    └── SeminarGroup.cs
```
