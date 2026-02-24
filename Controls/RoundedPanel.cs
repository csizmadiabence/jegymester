using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ticketmaster.Controls
{
public class RoundedPanel : Panel
{
    public int BorderRadius { get; set; } = 20;
    public int BorderSize { get; set; } = 1;
    public Color BorderColor { get; set; } = Color.Transparent;

        private GraphicsPath GetFigurePath(RectangleF rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float curveSize = radius * 2F;

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();

            return path;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. A doboz vágása marad a ClientRectangle-ön
            using (var pathRegion = GetFigurePath(this.ClientRectangle, BorderRadius))
            {
                this.Region = new Region(pathRegion);
            }

            // 2. A RAJZOLÁS ELTOLÁSA:
            // A Y koordinátát 1.0f-re állítjuk (lejjebb toljuk), 
            // és a magasságból (Height) is többet vonunk le, hogy alul se érjen ki.
            RectangleF rect = new RectangleF(1.0f, 1.0f, this.Width - 2.1f, this.Height - 2.1f);

            using (var path = GetFigurePath(rect, BorderRadius))
            {
                // Belső fehér rész
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Keret rajzolása
                if (BorderSize > 0)
                {
                    using (Pen pen = new Pen(BorderColor, BorderSize))
                    {
                        pen.Alignment = PenAlignment.Inset;
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
        }
    }
}
