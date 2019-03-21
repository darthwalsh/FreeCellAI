using System.IO;
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
      Assert.AreEqual(File.ReadAllText(path), g.ToString());
    }

    [TestMethod]
    public void CloneTest() {
      const string path = "game0.txt";
      var g = Importer.FromFile(path);
      Assert.AreEqual(g.Clone().ToString(), g.ToString());
    }
  }
}
