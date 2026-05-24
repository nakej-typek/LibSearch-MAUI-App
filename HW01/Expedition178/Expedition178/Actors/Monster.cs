using Expedition178.Actors;

namespace Expedition178;
using static GameConstants;

public class Monster(string name, int attack, int hp, int speed, MonsterType monsterType)
    : Entity(name, attack, hp, speed)
{
    private static readonly MonsterType[] monsterTypes = Enum.GetValues<MonsterType>();
    public MonsterType MonsterType { get; } = monsterType;

    public Monster(Entity entity, int round, string nameSuffix) : this(
        entity.Name,
        entity.Attack + round * (random.Next(MonsterAttackBonusMin, MonsterAttackBonusMax + 1) + round),
        entity.Hp + round * (random.Next(MonsterHpBonusMin, MonsterHpBonusMax + 1) + round),
        entity.Speed + round * random.Next(MonsterSpeedBonusMin, MonsterSpeedBonusMax + 1),
        RandomMonsterType())
    {
        this.Name = this.MonsterType.ToString() + nameSuffix;
    }

    private static MonsterType RandomMonsterType() => monsterTypes[random.Next(monsterTypes.Length)];

    public override string ToString()
    {
        return $"{Name} ({MonsterType}): {Attack} Attack, {Hp} HP, {Speed} Speed";
    }
}