using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeCellAI
{
  struct Card : IEquatable<Card> {
    static readonly Dictionary<char, int> ranks;
    static readonly Dictionary<int, char> fromRank;
    static readonly Dictionary<char, Suite> suites;
    static readonly Dictionary<Suite, char> fromSuite;

    static Card() {
      ranks = new Dictionary<char, int> {
        ['A'] = 1,
        ['0'] = 10,
        ['J'] = 11,
        ['Q'] = 12,
        ['K'] = 13,
      };
      for (var i = 2; i <= 9; ++i) {
        ranks.Add((char)('0' + i), i);
      }
      suites = Enum.GetValues(typeof(Suite)).Cast<Suite>().ToDictionary(s => Enum.GetName(typeof(Suite), s)[0]);

      fromRank = ranks.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
      fromSuite = suites.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    public int Rank { get; private set; }
    public Suite Suite { get; private set; }
    public Card(string card) {
      if (card.Length != 2) {
        throw new ArgumentException($"{nameof(card)} should be two chars");
      }
      if (!ranks.TryGetValue(card[0], out var r)) {
        throw new ArgumentOutOfRangeException($"{card[0]} is not a rank");
      }
      Rank = r;
      if (!suites.TryGetValue(card[1], out var s)) {
        throw new ArgumentOutOfRangeException($"{card[1]} is not a suite");
      }
      Suite = s;
    }

    public override bool Equals(object obj) => obj is Card && Equals((Card)obj);
    public bool Equals(Card other) => Rank == other.Rank && Suite == other.Suite;
    public override int GetHashCode() => HashCode.Combine(Rank, Suite);
    public override string ToString() => $"{fromRank[Rank]}{fromSuite[Suite]}";
  }

  enum Suite
  {
    Clubs,
    Diamonds,
    Hearts,
    Spades,
  }
}