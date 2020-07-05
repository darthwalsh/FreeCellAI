using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ImageParse;
using Parser;

namespace GUI
{
  public partial class ParserForm : Form
  {
    public ParserForm() {
      InitializeComponent();
    }

    async private void ParserForm_Load(object sender, EventArgs e) {
      const int fps = 1000 / 60;

      var image = new Bitmap(@"C:\Users\cwalsh\Downloads\Screenshot_20190527-235725.png"); //TODO

      pictureBox.Image = image;
      pictureBox.Height = Screen.PrimaryScreen.WorkingArea.Height;
      pictureBox.Width = image.Width;

      Top = 0;
      Size = pictureBox.Size;

      using (var timer = new Timer { Interval = fps }) {
        timer.Tick += (_, __) => pictureBox.Refresh();
        timer.Start();

        var parser = new ScreenParser(new InvertingTrackingBitmap {
          KeepCount = 5000,
          IAsyncBitmap = new DelayedBitmap {
            DelayInterval = fps,
            DelayCount = 200,
            IAsyncBitmap = new WrappingBitmap {
              Bitmap = image,
            },
          },
        });
        var parsed = await parser.Parse();
        Debug.WriteLine(parsed);
        pictureBox.Refresh();
      }
    }
  }
}
