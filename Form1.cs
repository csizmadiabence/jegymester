using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ticketmaster
{
    public partial class TicketMaster : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
            (
                int nLeftRect,
                int nTopRect,
                int nRightRect,
                int nBottomRect,
                int nWidthEllipse,
                int nHeightEllipse
            );

        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        //SENDMESSAGE-hez
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        // A gombokhoz kellenek:
        bool isHovering = false;
        bool isMinHovering = false;

        //smoothcriminal
        int passwordFinalY = 330;
        int panelFinalY = 365;
        int animSpeed = 2;
        bool isExpanded = false;

        //betűtípus
        private Font pillFont = new Font("Inter Medium", 7);

        private Pen headerLinePen = new Pen(Color.FromArgb(230, 230, 230), 1);
        public TicketMaster()
        {
            InitializeComponent();

            this.DoubleBuffered = true;

            //Program neve:
            this.Text = "Ticket Master";

            //emailbox
            SendMessage(EmailBox.Handle, 0x1501, 0, "email@domain.com");
            SendMessage(txtPassword.Handle, 0x1501, 0, "Enter your password");

            this.FormBorderStyle = FormBorderStyle.None;
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 30, 30));

            typeof(Panel).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
             null, pnlBottom, new object[] { true });

            //nemtoltibevalamiert
            picExit.Image = Properties.Resources.exiticon;
            picExit.BackColor = Color.Transparent;

            //eztse
            picMin.Image = Properties.Resources.minimizeicon;
            picMin.BackColor = Color.Transparent;

            //termscumo
            richTextBox1.Text = "By clicking continue, you agree to our Terms of Service and Privacy Policy";

            int start1 = richTextBox1.Text.IndexOf("Terms of Service");
            richTextBox1.Select(start1, "Terms of Service".Length);
            richTextBox1.SelectionColor = Color.Black;

            int start2 = richTextBox1.Text.IndexOf("Privacy Policy");
            richTextBox1.Select(start2, "Privacy Policy".Length);
            richTextBox1.SelectionColor = Color.Black;

            richTextBox1.SelectAll();
            richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
            richTextBox1.Select(0, 0);
            richTextBox1.ReadOnly = true;
            richTextBox1.Cursor = Cursors.Default;
            richTextBox1.Enter += (s, e) => { this.ActiveControl = null; };
            richTextBox1.SelectionChanged += (s, e) => { richTextBox1.DeselectAll(); };

            //proba$$
            MoviesTable.Rows.Add("Crime 101", "Thriller", "140 mins", "HU, EN");
            MoviesTable.Rows.Add("Beléd estem", "Comedy | Romance", "93 mins", "HU");
            MoviesTable.Rows.Add("Scream 7", "Horror", "115 mins", "HU, EN");

            ScreeningsTable.Rows.Add("Crime 101", "Veszprém - Balaton", "2026.02.24 | 12:00");
            ScreeningsTable.Rows.Add("Beléd estem", "Székesfehérvár - Alba", "2026.02.24 | 16:00");
            ScreeningsTable.Rows.Add("Scream 7", "Budapest - Arena", "2026.02.24 | 19:00");

            if (Environment.OSVersion.Version.Build >= 22000)
            {
                int[] margins = { 2 };
                DwmSetWindowAttribute(this.Handle, 33, margins, 4);
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x84)
            {
                m.Result = (IntPtr)(0x2);
            }
        }

        //Movies táblázat.
        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                e.PaintBackground(e.CellBounds, true);
                e.PaintContent(e.CellBounds);

                e.Graphics.DrawLine(headerLinePen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                e.Handled = true;
                return;
            }

            // 2. A RUBLIKÁK (Kapszulák) rajzolása
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.Value != null)
            {
                if (e.ColumnIndex == 1 || e.ColumnIndex == 2)
                {
                    e.PaintBackground(e.CellBounds, true);
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    string text = e.Value.ToString();

                    using (Font pillFont = new Font("Inter Medium", 7))
                    {
                        SizeF textSize = e.Graphics.MeasureString(text, pillFont);

                        int paddingX = 10;
                        int paddingY = 4;

                        int pillWidth = (int)textSize.Width + (paddingX * 1) + 1;
                        int pillHeight = (int)textSize.Height + (paddingY * 1);

                        int x = e.CellBounds.Left + 10;
                        int y = e.CellBounds.Top + (e.CellBounds.Height - pillHeight) / 2;

                        RectangleF pillRect = new RectangleF(x, y, pillWidth, pillHeight);
                        float radius = 5f;

                        using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                        {
                            path.AddArc(pillRect.X, pillRect.Y, radius * 2, radius * 2, 180, 90);
                            path.AddArc(pillRect.Right - radius * 2, pillRect.Y, radius * 2, radius * 2, 270, 90);
                            path.AddArc(pillRect.Right - radius * 2, pillRect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                            path.AddArc(pillRect.X, pillRect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                            path.CloseFigure();

                            using (SolidBrush brush = new SolidBrush(Color.White))
                            using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                            {
                                e.Graphics.FillPath(brush, path);
                                e.Graphics.DrawPath(pen, path);
                            }
                        }

                        using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(60, 60, 60)))
                        {
                            RectangleF textRect = new RectangleF(pillRect.X - 5, pillRect.Y, pillRect.Width + 10, pillRect.Height);

                            using (StringFormat sf = new StringFormat())
                            {
                                sf.Alignment = StringAlignment.Center;
                                sf.LineAlignment = StringAlignment.Center;

                                sf.Trimming = StringTrimming.None;
                                sf.FormatFlags = StringFormatFlags.NoWrap;

                                e.Graphics.DrawString(text, pillFont, textBrush, textRect, sf);
                            }
                        }
                    }

                    e.Handled = true;
                }
            }
        }

        // Alkalmazás bezárás gomb:
        private void picExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void picExit_MouseEnter(object sender, EventArgs e)
        {
            isHovering = true;
            picExit.Invalidate();
        }
        private void picExit_MouseLeave(object sender, EventArgs e)
        {
            isHovering = false;
            picExit.Invalidate();
        }

        private void picExit_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            if (isHovering)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, picExit.Width - 1, picExit.Height - 1);
                }
            }

            if (picExit.Image != null)
            {
                int targetSize = 15;

                int x = (picExit.Width - targetSize) / 2;
                int y = (picExit.Height - targetSize) / 2;

                e.Graphics.DrawImage(picExit.Image, x, y, targetSize, targetSize);
            }
        }

        // Alkalmazás tálcára letétele gomb:
        private void picMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void picMin_MouseEnter(object sender, EventArgs e)
        {
            isMinHovering = true;
            picMin.Invalidate();
        }

        private void picMin_MouseLeave(object sender, EventArgs e)
        {
            isMinHovering = false;
            picMin.Invalidate();
        }

        private void picMin_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (isMinHovering)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, picMin.Width - 1, picMin.Height - 1);
                }
            }

            if (picMin.Image != null)
            {
                int targetSize = 15;
                int x = (picMin.Width - targetSize) / 2;
                int y = (picMin.Height - targetSize) / 2;
                e.Graphics.DrawImage(picMin.Image, x, y, targetSize, targetSize);
            }
        }

        //Screenings táblázat:
        private void ScreeningsTable_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                e.PaintBackground(e.CellBounds, true);
                e.PaintContent(e.CellBounds);

                e.Graphics.DrawLine(headerLinePen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                e.Handled = true;
                return;
            }

            // 2. A RUBLIKÁK (Kapszulák) rajzolása
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.Value != null)
            {
                if (e.ColumnIndex == 1 || e.ColumnIndex == 2)
                {
                    e.PaintBackground(e.CellBounds, true);
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    string text = e.Value.ToString();

                    using (Font pillFont = new Font("Inter Medium", 7))
                    {
                        SizeF textSize = e.Graphics.MeasureString(text, pillFont);

                        int paddingX = 10;
                        int paddingY = 4;

                        int pillWidth = (int)textSize.Width + (paddingX * 1) + 1;
                        int pillHeight = (int)textSize.Height + (paddingY * 1);

                        int x = e.CellBounds.Left + 10;
                        int y = e.CellBounds.Top + (e.CellBounds.Height - pillHeight) / 2;

                        RectangleF pillRect = new RectangleF(x, y, pillWidth, pillHeight);
                        float radius = 5f;

                        using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                        {
                            path.AddArc(pillRect.X, pillRect.Y, radius * 2, radius * 2, 180, 90);
                            path.AddArc(pillRect.Right - radius * 2, pillRect.Y, radius * 2, radius * 2, 270, 90);
                            path.AddArc(pillRect.Right - radius * 2, pillRect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                            path.AddArc(pillRect.X, pillRect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                            path.CloseFigure();

                            using (SolidBrush brush = new SolidBrush(Color.White))
                            using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                            {
                                e.Graphics.FillPath(brush, path);
                                e.Graphics.DrawPath(pen, path);
                            }
                        }

                        using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(60, 60, 60)))
                        {
                            RectangleF textRect = new RectangleF(pillRect.X - 5, pillRect.Y, pillRect.Width + 10, pillRect.Height);

                            using (StringFormat sf = new StringFormat())
                            {
                                sf.Alignment = StringAlignment.Center;
                                sf.LineAlignment = StringAlignment.Center;

                                sf.Trimming = StringTrimming.None;
                                sf.FormatFlags = StringFormatFlags.NoWrap;

                                e.Graphics.DrawString(text, pillFont, textBrush, textRect, sf);
                            }
                        }
                    }

                    e.Handled = true;
                }
            }
        }

        //Animacio:
        private void tmrAnimate_Tick(object sender, EventArgs e)
        {
            if (isExpanded)
            {
                bool moved = false;

                if (txtPassword.Top < passwordFinalY)
                {
                    txtPassword.Top += animSpeed;
                    moved = true;
                }

                if (pnlBottom.Top < panelFinalY)
                {
                    pnlBottom.Top += animSpeed;
                    moved = true;
                }

                if (!moved) tmrAnimate.Stop();
            }
            else
            {
                bool moved = false;
                int originalY = EmailBox.Top;
                int panelOriginalY = EmailBox.Bottom + 10;

                if (txtPassword.Top > originalY)
                {
                    txtPassword.Top -= animSpeed;
                    moved = true;
                }

                if (pnlBottom.Top > panelOriginalY)
                {
                    pnlBottom.Top -= animSpeed;
                    moved = true;
                }

                if (!moved)
                {
                    tmrAnimate.Stop();
                    txtPassword.Visible = false;
                }
            }
        }

        //Emailbox valtozasa:
        private void EmailBox_TextChanged(object sender, EventArgs e)
        {
            if (EmailBox.Text.Length > 0 && !isExpanded)
            {
                txtPassword.Visible = true;

                EmailBox.BringToFront();
                txtPassword.SendToBack();

                txtPassword.Top = EmailBox.Top + 2;

                tmrAnimate.Start();
                isExpanded = true;
            }
            else if (EmailBox.Text.Length == 0 && isExpanded)
            {
                isExpanded = false;
                tmrAnimate.Start();
            }
        }
    }
}
