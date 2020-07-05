using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ImageParse;

namespace Parser
{
  static class Colors
  {
    public static Color Top = Color.White;
    public static Color Dim = Color.FromArgb(191, 191, 191);
    public static Color Red = Color.FromArgb(191, 0, 16);
    public static Color RedDim = Color.FromArgb(255, 0, 22);
    public static Color Black = Color.FromArgb(5, 6, 6);
    public static Color BlackDim = Color.FromArgb(4, 4, 4);
    public static Color BlackDim2 = Color.FromArgb(3, 4, 4);
    public static Color Border = Color.FromArgb(60, 60, 60);
    public static Color BorderDim = Color.FromArgb(49, 49, 49);
    public static Color BorderShadow = Color.FromArgb(12, 41, 12);

    public static bool IsSuit(Color c) => c.ArgbEquals(Red) || c.ArgbEquals(RedDim) || c.ArgbEquals(Black) || c.ArgbEquals(BlackDim) || c.ArgbEquals(BlackDim2);
  }

  public class ScreenParser
  {
    readonly IAsyncBitmap image;
    readonly Finder finder;
    readonly Dictionary<char, bool[,]> ranks;
    readonly Size largestRank;
    public ScreenParser(IAsyncBitmap image) {
      this.image = image;
      finder = new Finder(image);

      ranks = new Dictionary<char, bool[,]>();
      var assembly = Assembly.GetExecutingAssembly();
      foreach (var name in assembly.GetManifestResourceNames()) {
        using (var stream = assembly.GetManifestResourceStream(name))
        using (var bitmap = new Bitmap(stream)) {
          var pixels = new bool[bitmap.Width, bitmap.Height];
          for (var y = 0; y < bitmap.Height; ++y) {
            for (var x = 0; x < bitmap.Width; ++x) {
              pixels[x, y] = bitmap.GetPixel(x, y).ArgbEquals(Color.White);
            }
          }
          var rank = name.Replace("Parser.ranks.", "").Replace(".bmp", "").Single();
          ranks[rank] = pixels;
        }
      }
      largestRank = new Size(ranks.Values.Max(p => p.GetLength(0)), ranks.Values.Max(p => p.GetLength(1)));
    }

    public async Task<string> Parse() {
      // TODO also parse FreeCells and Foundation

      var cols = Enumerable.Range(0, 8).Select(_ => new List<string>()).ToList();
      for (var i = 0; i < 8; ++i) {
        var col = cols[i];
        var cardTop = await finder.FindColor(new Point((2 * i + 1) * image.Width / 16, 350), c => c.ArgbEquals(Colors.Dim) || c.ArgbEquals(Colors.Top), Dir.Down);

        while (true) {
          col.Add(await ParseCard(cardTop));
          var color = await image.GetPixel(cardTop);
          if (color.ArgbEquals(Colors.Top)) {
            break;
          }
          cardTop = await finder.FindColor(cardTop, c => !(c.ArgbEquals(Colors.Dim) || c.ArgbEquals(Colors.Top)), Dir.Down);
          cardTop = await finder.FindColor(cardTop, c => c.ArgbEquals(Colors.Dim) || c.ArgbEquals(Colors.Top), Dir.Down);
          cardTop.Y += 2;
        }
      }

      var maxColCount = cols.Max(col => col.Count);
      var rows = Enumerable.Range(0, maxColCount)
          .Select(rowI => string.Join(" ", cols.Select(col => col.ElementAtOrDefault(rowI) ?? "  ")));
      return string.Join(Environment.NewLine, rows);
    }

    async Task<string> ParseCard(Point cardTop) {
      var suitP = await finder.FindColor(cardTop, Colors.IsSuit, p => Dir.Down(Dir.Right(p)));
      var suitBorder = await finder.FindBoundary(suitP);

      var suitColor = await image.GetPixel(suitP);
      var isRed = suitColor.ArgbEquals(Colors.Red) || suitColor.ArgbEquals(Colors.RedDim);
      char suit;
      if (isRed) {
        var topMiddle = await finder.FindColor(suitBorder.Middle(), c => !Colors.IsSuit(c), Dir.Up);
        suit = suitBorder.Top - topMiddle.Y < -3 ? 'H' : 'D';
      }
      else {
        var gapTest = await finder.FindColor(Dir.Down(suitBorder.Top()), c => !Colors.IsSuit(c), p => Dir.Right(Dir.Down(Dir.Down(p))));
        suit = gapTest.Y < (suitBorder.Top + 0.6 * suitBorder.Height) ? 'C' : 'S';
      }

      return "" + await GetRank(suitBorder) + suit;
    }

    private async Task<char> GetRank(Rectangle suitBorder) {
      var rankP = suitBorder.Left();
      var rankBorder = Rectangle.Empty;

      while (rankBorder.Width < 2) {
        rankP = await finder.FindColor(Dir.Left(rankP), Colors.IsSuit, Dir.Left);
        rankBorder = await finder.FindBoundary(rankP);
      }

      var suitColor = new bool[largestRank.Width, largestRank.Height];
      for (var y = 0; y < largestRank.Height; ++y) {
        for (var x = 0; x < largestRank.Width; ++x) {
          suitColor[x, y] = Colors.IsSuit(await image.GetPixel(rankBorder.Location + new Size(x, y)));
        }
      }

      var bestRank = '\0';
      double bestRatio = -1;
      foreach (var kvp in ranks) {
        var expected = kvp.Value;
        var found = 0;
        var width = expected.GetLength(0);
        var height = expected.GetLength(1);
        // TODO is offset needed to compensate for mis-aligned images

        for (var y = 0; y < height; ++y) {
          for (var x = 0; x < width; ++x) {
            if (suitColor[x, y] == expected[x, y]) {
              ++found;
            }
          }
        }

        var ratio = (double)found / (width * height);
        if (ratio > bestRatio) {
          bestRatio = ratio;
          bestRank = kvp.Key;
        }
      }
      return bestRank;
    }
  }
}
