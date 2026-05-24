// See https://aka.ms/new-console-template for more information

using Expedition178.IO;

namespace Expedition178;

class Program
{
    public static void Main(string[] args)
    {
        new Game.Game(new MyIio()).Start();
    }
}