using ImageParse;
using System;
using System.Threading.Tasks;

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
            

            return "";
        }
    }
}
