using FreeCellAI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
  [TestClass]
  public class CardTests
  {
    [TestMethod]
    public void ImportTest() {
      var g = Importer.FromFile("game0.txt");
    }
  }
}
