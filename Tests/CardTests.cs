using System.IO;
using System.Linq;
using FreeCellAI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
  [TestClass]
  public class CardTests
  {
    [TestMethod]
    public void ImportTest() {
      const string path = "game0.txt";
      var g = Importer.FromFile(path);
      Assert.IsTrue(g.ToString().EndsWith(File.ReadAllText(path)));
    }
    [TestMethod]
    public void TrickyImportTest() {
      const string text = @"
0H 0D 9C 7S 8H AS 6S 3S
8D 4C 3D 2D AD AH QH 4S
9D 9S 8S 9H 8C KH JS AC
JD 7D 0C 7C KD 4H 5D QC
4D 7H KC 5H KS 2C QS 6H
2H 2S 3H QD 5S 6C    3C
6D 0S JC JH            
   5C                  ";
      var g = Importer.FromString(text);
      Assert.IsTrue(g.ToString().EndsWith(text));
    }

    [TestMethod]
    public void CloneTest() {
      const string path = "game0.txt";
      var g = Importer.FromFile(path);
      Assert.AreEqual(g.Clone().ToString(), g.ToString());
    }

    [TestMethod]
    public void CanMoveTests() {
      Assert.IsTrue(new Card("AH").CanMoveOnto(new Card("2C")));
      Assert.IsFalse(new Card("AH").CanMoveOnto(new Card("2D")));
      Assert.IsFalse(new Card("3H").CanMoveOnto(new Card("2C")));
    }

    [TestMethod]
    public void MoveTests() {
      const string text = @"
0H 0D 9C 7S 8H AS 6S 3S
8D 4C 3D 2D AD AH QH 4S
9D 9S 8S 9H 8C KH JS 5C
JD 7D 0C 7C KD 4H 5D QC
4D 7H KC 5H KS 2C QS 6H
2H 2S 3H QD 5S 6C AC 3C
6D 0S JC JH            ";
      var g = Importer.FromString(text);
      Move(ref g, new Move(
        new Position(Kind.Tableau, 0),
        new Position(Kind.FreeCell, 0)
      ));
      Assert.AreEqual(@"6D                     
                       
0H 0D 9C 7S 8H AS 6S 3S
8D 4C 3D 2D AD AH QH 4S
9D 9S 8S 9H 8C KH JS 5C
JD 7D 0C 7C KD 4H 5D QC
4D 7H KC 5H KS 2C QS 6H
2H 2S 3H QD 5S 6C AC 3C
   0S JC JH            ", g.ToString());

      Assert.IsFalse(g.TryMove(new Move(
        new Position(Kind.Tableau, 0),
        new Position(Kind.FreeCell, 0)
      ), out _));

      Move(ref g, new Move(
        new Position(Kind.Tableau, 0),
        new Position(Kind.FreeCell, 1)
      ));

      Move(ref g, new Move(
        new Position(Kind.Tableau, 0),
        new Position(Kind.FreeCell, 2)
      ));

      Move(ref g, new Move(
        new Position(Kind.Tableau, 0),
        new Position(Kind.FreeCell, 3)
      ));

      Assert.AreEqual(@"6D 2H 4D JD            
                       
0H 0D 9C 7S 8H AS 6S 3S
8D 4C 3D 2D AD AH QH 4S
9D 9S 8S 9H 8C KH JS 5C
   7D 0C 7C KD 4H 5D QC
   7H KC 5H KS 2C QS 6H
   2S 3H QD 5S 6C AC 3C
   0S JC JH            ", g.ToString());

      Move(ref g, new Move(
        new Position(Kind.Tableau, 6),
        new Position(Kind.Foundation, 0)
      ));

      Assert.AreEqual(@"6D 2H 4D JD AC         
                       
0H 0D 9C 7S 8H AS 6S 3S
8D 4C 3D 2D AD AH QH 4S
9D 9S 8S 9H 8C KH JS 5C
   7D 0C 7C KD 4H 5D QC
   7H KC 5H KS 2C QS 6H
   2S 3H QD 5S 6C    3C
   0S JC JH            ", g.ToString());

      Move(ref g, new Move(
        new Position(Kind.FreeCell, 2),
        new Position(Kind.Tableau, 4)
      ));

      Assert.AreEqual(@"6D 2H    JD AC         
                       
0H 0D 9C 7S 8H AS 6S 3S
8D 4C 3D 2D AD AH QH 4S
9D 9S 8S 9H 8C KH JS 5C
   7D 0C 7C KD 4H 5D QC
   7H KC 5H KS 2C QS 6H
   2S 3H QD 5S 6C    3C
   0S JC JH 4D         ", g.ToString());
    }



    [TestMethod]
    public void PossibleMovesTests() {
      const string text = @"
0H 0D 9C 7S 8H AS 6S 3S
8D 4C 3D 2D AD AH QH 4S
9D 9S 8S 9H 8C KH JS 5C
JD 7D 0C 7C KD 4H 5D QC
4D 7H KC 5H KS 2C QS 6H
2H 2S 3H QD 5S 6C AC 3C
6D 0S JC JH            ";
      var g = Importer.FromString(text);
      var moves = g.GetPossibleMoves().ToList();
      CollectionAssert.AreEquivalent(Enumerable.Range(0, 8).Select(c => new Move(
        new Position(Kind.Tableau, (sbyte)c),
        new Position(Kind.FreeCell, 0)))
        .Concat(new[] {
          // AC
          new Move(
            new Position(Kind.Tableau, 6),
            new Position(Kind.Foundation, 0)),
          // 0S
          new Move(
            new Position(Kind.Tableau, 1),
            new Position(Kind.Tableau, 3)),
          // 5S
          new Move(
            new Position(Kind.Tableau, 4),
            new Position(Kind.Tableau, 0)),
        }).ToList(), moves);
    }

    [TestMethod]
    public void OptimizedMoveTests() {
      const string text = @"
0H 0D 9C 7S 8H AS 6S 3S
8D 4C 3D 2D 3H KC QH 4S
9D 9S 8S 9H 8C KH JS 5C
JD 7D 0C 7C KD 4H 5D QC
4D 7H AH 5H KS QS 2C 6H
2H 2S AD QD 5S 6C AC 3C
6D 0S JC JH            ";
      var g = Importer.FromString(text);
      var moves = g.GetOptimizedMoves().ToList();
      CollectionAssert.AreEquivalent(new[] {
          // AC
          new Move(
            new Position(Kind.Tableau, 6),
            new Position(Kind.Foundation, 0)),
      }, moves);
      Move(ref g, moves.Single());

      moves = g.GetOptimizedMoves().ToList();
      CollectionAssert.AreEquivalent(new[] {
          // 2C
          new Move(
            new Position(Kind.Tableau, 6),
            new Position(Kind.Foundation, 0)),
      }, moves);
      Move(ref g, moves.Single());

      Assert.IsTrue(g.GetOptimizedMoves().Count() > 1, "Shouldn't move up 3C");

      // JC
      Move(ref g, new Move(
        new Position(Kind.Tableau, 2),
        new Position(Kind.FreeCell, 0)));

      // AD
      Move(ref g, g.GetOptimizedMoves().Single());
      // AH
      Move(ref g, g.GetOptimizedMoves().Single());

      moves = g.GetOptimizedMoves().ToList();
      CollectionAssert.AreEquivalent(new[] {
          // 3C
          new Move(
            new Position(Kind.Tableau, 7),
            new Position(Kind.Foundation, 0)),
      }, moves);
    }

    [TestMethod]
    public void SortTest() {
      var cards = new[] { new Card("5C"), new Card("5H"), new Card("6C") };
      CollectionAssert.AreEqual(cards, cards.OrderBy(c => c).ToList());
    }

    static void Move(ref Game game, Move move) => Assert.IsTrue(game.TryMove(move, out game));
  }
}
