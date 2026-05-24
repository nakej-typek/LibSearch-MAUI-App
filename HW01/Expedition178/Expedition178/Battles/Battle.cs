using Expedition178.Actors;
using Expedition178.IO;

namespace Expedition178.Battles;

public class Battle(IIO io) : IBattle
{
    public BattleResult OneBattle(IEnumerable<Adventurer> players, IEnumerable<Monster> enemies)
    {
        var playerList = players.ToList();
        var enemyList = enemies.ToList();
        int p = 0, e = 0;
        while (true)
        {
            var res = OneRound(playerList[p], enemyList[e]);
            if (res == RoundResult.PlayerWin)
            {
                e++;
                if (e == enemyList.Count)
                {
                    return BattleResult.PlayersWin;
                }
            }
            else
            {
                p++;
                if (p == playerList.Count)
                {
                    return BattleResult.EnemiesWin;
                }
            }
        }
    }

    private RoundResult? PerformAttack(Entity attacker, Entity defender, int damage, RoundResult attackerWinResult)
    {
        io.Log($"{attacker.Name} dealt {damage} damage to {defender.Name}.");
        defender.TakeDamage(damage);

        if (defender.CurHp <= 0)
        {
            io.Log($"{defender.Name} was defeated by {attacker.Name}.");
            return attackerWinResult;
        }

        io.Log($"{defender.Name} currently has {defender.CurHp} HP.");
        return null;
    }


    public RoundResult OneRound(Adventurer player, Monster enemy)
    {
        bool monsterFirst = enemy.Speed > player.Speed;

        while (true)
        {
            RoundResult? result;

            if (monsterFirst)
            {
                result = PerformAttack(enemy, player, enemy.Attack, RoundResult.EnemyWin);
            }
            else
            {
                int damage = player.DamageModifier(player, enemy, player.Attack);
                result = PerformAttack(player, enemy, damage, RoundResult.PlayerWin);
            }

            if (result is not null)
            {
                return result.Value;
            }

            monsterFirst = !monsterFirst;
        }
    }
}