using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeCellAI
{
  class Game  : ICloneable {
    readonly List<List<Card>> tableau;
    readonly Dictionary<Suite, int> foundations;
    readonly List<Card?> freeCells;

    public Game(IEnumerable<IEnumerable<Card>> tableau) : this(
      tableau,
      Enum.GetValues(typeof(Suite)).Cast<Suite>().ToDictionary(s => s, s => 0),
      Enumerable.Repeat<Card?>(null, 4).ToList()) { }

    Game(IEnumerable<IEnumerable<Card>> tableau, Dictionary<Suite, int> foundations, List<Card?> freeCells) {
      var allCards = tableau.SelectMany(col => col);
      if (allCards.Count() != 52) {
        throw new ArgumentException($"{allCards.Count()} cards");
      }
      var dups = allCards.GroupBy(c => c).Where(g => g.Count() > 1);
      if (dups.Any()) {
        throw new ArgumentException($"{dups.First().Key} was repeated!");
      }

      this.tableau = tableau.Select(col => col.ToList()).ToList();
      this.foundations = foundations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
      this.freeCells = freeCells.ToList();
    }

    public override string ToString() {
      var strings = tableau.Select(col => col.Select(c => c.ToString()).ToList());
      var rows = tableau.Max(col => col.Count);
      var lines = Enumerable.Range(0, rows).Select(i => string.Join(" ", 
        strings.Select(col => i < col.Count ? col[i] : "  ")));
      return string.Join(Environment.NewLine, lines);
    }

    // If cloning is a bottleneck, consider switching to immutable stack implementation to reduce copying
    internal Game Clone() => new Game(tableau, foundations, freeCells);
    object ICloneable.Clone() => Clone();
  }
}