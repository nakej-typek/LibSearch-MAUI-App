using System.Reflection.Metadata.Ecma335;

namespace Expedition178.Actors;
using static GameConstants;

public class Entity(string name, int attack, int hp, int speed)
{
    protected static readonly Random random = new();

    public string Name { get; protected init; } = name;
    public int Attack { get; protected set; } = attack;
    public int Hp { get; protected set; } = hp;
    public int CurHp { get; private set; } = hp;
    public int Speed { get; protected set; } = speed;

    public Entity() : this(
        random.GetString("joemom",6) + MonsterSuffixes[random.Next(MonsterSuffixes.Length)],
        random.Next(MinEntityAttack, MaxEntityAttack + 1),
        random.Next(MinEntityHp, MaxEntityHp + 1),
        random.Next(MinEntitySpeed, MaxEntitySpeed + 1))
    {
        Name = Name[0].ToString().ToUpper() + Name[1..];
    }
    public void TakeDamage(int damage)
    {
        CurHp -= damage;
        CurHp = Math.Max(CurHp, 0);
    }

    public void Heal()
    {
        CurHp = Hp;
    }

    public int DamageModifier(Adventurer adventurer, Monster monster, int damage)
    {
        var atk = adventurer.AttackType;
        var mon = monster.MonsterType;

        const AttackType light = AttackType.Light;
        const AttackType dark = AttackType.Dark;
        const AttackType fire = AttackType.Fire;
        const AttackType ice = AttackType.Ice;
        const AttackType physical = AttackType.Physical;

        const MonsterType nature = MonsterType.Nature;
        const MonsterType radiant = MonsterType.Radiant;
        const MonsterType shadow = MonsterType.Shadow;

        // Weakness: ceil(1.5 * damage)
        if ((atk == light && mon == shadow) ||
            (atk == dark && mon == radiant) ||
            (atk == fire && mon == nature) ||
            (atk == ice && mon == nature))
            return (int)Math.Ceiling(damage * 1.5);

        // Resistance: floor(0.5 * damage)
        if ((atk == light && mon == nature) ||
            (atk == physical && mon == shadow))
            return (int)Math.Floor(damage * 0.5);

        // Immunity: 0
        if ((atk == light && mon == radiant) ||
            (atk == dark && mon == shadow))
            return 0;

        return damage;
    }
}