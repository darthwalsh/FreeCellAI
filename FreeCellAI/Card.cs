using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeCellAI
{
  public struct Card : IEquatable<Card>, IComparable<Card> {
    static readonly Dictionary<char, sbyte> ranks;
    static readonly Dictionary<sbyte, char> fromRank;
    static readonly Dictionary<char, Suit> suits;
    static readonly Dictionary<Suit, char> fromSuit;

    static Card() {
      ranks = new Dictionary<char, sbyte> {
        ['A'] = 1,
        ['0'] = 10,
        ['J'] = 11,
        ['Q'] = 12,
        ['K'] = 13,
      };
      for (var i = 2; i <= 9; ++i) {
        ranks.Add((char)('0' + i), (sbyte)i);
      }
      suits = AllSuits.ToDictionary(s => Enum.GetName(typeof(Suit), s)[0]);

      fromRank = ranks.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
      fromSuit = suits.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    public sbyte Rank { get; private set; }
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

    public Card(int rank, Suit suit) {
      Rank = (sbyte)rank;
      Suit = suit;
    }
    
    public bool CanMoveOnto(Card other) => (IsRed(Suit) != IsRed(other.Suit)) && Rank + 1 == other.Rank;

    public override bool Equals(object obj) => obj is Card && Equals((Card)obj);
    public bool Equals(Card other) => Rank == other.Rank && Suit == other.Suit;
    public override int GetHashCode() => HashCode.Combine(Rank, Suit);
    public override string ToString() => Rank == 0 ? throw new InvalidOperationException("Card is unitialized!") : 
      $"{fromRank[Rank]}{fromSuit[Suit]}";
    public int CompareTo(Card other) {
      if (Rank != other.Rank) {
        return Rank - other.Rank;
      }
      return Suit - other.Suit;
    }

    public static readonly IEnumerable<Suit> AllSuits = new[] { Suit.Clubs, Suit.Diamonds, Suit.Hearts, Suit.Spades };
    public static bool IsRed(Suit s) => s == Suit.Diamonds || s == Suit.Hearts;
  }

  public enum Suit : sbyte
  {
    Clubs,
    Diamonds,
    Hearts,
    Spades,
  }
}