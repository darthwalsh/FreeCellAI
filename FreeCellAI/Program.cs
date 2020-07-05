using System;
using System.Drawing;
using System.Threading.Tasks;
using ImageParse;
using Parser;

namespace FreeCellAI
{
  class Program
  {
    async static Task Main(string[] args) {
      var text = @"
0H 0D 9C 7S 8H AS 6S 3S
8D 4C 3D 2D AD AH QH 4S
9D 9S 8S 9H 8C KH JS AC
JD 7D 0C 7C KD 4H 5D QC
4D 7H KC 5H KS 2C QS 6H
2H 2S 3H QD 5S 6C 5C 3C
6D 0S JC JH            ";

      for (var i = 0; i < args.Length; i++) {
        var arg = args[i].ToLower();
        switch (arg) {
          case "--prioritydelta":
            ++i;
            Game.PriorityDelta = int.Parse(args[i]);
            continue;
        }

        if (arg.EndsWith(".png")) {
          var bitmap = new WrappingBitmap {
            Bitmap = new Bitmap(args[i])
          };
          text = await new ScreenParser(bitmap).Parse();
          Console.WriteLine(text);
          continue;
        }

        throw new Exception($"Unknown arg {arg}");
      }
      
      var g = Importer.FromString(text);
      if (!Solver.TrySolve(g, out var solution)) {
        Console.Error.WriteLine("No Solution");
        Environment.Exit(1);
      }
      for (var i = 0; solution != null; solution = solution.Previous, i++) {
        Console.WriteLine(solution.Game);
        Console.WriteLine("");
        Console.WriteLine($"{i}:  {solution.Move}");
        Console.WriteLine("");
      }
    }
  }
}

