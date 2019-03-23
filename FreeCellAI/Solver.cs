using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FreeCellAI
{
  static class Solver
  {
    public static bool TrySolve(Game g, out Change solution) {
      var seen = new HashSet<Game>();
      var toSearch = new Stack<Change>();
      toSearch.Push(new Change {
        Game = g,
      });
      while (toSearch.TryPop(out var current)) {
        if (!seen.Add(current.Game)) {
          continue;
        }
        foreach (var move in current.Game.GetOptimizedMoves()) {
          var clone = current.Game.Clone();
          clone.MoveCard(move);
          var next = new Change {
            Previous = current,
            Game = clone,
            Move = move,
          };
          if (clone.Solved) {
            solution = next;
            return true;
          }
          toSearch.Push(next);
        }
      }
      solution = null;
      return false;
    }
  }

  class Change
  {
    public Change Previous { get; set; }
    public Game Game { get; set; }
    public Move Move { get; set; }
  }
}
