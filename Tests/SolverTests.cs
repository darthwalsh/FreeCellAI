using System.Diagnostics;
using System.IO;
using System.Linq;
using FreeCellAI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
  [TestClass]
  public class SolverTests
  {
    [TestMethod]
    [Timeout(1000 * 3)]
    public void SimpleTest() {
      const string text = @"
QC QD QH QS KC KD KH KS
0C 0D 0H 0S JC JD JH JS
8C 8D 8H 8S 9C 9D 9H 9S
6C 6D 6H 6S 7C 7D 7H 7S
4C 4D 4H 4S 5C 5D 5H 5S
2C 2D 2H 2S 3C 3D 3H 3S
AC AD AH AS            ";
      Solve(text);
    }

    [TestMethod]
    [Timeout(1000 * 10)]
    public void SolveTest() {
      const string text = @"
0H 0D 9C 7S 8H AS 6S 3S
8D 4C 3D 2D AD AH QH 4S
9D 9S 8S 9H 8C KH JS AC
JD 7D 0C 7C KD 4H 5D QC
4D 7H KC 5H KS 2C QS 6H
2H 2S 3H QD 5S 6C 5C 3C
6D 0S JC JH            ";
      Solve(text);
    }

    static void Solve(string text) {
      var g = Importer.FromString(text);
      Assert.IsTrue(Solver.TrySolve(g, out var solution));
      for (; solution != null; solution = solution.Previous) {
        Debug.WriteLine(solution.Game);
        Debug.WriteLine("");
        Debug.WriteLine(solution.Move);
        Debug.WriteLine("");
      }
    }
  }
}
