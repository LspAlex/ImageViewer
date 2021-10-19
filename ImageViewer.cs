using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Image.Tools
{
    public partial class DockPropertyImageViewer : Form
    {
        int size = Screen.PrimaryScreen.WorkingArea.Width / 32;
        int leftX = -1;
        int rightX = -1;
        int Y = -1;
        List<Image> imgList = new List<Image>();
        List<Image> drawList = new List<Image>();
        int index = 0;

        public DockPropertyImageViewer()
        {
            InitializeComponent();
        }

        public DockPropertyImageViewer(List<string> imgDirList)
        {
            InitializeComponent();
            foreach (string dir in imgDirList)
            {
                imgList.Add(Image.FromFile(dir));
            }
            SetImage();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            DrawArrow(e, drawList[index]);            
        }

        private void DrawArrow(PaintEventArgs e, Image image)
        {
            BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;
            BufferedGraphics myBuffer = currentContext.Allocate(e.Graphics, e.ClipRectangle);
            Graphics g = myBuffer.Graphics;
            g.Clear(this.BackColor);

            this.ClientSize = new Size(image.Width, image.Height);
            g.DrawImage(image, 0, 0, ClientSize.Width, ClientSize.Height);

            leftX = size / 2;
            rightX = this.ClientSize.Width - size * 3 / 2;
            Y = (this.ClientSize.Height - size) / 2;

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormM));
            Image imgLeft = (Image)(resources.GetObject("customImgViewLeft"));
            Image imgRight = (Image)(resources.GetObject("customImgViewRight"));
            g.DrawImage(imgLeft, leftX, Y, size, size);
            g.DrawImage(imgRight, rightX, Y, size, size);

            myBuffer.Render(e.Graphics);
            myBuffer.Dispose();
            g.Dispose();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (CheckPosition(e.Location, new Point(leftX, Y), size))
            {
                // Left Click
                index -= 1;
                if (index < 0)
                {
                    index = drawList.Count - 1;
                }
            }
            else if (CheckPosition(e.Location, new Point(rightX, Y), size))
            {
                // Right Click
                index += 1;
                if (index > drawList.Count - 1)
                {
                    index = index - drawList.Count;
                }
            }
            CheckImageEdge();
            this.Refresh();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta < 0)
            {
                drawList[index] = resizeImage(imgList[index], new Size((int)(drawList[index].Width * 0.9), (int)(drawList[index].Height * 0.9)));
                if (Math.Min(drawList[index].Width, drawList[index].Height) < Math.Max(this.MinimumSize.Width, this.MinimumSize.Height))
                {
                    drawList[index] = resizeImage(imgList[index], new Size((int)(drawList[index].Width * 10 / 9d), (int)(drawList[index].Height * 10 / 9d)));
                    return;
                }
            }
            else if (e.Delta > 0)
            {
                drawList[index] = resizeImage(imgList[index], new Size((int)(drawList[index].Width * 1.1), (int)(drawList[index].Height * 1.1)));
            }
            this.Refresh();
        }

        private bool CheckPosition(Point p, Point sta, int edgeLen)
        {
            if (p.X >= sta.X && p.X <= sta.X + edgeLen && p.Y >= sta.Y && p.Y <= sta.Y + edgeLen)
            {
                return true;
            }
            return false;
        }

        private void SetImage()
        {
            for (int i = 0; i < imgList.Count; i++)
            {
                drawList.Add(resizeImage(imgList[i], new Size((int)(imgList[i].Width), (int)(imgList[i].Height))));
            }
            RefreshImage();
        }

        private void RefreshImage()
        {
            drawList[index] = resizeImage(imgList[index], new Size((int)(imgList[index].Width), (int)(imgList[index].Height)));            
        }

        private void CheckImageEdge()
        {
            int maxImgLen = Math.Max(drawList[index].Width, drawList[index].Height);
            int minImgLen = Math.Min(drawList[index].Width, drawList[index].Height);
            int maxWindowLen = Math.Max(this.MinimumSize.Width, this.MinimumSize.Height);
            int minScreenLen = Math.Min(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
            double radio = ((double)Math.Max(drawList[index].Width, drawList[index].Height) / (double)Math.Min(drawList[index].Width, drawList[index].Height));
            if (maxImgLen > minScreenLen)
            {
                if (imgList[index].Width > imgList[index].Height)
                {
                    drawList[index] = resizeImage(imgList[index], new Size(minScreenLen, (int)(minScreenLen / radio)));
                }
                else
                {
                    drawList[index] = resizeImage(imgList[index], new Size((int)(minScreenLen / radio), minScreenLen));
                }
            }
            if (minImgLen < maxWindowLen)
            {
                if (imgList[index].Width > imgList[index].Height)
                {
                    drawList[index] = resizeImage(imgList[index], new Size((int)(maxWindowLen * radio), maxWindowLen));
                }
                else
                {
                    drawList[index] = resizeImage(imgList[index], new Size(maxWindowLen, (int)(maxWindowLen * radio)));
                }
            }
        }

        private static System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            //Get Image Wid and hei
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            //Calculate radio of image
            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            //Expect wid and hei
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return (System.Drawing.Image)b;
        }
    }
}
