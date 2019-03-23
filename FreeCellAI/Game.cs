using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeCellAI
{
  class Game : ICloneable {
    readonly List<List<Card>> tableau;
    readonly Dictionary<Suit, int> foundations;
    readonly List<Card?> freeCells;

    public Game(IEnumerable<IEnumerable<Card>> tableau) : this(
      tableau,
      Enum.GetValues(typeof(Suit)).Cast<Suit>().ToDictionary(s => s, s => 0),
      Enumerable.Repeat<Card?>(null, 4).ToList()) { }

    Game(IEnumerable<IEnumerable<Card>> tableau, Dictionary<Suit, int> foundations, List<Card?> freeCells) {
      var allCards = tableau.SelectMany(col => col)
        .Concat(freeCells.SelectMany(c => c.HasValue ? new[] { c.Value } : new Card[0]))
        .Concat(foundations.SelectMany(kvp => Enumerable.Range(1, kvp.Value).Select(r => new Card(r, kvp.Key))));
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

    bool CanMove(Card card, Position onto) {
      switch (onto.Kind) {
        case Kind.Tableau:
          var col = tableau[onto.Index];
          if (col.Count == 0) {
            return true;
          }
          return card.CanMoveOnto(col.Last());
        case Kind.Foundation:
          return foundations[card.Suit] + 1 == card.Rank;
        case Kind.FreeCell:
          return freeCells[onto.Index] == null;
      }
      throw new InvalidOperationException("invalid Kind");
    }

    Card GetCard(Position pos) {
      switch (pos.Kind) {
        case Kind.Tableau:
          var col = tableau[pos.Index];
          if (col.Count == 0) {
            throw new InvalidOperationException("Can't move from empty column");
          }
          return col.Last();
        case Kind.Foundation:
          throw new InvalidOperationException("Can't move from foundation");
        case Kind.FreeCell:
          return freeCells[pos.Index].Value;
      }
      throw new InvalidOperationException("invalid Kind");
    }

    internal void MoveCard(Move move) {
      Card card;
      switch (move.From.Kind) {
        case Kind.Tableau:
          var col = tableau[move.From.Index];
          if (col.Count == 0) {
            throw new InvalidOperationException("Can't move from empty column");
          }
          card = col.Last();
          col.RemoveAt(col.Count - 1);
          break;
        case Kind.Foundation:
          throw new InvalidOperationException("Can't move from foundation");
        case Kind.FreeCell:
          card = freeCells[move.From.Index].Value;
          freeCells[move.From.Index] = null;
          break;
        default:
          throw new InvalidOperationException("invalid Kind");
      }

      switch (move.Onto.Kind) {
        case Kind.Tableau:
          var col = tableau[move.Onto.Index];
          col.Add(card);
          break;
        case Kind.Foundation:
          foundations[card.Suit]++;
          break;
        case Kind.FreeCell:
          freeCells[move.Onto.Index] = card;
          break;
      }
    }

    public bool TryMove(Move move, out Game result) {
      var card = GetCard(move.From);
      if (!CanMove(card, move.Onto)) {
        result = null;
        return false;
      }

      result = Clone();
      result.MoveCard(move);
      return true;
    }

    IEnumerable<Position> GetFroms() {
      var free = freeCells.SelectMany((c, i) => c.HasValue ? new[] { new Position(Kind.FreeCell, (byte)i) } : new Position[0]);
      var tab = tableau.SelectMany((col, i) => col.Any() ? new[] { new Position(Kind.Tableau, (byte)i) } : new Position[0]);
      return free.Concat(tab);
    }

    IEnumerable<Position> GetTos() {
      var free = freeCells
        .SelectMany((c, i) => c.HasValue ? new Position[0] : new[] { new Position(Kind.FreeCell, (byte)i) })
        .FirstOrDefault(); // Only move to the first occupied freeCell 
      var tos = Enumerable.Range(0, tableau.Count).Select(i => new Position(Kind.Tableau, (byte)i)).ToList();
      tos.Add(new Position(Kind.Foundation, 0));
      if (free.Kind != Kind.Uninitialized) {
        tos.Add(free);
      }
      return tos;
    }

    internal IEnumerable<Move> GetPossibleMoves() {
      foreach (var from in GetFroms()) {
        foreach (var to in GetTos()) {
          var move = new Move(from, to);
          if (TryMove(move, out var result)) {
            yield return move;
          }
        }
      }
    }

    public override string ToString() {
      var found = Enum.GetValues(typeof(Suit)).Cast<Suit>().Select(s =>
        foundations[s] > 0 ? new Card(foundations[s], s).ToString() : "  ");
      var free = freeCells.Select(c => c.HasValue ? c.ToString() : "  ");
      var topRow = string.Join(" ", free.Concat(found)); 
      var blank = new string(' ', tableau.Count * 3 - 1);

      var strings = tableau.Select(col => col.Select(c => c.ToString()).ToList());
      var rows = tableau.Max(col => col.Count);
      var lines = Enumerable.Range(0, rows).Select(i => string.Join(" ", 
        strings.Select(col => i < col.Count ? col[i] : "  ")));
      return string.Join(Environment.NewLine, new[] { topRow, blank }.Concat(lines));
    }

    // If cloning is a bottleneck, consider switching to immutable stack implementation to reduce copying
    internal Game Clone() => new Game(tableau, foundations, freeCells);
    object ICloneable.Clone() => Clone();

    internal enum Kind : byte
    {
      Uninitialized = 0,
      Tableau,
      Foundation,
      FreeCell,
    }

    internal struct Position : IEquatable<Position>
    {
      public Position(Kind kind, byte index) {
        Kind = kind;
        Index = index;
      }

      public Kind Kind { get; private set; }
      public byte Index { get; private set; }

      public override bool Equals(object obj) => obj is Position && Equals((Position)obj);
      public bool Equals(Position other) => Kind == other.Kind && Index == other.Index;
      public override int GetHashCode() => HashCode.Combine(Kind, Index);

      public override string ToString() => Enum.GetName(typeof(Kind), Kind).Substring(0, 3) + Index;
    }

    internal struct Move : IEquatable<Move>
    {
      public Move(Position from, Position onto) {
        From = from;
        Onto = onto;
      }

      public Position From { get; private set; }
      public Position Onto { get; private set; }

      public override bool Equals(object obj) => obj is Move && Equals((Move)obj);
      public bool Equals(Move other) => From.Equals(other.From) && Onto.Equals(other.Onto);
      public override int GetHashCode() => HashCode.Combine(From, Onto);

      public override string ToString() => $"{From} \u2192 {Onto}";
    }
  }
}