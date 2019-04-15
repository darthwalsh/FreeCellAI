using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageParse;
using Parser;

namespace GUI
{
    public partial class ParserForm : Form
    {
        public ParserForm()
        {
            InitializeComponent();
        }

        async private void ParserForm_Load(object sender, EventArgs e)
        {
            const int fps = 1000 / 60;

            var image = new Bitmap(Path.Combine(Application.StartupPath, "game0.png"));

            pictureBox.Image = image;
            pictureBox.Height = Screen.PrimaryScreen.WorkingArea.Height;
            pictureBox.Width = image.Width;

            Top = 0;
            Size = pictureBox.Size;

            using (var timer = new Timer { Interval = fps })
            {
                timer.Tick += (_, __) => pictureBox.Refresh();
                timer.Start();

                var parser = new ScreenParser(new InvertingTrackingBitmap
                {
                    KeepCount = 2000,
                    IAsyncBitmap = new DelayedBitmap
                    {
                        DelayInterval = fps,
                        DelayCount = 50,
                        IAsyncBitmap = new WrappingBitmap
                        {
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
