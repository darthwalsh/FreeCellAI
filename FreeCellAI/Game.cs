using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeCellAI
{
  class Game {
    readonly List<List<Card>> tableau;
    readonly Dictionary<Suite, int> foundations;
    readonly List<Card?> freeCells = Enumerable.Repeat<Card?>(null, 4).ToList();

    public Game(IEnumerable<IEnumerable<Card>> tableau) {
      var allCards = tableau.SelectMany(col => col);
      if (allCards.Count() != 52) {
        throw new ArgumentException($"{allCards.Count()} cards");
      }
      var dups = allCards.GroupBy(c => c).Where(g => g.Count() > 1);
      if (dups.Any()) {
        throw new ArgumentException($"{dups.First().Key} was repeated!");
      }

      tableau = tableau.Select(col => col.ToList()).ToList();
      foundations = Enum.GetValues(typeof(Suite)).Cast<Suite>().ToDictionary(s => s, s => 0);
    }
  }
}