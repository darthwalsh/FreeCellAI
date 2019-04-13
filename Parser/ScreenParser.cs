using System;
using System.Drawing;
using System.Threading.Tasks;
using ImageParse;

namespace Parser
{
    public class ScreenParser
    {
        IAsyncBitmap image;
        Finder finder;
        public ScreenParser(IAsyncBitmap image)
        {
            this.image = image;
            finder = new Finder(image);
        }

        public async Task<string> Parse()
        {
            for (var p = new Point(0, 0); p.X < image.Width; p = Dir.Down(Dir.Right(p)))
            {
                await image.GetPixel(p);
            }

            return "";
        }
    }
}
