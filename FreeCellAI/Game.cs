using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeCellAI
{
  public class Game : ICloneable, IEquatable<Game>
  {
    readonly List<ImmutableStack<Card>> tableau;
    readonly Dictionary<Suit, int> foundations;
    readonly List<Card?> freeCells;
    readonly List<Card?> orderedFreeCells;
    readonly int hashCode;

    public Game(IEnumerable<IEnumerable<Card>> tableau) : this(
      tableau.Select(ImmutableStack<Card>.New),
      Card.AllSuits.ToDictionary(s => s, s => 0),
      Enumerable.Repeat<Card?>(null, 4).ToList(),
      checkCounts: true) { }

    Game(IEnumerable<ImmutableStack<Card>> tableau, Dictionary<Suit, int> foundations, List<Card?> freeCells, bool checkCounts) {
      if (checkCounts) {
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
      }

      this.tableau = tableau.ToList();
      this.foundations = foundations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
      this.freeCells = freeCells.ToList();
      this.orderedFreeCells = freeCells.ToList();
      this.orderedFreeCells.Sort();

      var hash = new HashCode();
      foreach (var col in this.tableau) {
        foreach (var card in col) {
          hash.Add(card);
        }
      }
      foreach (var suit in Card.AllSuits) {
        hash.Add(this.foundations[suit]);
      }
      foreach (var card in orderedFreeCells) {
        hash.Add(card);
      }
      hashCode = hash.ToHashCode();
    }

    bool CanMove(Card card, Position onto) {
      switch (onto.Kind) {
        case Kind.Tableau:
          var col = tableau[onto.Index];
          if (col.IsEmpty) {
            return true;
          }
          return card.CanMoveOnto(col.Head);
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
          if (col.IsEmpty) {
            throw new InvalidOperationException("Can't move from empty column");
          }
          return col.Head;
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
          if (col.IsEmpty) {
            throw new InvalidOperationException("Can't move from empty column");
          }
          card = col.Head;
          tableau[move.From.Index] = col.Pop();
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
          tableau[move.Onto.Index] = tableau[move.Onto.Index].Push(card);
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
      var free = freeCells.SelectMany((c, i) => c.HasValue ? new[] { new Position(Kind.FreeCell, (sbyte)i) } : new Position[0]);
      var tab = tableau.SelectMany((col, i) => col.Any() ? new[] { new Position(Kind.Tableau, (sbyte)i) } : new Position[0]);
      return free.Concat(tab);
    }

    IEnumerable<Position> GetTos() {
      var free = freeCells
        .SelectMany((c, i) => c.HasValue ? new Position[0] : new[] { new Position(Kind.FreeCell, (sbyte)i) })
        .FirstOrDefault(); // Only move to the first occupied freeCell 
      var tos = Enumerable.Range(0, tableau.Count).Select(i => new Position(Kind.Tableau, (sbyte)i)).ToList();
      tos.Add(new Position(Kind.Foundation, 0));
      if (free.Kind != Kind.Uninitialized) {
        tos.Add(free);
      }
      return tos;
    }

    internal IEnumerable<Move> GetOptimizedMoves() {
      // i.e. if foundation has 3H 4D, then any black 5 or lower should immediately go to foundation
      var lowestRed = foundations.Where(kvp => Card.IsRed(kvp.Key)).Min(kvp => kvp.Value);
      var lowestBlack = foundations.Where(kvp => !Card.IsRed(kvp.Key)).Min(kvp => kvp.Value);

      var moves = GetPossibleMoves();
      var best = moves
        .Where(m => m.Onto.Kind == Kind.Foundation)
        .Where(m => {
          var card = GetCard(m.From);
          var limit = 2 + (Card.IsRed(card.Suit) ? lowestBlack : lowestRed);
          return card.Rank <= limit;
        });
      if (best.Any()) {
        return new[] { best.First() };
      }
      return moves;
    }

    internal IEnumerable<Move> GetPossibleMoves() {
      var tos = GetTos().ToList(); // cache for performance
      foreach (var from in GetFroms()) {
        foreach (var to in tos) {
          if (from.Equals(to)) {
            continue;
          }
          if (CanMove(GetCard(from), to)) {
            yield return new Move(from, to);
          }
        }
      }
    }

    public bool Solved => Card.AllSuits.All(suit => foundations[suit] == 13);

    public override string ToString() {
      var found = Card.AllSuits.Select(s =>
        foundations[s] > 0 ? new Card(foundations[s], s).ToString() : "  ");
      var free = freeCells.Select(c => c.HasValue ? c.ToString() : "  ");
      var topRow = string.Join(" ", free.Concat(found)); 
      var blank = new string(' ', tableau.Count * 3 - 1);

      // Reverse is needed because stack enumerates backwards
      var strings = tableau.Select(col => col.Reverse().Select(c => c.ToString()).ToList());
      var rows = tableau.Max(col => col.Count());
      var lines = Enumerable.Range(0, rows).Select(i => string.Join(" ", 
        strings.Select(col => i < col.Count ? col[i] : "  ")));
      return string.Join(Environment.NewLine, new[] { topRow, blank }.Concat(lines));
    }

    // If cloning is a bottleneck, consider switching to immutable stack implementation to reduce copying
    internal Game Clone() => new Game(tableau, foundations, freeCells, checkCounts: false); // Skip counts for performance
    object ICloneable.Clone() => Clone();
    public override bool Equals(object obj) => Equals(obj as Game);
    public bool Equals(Game other) => other != null && 
      tableau.Zip(other.tableau, (col, oCol) => col.SequenceEqual(oCol)).All(b => b) && 
      foundations.Keys.All(suit => foundations[suit] == other.foundations[suit]) &&
      orderedFreeCells.SequenceEqual(other.orderedFreeCells);
    public override int GetHashCode() => hashCode;
  }

  public enum Kind : sbyte
  {
    Uninitialized = 0,
    Tableau,
    Foundation,
    FreeCell,
  }

  public struct Position : IEquatable<Position>
  {
    public Position(Kind kind, sbyte index) {
      Kind = kind;
      Index = index;
    }

    public Kind Kind { get; private set; }
    public sbyte Index { get; private set; }

    public override bool Equals(object obj) => obj is Position && Equals((Position)obj);
    public bool Equals(Position other) => Kind == other.Kind && Index == other.Index;
    public override int GetHashCode() => HashCode.Combine(Kind, Index);

    public override string ToString() => Enum.GetName(typeof(Kind), Kind).Substring(0, 3) + Index;
  }

  public struct Move : IEquatable<Move>
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

    public override string ToString() => $"{From} > {Onto}";
  }
}