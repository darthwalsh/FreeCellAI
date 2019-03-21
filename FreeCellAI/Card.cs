using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeCellAI
{
  struct Card : IEquatable<Card> {
    static readonly Dictionary<char, byte> ranks;
    static readonly Dictionary<byte, char> fromRank;
    static readonly Dictionary<char, Suit> suits;
    static readonly Dictionary<Suit, char> fromSuit;

    static Card() {
      ranks = new Dictionary<char, byte> {
        ['A'] = 1,
        ['0'] = 10,
        ['J'] = 11,
        ['Q'] = 12,
        ['K'] = 13,
      };
      for (var i = 2; i <= 9; ++i) {
        ranks.Add((char)('0' + i), (byte)i);
      }
      suits = Enum.GetValues(typeof(Suit)).Cast<Suit>().ToDictionary(s => Enum.GetName(typeof(Suit), s)[0]);

      fromRank = ranks.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
      fromSuit = suits.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    public byte Rank { get; private set; }
    public Suit Suit { get; private set; }
    public Card(string card) {
      if (card.Length != 2) {
        throw new ArgumentException($"{nameof(card)} should be two chars");
      }
      if (!ranks.TryGetValue(card[0], out var r)) {
        throw new ArgumentOutOfRangeException($"{card[0]} is not a rank");
      }
      Rank = r;
      if (!suits.TryGetValue(card[1], out var s)) {
        throw new ArgumentOutOfRangeException($"{card[1]} is not a suit");
      }
      Suit = s;
    }

    public bool IsRed() => Suit == Suit.Diamonds || Suit == Suit.Hearts;

    public bool CanMoveOnto(Card? other) => other == null ||
      ((IsRed() != other.Value.IsRed()) && Rank + 1 == other.Value.Rank);

    public override bool Equals(object obj) => obj is Card && Equals((Card)obj);
    public bool Equals(Card other) => Rank == other.Rank && Suit == other.Suit;
    public override int GetHashCode() => HashCode.Combine(Rank, Suit);
    public override string ToString() => $"{fromRank[Rank]}{fromSuit[Suit]}";
  }

  enum Suit : byte
  {
    Clubs,
    Diamonds,
    Hearts,
    Spades,
  }
}