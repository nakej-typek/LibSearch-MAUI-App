using Expedition178.Actors;

namespace Expedition178.Battles;

public interface IBattle
{
    /// <summary>
    /// Performs a battle between the player's team of adventurers and a team of enemy creatures.
    /// </summary>
    /// <param name="adventurers">The player's team</param>
    /// <param name="monsters">The enemy team</param>
    /// <returns>The winner</returns>
    BattleResult OneBattle(IEnumerable<Adventurer> adventurers, IEnumerable<Monster> monsters);
    
    /// <summary>
    /// Performs one round of battle between two characters.
    /// </summary>
    /// <param name="adventurer">The player's adventurer</param>
    /// <param name="monster">The enemy creature</param>
    /// <returns>The character that wins the round</returns>
    RoundResult OneRound(Adventurer adventurer, Monster monster);
}