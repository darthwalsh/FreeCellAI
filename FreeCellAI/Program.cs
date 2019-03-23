using System;

namespace FreeCellAI
{
  class Program
  {
    static void Main(string[] args) {
      const string text = @"
0H 0D 9C 7S 8H AS 6S 3S
8D 4C 3D 2D AD AH QH 4S
9D 9S 8S 9H 8C KH JS AC
JD 7D 0C 7C KD 4H 5D QC
4D 7H KC 5H KS 2C QS 6H
2H 2S 3H QD 5S 6C 5C 3C
6D 0S JC JH            ";
      var g = Importer.FromString(text);
      if (!Solver.TrySolve(g, out var solution)) {
        Console.Error.WriteLine("No Solution");
        Environment.Exit(1);
      }
      for (; solution != null; solution = solution.Previous) {
        Console.WriteLine(solution.Game);
        Console.WriteLine("");
        Console.WriteLine(solution.Move);
        Console.WriteLine("");
      }
    }
  }
}

