namespace Expedition178.Actors;
using static GameConstants;
public class Adventurer(string name, int attack, int hp, int speed, int lvl, AttackType attackType, int xp)
    : Entity(name, attack,
        hp, speed)
{
    public int Lvl { get; private set; } = lvl;
    public AttackType AttackType { get; } = attackType;
    public int Xp { get; private set; } = xp;

    public Adventurer(string name, int attack, int hp, int speed, AttackType attackType) :
        this(name, attack, hp, speed, 1, attackType, 0)
    {
    }
    
    public int AddXp(int xp)
    {
        Xp += xp;
        int levelsGained = Xp / XpPerLevel;
        while (Xp >= XpPerLevel)
        {
           Xp -= XpPerLevel;
           LevelUp();
        }

        return levelsGained;
    }

    public void LevelUp()
    {
        int sharedResource = 5 + Lvl;

        int hpBonus = random.Next(3 + Lvl, sharedResource);
        sharedResource -= hpBonus;
        int attackBonus = sharedResource;
        int speedBonus = random.Next(0, 2);

        Attack += attackBonus + Lvl;
        Hp += hpBonus + Lvl;
        Speed += speedBonus;
        Lvl++;
    }

    public override string ToString()
    {
        return $"{Name} ({AttackType}): {Attack} Attack, {CurHp}/{Hp} HP, {Speed} Speed, Level {Lvl} {Xp}/100 XP";
    }
}