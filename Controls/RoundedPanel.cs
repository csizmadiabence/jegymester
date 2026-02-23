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

        public RoundedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.Parent != null)
            {
                e.Graphics.Clear(this.Parent.BackColor);
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            GraphicsPath path = new GraphicsPath();
            int d = BorderRadius;
            path.AddArc(0, 0, d, d, 180, 90);
            path.AddArc(Width - d - 1, 0, d, d, 270, 90);
            path.AddArc(Width - d - 1, Height - d - 1, d, d, 0, 90);
            path.AddArc(0, Height - d - 1, d, d, 90, 90);
            path.CloseAllFigures();

            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            base.OnPaint(e);
        }
    }
}
