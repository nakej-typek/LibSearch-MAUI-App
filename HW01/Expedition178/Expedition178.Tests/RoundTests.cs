using Expedition178.Actors;
using Expedition178.Battles;
using Expedition178.IO;

namespace Expedition178.Tests;

public class OneRoundTests
{
    private static readonly MyIio io = new MyIio();
    private readonly Battle _battleground = new Battle(io);
                    
    // ==================== BASIC COMBAT MECHANICS ====================

    [Fact]
    public void OneRound_FasterAdventurerAttacksFirst_KillsMonster()
    {
        // Adventurer: speed 10, attack 20 -> will kill the monster (5 HP) before it attacks
        var adventurer = new Adventurer("TestHero", 20, 50, 10, AttackType.Physical);
        var monster = new Monster("TestMonster", 5, 5, 1, MonsterType.Nature);
            
        _battleground.OneRound(adventurer, monster);
        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.PlayerWin, result);
        Assert.True(monster.CurHp <= 0);
    }

    [Fact]
    public void OneRound_FasterMonsterAttacksFirst_KillsAdventurer()
    {
        // Monster: speed 10, attack 100 -> kills adventurer (5 HP) before it attacks
        var adventurer = new Adventurer("TestHero", 20, 5, 1, AttackType.Physical);
        var monster = new Monster("TestMonster", 100, 50, 10, MonsterType.Nature);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.EnemyWin, result);
        Assert.True(adventurer.CurHp <= 0);
    }

    [Fact]
    public void OneRound_EqualSpeed_AdventurerAttacksFirst()
    {
        // Same speed=5 -> adventurer attacks first, kills monster instantly
        var adventurer = new Adventurer("TestHero", 100, 5, 5, AttackType.Physical);
        var monster = new Monster("TestMonster", 100, 5, 5, MonsterType.Nature);

        var result = _battleground.OneRound(adventurer, monster);

        // Adventurer attacks first (same speed), deals 100 damage, monster dies
        Assert.Equal(RoundResult.PlayerWin, result);
        Assert.True(monster.CurHp <= 0);
        // Adventurer should still have full HP since they struck first and killed the monster
        Assert.Equal(5, adventurer.CurHp);
    }

    [Fact]
    public void OneRound_MultiTurnCombat_AdventurerWins()
    {
        // Adventurer: 3 atk, 20 hp, 5 speed; Monster: 2 atk, 8 hp, 3 speed
        // Adventurer faster. Rounds: 3,3,3 -> monster dies after ceil(8/3)=3 hits
        var adventurer = new Adventurer("Hero", 3, 20, 5, AttackType.Physical);
        var monster = new Monster("Goblin", 2, 8, 3, MonsterType.Nature);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.PlayerWin, result);
        Assert.True(monster.CurHp <= 0);
        // Adventurer took some damage but survived
        Assert.True(adventurer.CurHp > 0);
    }

    [Fact]
    public void OneRound_MultiTurnCombat_MonsterWins()
    {
        // Adventurer: 1 atk, 5 hp, 5 speed; Monster: 3 atk, 50 hp, 3 speed
        var adventurer = new Adventurer("WeakHero", 1, 5, 5, AttackType.Physical);
        var monster = new Monster("BigMonster", 3, 50, 3, MonsterType.Nature);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.EnemyWin, result);
        Assert.True(adventurer.CurHp <= 0);
    }

    // ==================== DAMAGE MODIFIER / TYPE EFFECTIVENESS ====================

    [Fact]
    public void OneRound_Weakness_FireVsNature_DealsBonusDamage()
    {
        // Fire vs Nature = weakness = ceil(1.5 * atk)
        // atk=5 -> ceil(7.5) = 8 damage per hit
        var adventurer = new Adventurer("FireMage", 5, 50, 10, AttackType.Fire);
        var monster = new Monster("PlantBeast", 1, 8, 1, MonsterType.Nature);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.PlayerWin, result);
        // 8 damage in one hit kills the monster (8 HP)
        Assert.True(monster.CurHp <= 0);
    }

    [Fact]
    public void OneRound_Weakness_IceVsNature_DealsBonusDamage()
    {
        // Ice vs Nature = weakness = ceil(1.5 * 3) = 5
        var adventurer = new Adventurer("IceMage", 3, 50, 10, AttackType.Ice);
        var monster = new Monster("TreeEnt", 1, 10, 1, MonsterType.Nature);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.PlayerWin, result);
        // ceil(3 * 1.5) = 5 per hit. Hero faster (spd 10 vs 1).
        // Turn 1: hero deals 5, monster at 5 HP. Monster deals 1, hero at 49.
        // Turn 2: hero deals 5, monster at 0. Dead.
        Assert.Equal(49, adventurer.CurHp);
    }

    [Fact]
    public void OneRound_Weakness_DarkVsRadiant_DealsBonusDamage()
    {
        // Dark vs Radiant = weakness = ceil(1.5 * 4) = 6
        var adventurer = new Adventurer("Shadow", 4, 50, 10, AttackType.Dark);
        var monster = new Monster("Angel", 1, 6, 1, MonsterType.Radiant);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.PlayerWin, result);
        // 6 damage one-shots the 6 HP monster
        Assert.Equal(50, adventurer.CurHp);
    }

    [Fact]
    public void OneRound_Weakness_LightVsShadow_DealsBonusDamage()
    {
        // Light vs Shadow = weakness = ceil(1.5 * 4) = 6
        var adventurer = new Adventurer("Paladin", 4, 50, 10, AttackType.Light);
        var monster = new Monster("Shade", 1, 6, 1, MonsterType.Shadow);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.PlayerWin, result);
        Assert.Equal(50, adventurer.CurHp);
    }

    [Fact]
    public void OneRound_Resistance_LightVsNature_DealsReducedDamage()
    {
        // Light vs Nature = resistance = floor(0.5 * atk)
        // atk=5 -> floor(2.5) = 2
        var adventurer = new Adventurer("LightUser", 5, 50, 10, AttackType.Light);
        var monster = new Monster("TreeEnt", 1, 5, 1, MonsterType.Nature);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.PlayerWin, result);
        // floor(5*0.5)=2 per hit, needs ceil(5/2)=3 hits. Monster attacks twice: hero 50-2=48
        Assert.Equal(48, adventurer.CurHp);
    }

    [Fact]
    public void OneRound_Resistance_PhysicalVsShadow_DealsReducedDamage()
    {
        // Physical vs Shadow = resistance = floor(0.5 * 6) = 3
        var adventurer = new Adventurer("Warrior", 6, 50, 10, AttackType.Physical);
        var monster = new Monster("Shade", 1, 7, 1, MonsterType.Shadow);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.PlayerWin, result);
        // floor(6*0.5)=3 per hit, needs ceil(7/3)=3 hits. Monster attacks twice.
        Assert.Equal(48, adventurer.CurHp);
    }

    [Fact]
    public void OneRound_Immunity_LightVsRadiant_DealsZeroDamage()
    {
        // Light vs Radiant = immunity = 0 damage
        var adventurer = new Adventurer("Cleric", 10, 5, 10, AttackType.Light);
        var monster = new Monster("Angel", 3, 50, 1, MonsterType.Radiant);

        var result = _battleground.OneRound(adventurer, monster);

        // Adventurer can never kill the monster (0 damage), but monster eventually kills adventurer
        Assert.Equal(RoundResult.EnemyWin, result);
        Assert.True(adventurer.CurHp <= 0);
        // Monster should still have full HP
        Assert.Equal(50, monster.CurHp);
    }

    [Fact]
    public void OneRound_Immunity_DarkVsShadow_DealsZeroDamage()
    {
        // Dark vs Shadow = immunity = 0 damage
        var adventurer = new Adventurer("Rogue", 10, 5, 10, AttackType.Dark);
        var monster = new Monster("Shade", 3, 50, 1, MonsterType.Shadow);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.EnemyWin, result);
        Assert.Equal(50, monster.CurHp);
    }

    [Fact]
    public void OneRound_NeutralDamage_NoModifier()
    {
        // Physical vs Nature = no special interaction = 100% damage
        var adventurer = new Adventurer("Fighter", 5, 50, 10, AttackType.Physical);
        var monster = new Monster("Beast", 1, 5, 1, MonsterType.Nature);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.PlayerWin, result);
        // 5 damage kills 5 HP monster in one hit
        Assert.Equal(50, adventurer.CurHp);
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void OneRound_MonsterDiesExactlyAtZeroHp()
    {
        // attack exactly equals HP
        var adventurer = new Adventurer("Hero", 10, 50, 10, AttackType.Physical);
        var monster = new Monster("Goblin", 1, 10, 1, MonsterType.Nature);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.PlayerWin, result);
        Assert.Equal(0, monster.CurHp);
    }

    [Fact]
    public void OneRound_AdventurerDiesExactlyAtZeroHp()
    {
        var adventurer = new Adventurer("WeakHero", 1, 10, 1, AttackType.Physical);
        var monster = new Monster("BigBoss", 10, 50, 10, MonsterType.Nature);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.EnemyWin, result);
        Assert.Equal(0, adventurer.CurHp);
    }

    [Fact]
    public void OneRound_WeaknessRoundsUp_OddAttack()
    {
        // Fire vs Nature, atk = 3 -> ceil(3 * 1.5) = ceil(4.5) = 5
        var adventurer = new Adventurer("OddFire", 3, 50, 10, AttackType.Fire);
        var monster = new Monster("Plant", 1, 5, 1, MonsterType.Nature);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.PlayerWin, result);
        // 5 damage one-shots the 5 HP monster
        Assert.Equal(50, adventurer.CurHp);
    }

    [Fact]
    public void OneRound_ResistanceRoundsDown_OddAttack()
    {
        // Light vs Nature, atk = 3 -> floor(3 * 0.5) = floor(1.5) = 1
        var adventurer = new Adventurer("WeakLight", 3, 50, 10, AttackType.Light);
        var monster = new Monster("Plant", 1, 3, 1, MonsterType.Nature);

        var result = _battleground.OneRound(adventurer, monster);

        Assert.Equal(RoundResult.PlayerWin, result);
        // floor(3*0.5)=1 per hit, 3 hits needed. Monster attacks twice.
        Assert.Equal(48, adventurer.CurHp);
    }
}

