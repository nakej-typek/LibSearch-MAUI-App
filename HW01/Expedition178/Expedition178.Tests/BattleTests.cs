using Expedition178.Actors;
using Expedition178.Battles;
using Expedition178.IO;

namespace Expedition178.Tests;

public class OneBattleTests 
    {
    private static readonly MyIio io = new MyIio();
    private readonly Battle _battleground = new Battle(io);

    // ==================== BASIC BATTLE OUTCOMES ====================

    [Fact]
    public void OneBattle_StrongAdventurers_WinBattle()
    {
        var players = new List<Adventurer>
        {
            new("Hero1", 20, 50, 10, AttackType.Physical),
            new("Hero2", 20, 50, 10, AttackType.Fire),
            new("Hero3", 20, 50, 10, AttackType.Dark)
        };
        var enemies = new List<Monster>
        {
            new("Goblin1", 1, 5, 1, MonsterType.Nature),
            new("Goblin2", 1, 5, 1, MonsterType.Nature),
            new("Goblin3", 1, 5, 1, MonsterType.Nature)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.PlayersWin, result);
    }

    [Fact]
    public void OneBattle_StrongMonsters_WinBattle()
    {
        var players = new List<Adventurer>
        {
            new("WeakHero1", 1, 5, 1, AttackType.Physical),
            new("WeakHero2", 1, 5, 1, AttackType.Physical),
            new("WeakHero3", 1, 5, 1, AttackType.Physical)
        };
        var enemies = new List<Monster>
        {
            new("Boss1", 50, 100, 10, MonsterType.Nature),
            new("Boss2", 50, 100, 10, MonsterType.Nature),
            new("Boss3", 50, 100, 10, MonsterType.Nature)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.EnemiesWin, result);
    }

    [Fact]
    public void OneBattle_FirstAdventurerSweepsAllMonsters()
    {
        var players = new List<Adventurer>
        {
            new("Sweeper", 100, 200, 10, AttackType.Physical),
            new("Backup1", 5, 20, 5, AttackType.Fire),
            new("Backup2", 5, 20, 5, AttackType.Ice)
        };
        var enemies = new List<Monster>
        {
            new("Weak1", 1, 5, 1, MonsterType.Nature),
            new("Weak2", 1, 5, 1, MonsterType.Nature),
            new("Weak3", 1, 5, 1, MonsterType.Nature)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.PlayersWin, result);
        // Backup heroes should still have full HP
        Assert.Equal(20, players[1].CurHp);
        Assert.Equal(20, players[2].CurHp);
    }

    [Fact]
    public void OneBattle_FirstMonsterSweepsAllAdventurers()
    {
        var players = new List<Adventurer>
        {
            new("Weak1", 1, 5, 1, AttackType.Light),
            new("Weak2", 1, 5, 1, AttackType.Light),
            new("Weak3", 1, 5, 1, AttackType.Light)
        };
        var enemies = new List<Monster>
        {
            // Immune to Light and fast, so kills all adventurers with 0 damage taken
            new("ImmuneAngel", 50, 200, 10, MonsterType.Radiant),
            new("Backup1", 5, 20, 5, MonsterType.Nature),
            new("Backup2", 5, 20, 5, MonsterType.Nature)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.EnemiesWin, result);
        // Backup monsters should still have full HP
        Assert.Equal(20, enemies[1].CurHp);
        Assert.Equal(20, enemies[2].CurHp);
    }

    // ==================== ADVENTURER ROTATION ====================

    [Fact]
    public void OneBattle_AdventurerDies_NextOneStepsUp()
    {
        var players = new List<Adventurer>
        {
            new("Fragile", 5, 3, 10, AttackType.Physical), // dies fast
            new("Tank", 10, 100, 10, AttackType.Physical),   // cleans up
            new("Reserve", 5, 50, 5, AttackType.Fire)
        };
        var enemies = new List<Monster>
        {
            new("Strong", 10, 20, 5, MonsterType.Nature),
            new("Weak1", 1, 3, 1, MonsterType.Nature),
            new("Weak2", 1, 3, 1, MonsterType.Nature)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.PlayersWin, result);
        // First adventurer should be dead
        Assert.True(players[0].CurHp <= 0);
        // Second should have taken the brunt
        Assert.True(players[1].CurHp > 0);
    }

    [Fact]
    public void OneBattle_MonsterDies_NextOneStepsUp()
    {
        var players = new List<Adventurer>
        {
            new("Hero", 10, 100, 10, AttackType.Physical),
            new("Hero2", 10, 100, 10, AttackType.Fire),
            new("Hero3", 10, 100, 10, AttackType.Dark)
        };
        var enemies = new List<Monster>
        {
            new("Weak", 5, 5, 1, MonsterType.Nature),
            new("Medium", 5, 15, 1, MonsterType.Nature),
            new("Strong", 5, 30, 1, MonsterType.Nature)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.PlayersWin, result);
        // All monsters should be dead
        Assert.True(enemies.All(m => m.CurHp <= 0));
    }

    // ==================== TYPE EFFECTIVENESS IN BATTLE ====================

    [Fact]
    public void OneBattle_TypeAdvantage_PlayerWinsDespiteWeakerStats()
    {
        // Dark adventurer vs Radiant monster: 150% damage
        var players = new List<Adventurer>
        {
            new("DarkKnight", 4, 20, 5, AttackType.Dark),
            new("Backup1", 4, 20, 5, AttackType.Dark),
            new("Backup2", 4, 20, 5, AttackType.Dark)
        };
        // Dark vs Radiant = weakness: ceil(4 * 1.5) = 6 per hit
        var enemies = new List<Monster>
        {
            new("Angel1", 3, 12, 4, MonsterType.Radiant),
            new("Angel2", 3, 12, 4, MonsterType.Radiant),
            new("Angel3", 3, 12, 4, MonsterType.Radiant)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.PlayersWin, result);
    }

    [Fact]
    public void OneBattle_TypeImmunity_MonsterWins()
    {
        // Light vs Radiant = immunity
        var players = new List<Adventurer>
        {
            new("LightMage1", 10, 10, 5, AttackType.Light),
            new("LightMage2", 10, 10, 5, AttackType.Light),
            new("LightMage3", 10, 10, 5, AttackType.Light)
        };
        var enemies = new List<Monster>
        {
            new("Angel1", 5, 50, 6, MonsterType.Radiant),
            new("Angel2", 5, 50, 6, MonsterType.Radiant),
            new("Angel3", 5, 50, 6, MonsterType.Radiant)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.EnemiesWin, result);
    }

    // ==================== MIXED TYPE SCENARIOS ====================

    [Fact]
    public void OneBattle_MixedMonsterTypes_CorrectDamageApplied()
    {
        // Fire adventurer: strong vs Nature (150%), neutral vs Radiant (100%), neutral vs Shadow (100%)
        var players = new List<Adventurer>
        {
            new("FireKnight", 10, 100, 10, AttackType.Fire),
            new("Backup1", 10, 50, 5, AttackType.Physical),
            new("Backup2", 10, 50, 5, AttackType.Ice)
        };
        var enemies = new List<Monster>
        {
            new("NatureBeast", 2, 15, 1, MonsterType.Nature),   // Fire is super effective: ceil(15)=15
            new("RadiantBeing", 2, 10, 1, MonsterType.Radiant),
            new("ShadowWraith", 2, 10, 1, MonsterType.Shadow)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.PlayersWin, result);
    }

    // ==================== HP AND DAMAGE TRACKING ====================

    [Fact]
    public void OneBattle_DamageIsPersistentAcrossRounds()
    {
        // First adventurer weakens the monster, second finishes it off
        var players = new List<Adventurer>
        {
            new("Weakener", 5, 6, 10, AttackType.Physical), // deals 5 damage, dies to 10
            new("Finisher", 5, 50, 10, AttackType.Physical),
            new("Reserve", 5, 50, 5, AttackType.Physical)
        };
        var enemies = new List<Monster>
        {
            // 20 HP monster with 10 attack: kills first adventurer but loses HP
            new("Tank", 10, 20, 5, MonsterType.Nature),
            new("Minion1", 1, 3, 1, MonsterType.Nature),
            new("Minion2", 1, 3, 1, MonsterType.Nature)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.PlayersWin, result);
        // First adventurer should be dead (10 dmg > 6 HP)
        Assert.True(players[0].CurHp <= 0);
    }

    [Fact]
    public void OneBattle_AllAdventurersDie_EnemiesWin()
    {
        var players = new List<Adventurer>
        {
            new("Hero1", 2, 10, 3, AttackType.Physical),
            new("Hero2", 2, 10, 3, AttackType.Physical),
            new("Hero3", 2, 10, 3, AttackType.Physical)
        };
        var enemies = new List<Monster>
        {
            new("Boss", 20, 200, 10, MonsterType.Nature),
            new("Guard1", 10, 100, 5, MonsterType.Nature),
            new("Guard2", 10, 100, 5, MonsterType.Nature)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.EnemiesWin, result);
        Assert.True(players.All(p => p.CurHp <= 0));
    }

    [Fact]
    public void OneBattle_AllMonstersDie_PlayersWin()
    {
        var players = new List<Adventurer>
        {
            new("Hero1", 20, 100, 10, AttackType.Physical),
            new("Hero2", 20, 100, 10, AttackType.Fire),
            new("Hero3", 20, 100, 10, AttackType.Dark)
        };
        var enemies = new List<Monster>
        {
            new("Minion1", 1, 10, 1, MonsterType.Nature),
            new("Minion2", 1, 10, 1, MonsterType.Nature),
            new("Minion3", 1, 10, 1, MonsterType.Nature)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.PlayersWin, result);
        Assert.True(enemies.All(m => m.CurHp <= 0));
    }

    // ==================== EDGE: SINGLE ENTITY LISTS ====================

    [Fact]
    public void OneBattle_OneVsOne_PlayerWins()
    {
        var players = new List<Adventurer>
        {
            new("Solo", 10, 50, 10, AttackType.Physical)
        };
        var enemies = new List<Monster>
        {
            new("Lone", 1, 5, 1, MonsterType.Nature)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.PlayersWin, result);
    }

    [Fact]
    public void OneBattle_OneVsOne_MonsterWins()
    {
        var players = new List<Adventurer>
        {
            new("Solo", 1, 3, 1, AttackType.Light)
        };
        var enemies = new List<Monster>
        {
            // Immune to light, so adventurer deals 0 forever
            new("ImmuneMonster", 5, 50, 10, MonsterType.Radiant)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.EnemiesWin, result);
    }

    // ==================== WEAKNESS ROUNDING IN BATTLE CONTEXT ====================

    [Fact]
    public void OneBattle_WeaknessRoundingCeil_CorrectKillCount()
    {
        // Ice vs Nature: ceil(1.5 * 3) = 5 per hit
        var players = new List<Adventurer>
        {
            new("IceMage", 3, 100, 10, AttackType.Ice),
            new("Backup1", 3, 50, 5, AttackType.Ice),
            new("Backup2", 3, 50, 5, AttackType.Ice)
        };
        var enemies = new List<Monster>
        {
            new("Plant1", 1, 9, 1, MonsterType.Nature), // 5+5 = 10 > 9, dead in 2 hits
            new("Plant2", 1, 5, 1, MonsterType.Nature),  // 5 >= 5, dead in 1 hit
            new("Plant3", 1, 10, 1, MonsterType.Nature)  // 5+5 = 10 = 10, dead in 2 hits
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.PlayersWin, result);
    }

    [Fact]
    public void OneBattle_ResistanceRoundingFloor_BattleStillPossible()
    {
        // Physical vs Shadow: floor(0.5 * 10) = 5
        var players = new List<Adventurer>
        {
            new("Warrior", 10, 100, 10, AttackType.Physical),
            new("Backup1", 10, 50, 5, AttackType.Physical),
            new("Backup2", 10, 50, 5, AttackType.Physical)
        };
        var enemies = new List<Monster>
        {
            new("Shadow1", 2, 10, 1, MonsterType.Shadow), // 5+5=10, dead in 2 hits
            new("Shadow2", 2, 10, 1, MonsterType.Shadow),
            new("Shadow3", 2, 10, 1, MonsterType.Shadow)
        };

        var result = _battleground.OneBattle(players, enemies);

        Assert.Equal(BattleResult.PlayersWin, result);
    }
}


