using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeCellAI
{
  public static class Solver
  {
    public static bool TrySolve(Game g, out Change solution) {
      var seen = new HashSet<Game>();
      var toSearch = new C5.IntervalHeap<Change> {
          new Change {
            Game = g,
          }
      };
      while (toSearch.Any()) {
        var current = toSearch.FindMin();
        toSearch.DeleteMin();
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
          toSearch.Add(next);
        }
      }
      solution = null;
      return false;
    }
  }

  public class Change : IComparable<Change>
  {
    public Change Previous { get; set; }
    public Game Game { get; set; }
    public Move Move { get; set; }

    public int CompareTo(Change other) => Game.CompareTo(other.Game);
  }
}
