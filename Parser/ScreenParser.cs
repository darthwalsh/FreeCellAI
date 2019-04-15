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
        public static Color Border = Color.FromArgb(60, 60, 60);
        public static Color BorderDim = Color.FromArgb(49, 49, 49);
        public static Color BorderShadow = Color.FromArgb(12, 41, 12);

        public static bool IsSuit(Color c)
        {
            return c.ArgbEquals(Red) || c.ArgbEquals(RedDim) || c.ArgbEquals(Black) || c.ArgbEquals(BlackDim);
        }
    }

    public class ScreenParser
    {
        IAsyncBitmap image;
        Finder finder;
        Dictionary<char, bool[,]> ranks;
        public ScreenParser(IAsyncBitmap image)
        {
            this.image = image;
            finder = new Finder(image);

            ranks = new Dictionary<char, bool[,]>();
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var name in assembly.GetManifestResourceNames())
            {
                using (var stream = assembly.GetManifestResourceStream(name))
                using (var bitmap = new Bitmap(stream))
                {
                    var pixels = new bool[bitmap.Width, bitmap.Height];
                    for (var y = 0; y < bitmap.Height; ++y)
                    {
                        for (var x = 0; x < bitmap.Width; ++x)
                        {
                            pixels[x,y] = bitmap.GetPixel(x, y).ArgbEquals(Color.White);
                        }
                    }
                    var rank = name.Replace("Parser.ranks.", "").Replace(".bmp", "").Single();
                    ranks[rank] = pixels;
                }
            }
        }

        public async Task<string> Parse()
        {
            // TODO also parse FreeCells and Foundation

            var cols = Enumerable.Range(0, 8).Select(_ => new List<string>()).ToList();
            for (var i = 0; i < 8; ++i)
            {
                var col = cols[i];
                var cardTop = await finder.FindColor(new Point((2 * i + 1) * image.Width / 16, 350), c => c.ArgbEquals(Colors.Dim) || c.ArgbEquals(Colors.Top), Dir.Down);

                while (true)
                {
                    col.Add(await ParseCard(cardTop));
                    var color = await image.GetPixel(cardTop);
                    if (color.ArgbEquals(Colors.Top))
                    {
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

        public async Task<string> ParseCard(Point cardTop)
        {
            var suitP = await finder.FindColor(cardTop, Colors.IsSuit, p => Dir.Down(Dir.Right(p)));
            var suitBorder = await finder.FindBoundary(suitP);

            var suitColor = await image.GetPixel(suitP);
            var isRed = suitColor.ArgbEquals(Colors.Red) || suitColor.ArgbEquals(Colors.RedDim);
            char suit;
            if (isRed)
            {
                var topMiddle = await finder.FindColor(suitBorder.Middle(), c => !Colors.IsSuit(c), Dir.Up);
                suit = suitBorder.Top - topMiddle.Y < -3 ? 'H' : 'D';
            }
            else
            {
                var test = await image.GetPixel(suitBorder.Top() + new Size(5, 2));
                suit = test.ArgbEquals(Colors.Black) || test.ArgbEquals(Colors.BlackDim) ? 'C' : 'S';
            }

            var rankP = suitBorder.Left();
            Rectangle rankBorder = Rectangle.Empty;

            while (rankBorder.Width < 2)
            {
                rankP = await finder.FindColor(Dir.Left(rankP), Colors.IsSuit, Dir.Left);
                rankBorder = await finder.FindBoundary(rankP);
            }
            //TODO parse rank

            return "" + suit;
        }
    }
}
