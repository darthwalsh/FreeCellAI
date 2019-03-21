using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FreeCellAI
{
  static class Importer
  {
    const int colCount = 8;

    public static Game FromString(string game) => FromLines(game.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));

    public static Game FromFile(string path) => FromLines(File.ReadLines(path));

    static Game FromLines(IEnumerable<string> lines) {
      var cols = new List<List<Card>>();
      for (var i = 0; i < colCount; ++i) {
        cols.Add(new List<Card>());
      }
      foreach (var line in lines) {
        if (line.Length != 3 * colCount - 1) {
          throw new ArgumentException($"lines was {line.Length}");
        }
        for (var i = 0; i < colCount; ++i) {
          var text = line.Substring(i * 3, 2);
          if (text != "  ") {
            cols[i].Add(new Card(text));
          }
        }
      }
      return new Game(cols);
    }
  }
}
