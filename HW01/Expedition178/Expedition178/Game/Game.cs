using Expedition178.Actors;
using Expedition178.Battles;
using Expedition178.IO;

namespace Expedition178.Game;
using static GameConstants;

public class Game : IGame
{
    private readonly IIO _io;
    private List<Adventurer> AdventurersPool { get; }
    private List<Adventurer> Adventurers { get; set; }
    private List<Monster> Monsters { get; set; }
    private Battle Battleground { get; }
    private int Round { get; set; }
    private const int MaxRound = 3;
    
    public Game(IIO io)
    {
        _io = io;
        AdventurersPool = [];
        Adventurers = [];
        Monsters = [];
        
        for (int i = 0; i < 6; i++)
        {
            Entity e = new Entity();
            AttackType genAttackType = (AttackType)((i + e.Name[0]) % Enum.GetValues<AttackType>().Length);
            Adventurer a = new Adventurer(e.Name, e.Attack, e.Hp, e.Speed, genAttackType);
            AdventurersPool.Add(a);
        }
        
        Battleground = new Battle(_io);
        io.Log("Welcome to Expedition 178! Please, choose your three adventurers:");
        for (int i = 0; i < 6; i++)
        {
            io.Log($"{i + 1}. " + AdventurersPool[i]);
        }

        io.Log("Whenever you're ready, please enter the start command.");
    }

    public void Start()
    {
        AdventureStart();
        NextMonsters();
        while (true)
        {
            var stropt = _io.GetMsg() ?? string.Empty;
            stropt = stropt.Trim().ToLower();
            switch (stropt)
            {
                case "check":
                    Check();
                    break;

                case "fight":
                    if (Fight()) return;
                    break;

                case "info":
                    Info();
                    break;

                case "sus":
                    Sus();
                    break;

                case "sort":
                    Sort();
                    break;

                case "quit":
                    _io.Log("Quiting game.");
                    return;

                default:
                    if (stropt.Contains("check") || stropt.Contains("fight") ||
                        stropt.Contains("info") || stropt.Contains("sort") ||
                        stropt.Contains("quit"))
                    {
                        _io.Log("Invalid command format.");
                    }
                    else
                    {
                        _io.Log("Invalid command.");
                    }

                    continue;
            }
        }
    }
    
    private void AdventureStart()
    {
        _io.StartArgsParser(out var a, out var b, out var c, 6);
        Adventurers = [AdventurersPool[a], AdventurersPool[b], AdventurersPool[c]];
        _io.Log($"You have chosen:\n" + string.Join("\n", Adventurers.Select(s => s.Name)));
        _io.Log("Your commands are: check, fight, info, sort, quit.");
    }

    private void NextMonsters()
    {
        Monster m1 = new Monster(new Entity(), Round, MonsterSuffixes[0]);
        Monster m2 = new Monster(new Entity(), Round, MonsterSuffixes[1]);
        Monster m3 = new Monster(new Entity(), Round, MonsterSuffixes[2]);

        Monsters = [m1, m2, m3];
    }

    private void PrintActors<T>(IEnumerable<T> actors)
    {
        _io.Log(string.Join("\n", actors));
    }
    
    private void Check()
    {
        PrintActors(Monsters);    
    }
    
    private void Info()
    {
        _io.Log($"Waves defeated: {Round}");
        PrintActors(Adventurers);
    }

    private void Sus()
    {
        _io.Log("https://www.youtube.com/watch?v=mxYUg_BQAlw");
    }

    private bool Fight()
    {
        int levelsGained;
        var win = Battleground.OneBattle(Adventurers, Monsters);
        if (win == BattleResult.PlayersWin)
        {
            _io.Log("You won the battle! ");
            Round++;
            NextMonsters();
            levelsGained = Adventurers.Sum(adventurer => adventurer.AddXp(XpPerLevel));
        }
        else
        {
            int compensationXp = 10 + 10 * Monsters.Count(m => m.CurHp <= 0);
            _io.Log(
                $"You lost the battle, but your adventurers gained {compensationXp} XP for their efforts.");
            Monsters.ForEach(monster => monster.Heal());
            levelsGained = Adventurers.Sum(adventurer => adventurer.AddXp(compensationXp));
        }

        if (levelsGained > 0)
        {
            _io.Log("The adventurers leveled up! ");
        }

        if (Round == MaxRound)
        {
            _io.Log("\nCongratulations! You have completed the expedition!");
            _io.Log(
                "             ___________\n            '._==_==_=_.'\n            .-\\:      /-.\n           | (|:.     |) |\n            '-|:.     |-'\n              \\::.    /\n               '::. .'\n                 ) (\n               _.' '._\n              `\"\"\"\"\"\"\"`");
            _io.Log("Here are your final stats:");
            Info();
            return true;
        }

        Adventurers.ForEach(adventurer => adventurer.Heal());
        return false;
    }


    private void Sort()
    {
        _io.Log($"Choose the order:\n" + string.Join("\n", Adventurers));
        int a, b, c;
        _io.SortArgsParser(out a, out b, out c, 3);
        var temp = Adventurers;
        Adventurers = [temp[a], temp[b], temp[c]];
    }
}