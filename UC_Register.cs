using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace ticketmaster
{
    public partial class UC_Register : UserControl
    {
        //SENDMESSAGE DLL!!!
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        //ALKALMAZÁS MIN GOMBHOZ KELL:
        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        //Gombokhoz kellenek:
        bool isHovering = false;
        bool isMinHovering = false;

        public UC_Register()
        {
            InitializeComponent();

            this.Load += UC_Register_Load;
            LoadButtonImages();

            SetupTermsCumo();
        }

        //Termscumo kiírása helyesen:
        private void SetupTermsCumo()
        {
            termscumo.Text = "By clicking continue, you agree to our Terms of Service and Privacy Policy";

            int start1 = termscumo.Text.IndexOf("Terms of Service");
            termscumo.Select(start1, "Terms of Service".Length);
            termscumo.SelectionColor = Color.Black;

            int start2 = termscumo.Text.IndexOf("Privacy Policy");
            termscumo.Select(start2, "Privacy Policy".Length);
            termscumo.SelectionColor = Color.Black;

            termscumo.SelectAll();
            termscumo.SelectionAlignment = HorizontalAlignment.Center;
            termscumo.Select(0, 0);
            termscumo.ReadOnly = true;
            termscumo.Cursor = Cursors.Default;
            termscumo.Enter += (s, e) => { this.ActiveControl = null; };
            termscumo.SelectionChanged += (s, e) => { termscumo.DeselectAll(); };
        }

        //Képek betöltése a gombokhoz:
        private void LoadButtonImages()
        {
            picExit.Image = Properties.Resources.exiticon;
            picExit.BackColor = Color.Transparent;

            picMin.Image = Properties.Resources.minimizeicon;
            picMin.BackColor = Color.Transparent;
        }

        private void UC_Register_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;

            this.SuspendLayout();
            RegisterPanel.SuspendLayout();

            TextBox[] allInputs = new TextBox[] {
            Username,
            EmailBoxReg,
            PhoneNumber,
            PasswordReg,
            ConfirmPasswordReg
            };

            foreach (TextBox tb in allInputs)
            {
                Point loc = tb.Location;

                ticketmaster.Controls.RoundedPanel wrapper = new ticketmaster.Controls.RoundedPanel();

                wrapper.Size = new Size(275 + 10, 32);

                wrapper.Location = new Point(loc.X - 4, loc.Y - 2);

                wrapper.BackColor = RegisterPanel.BackColor;
                wrapper.BorderRadius = 3;
                wrapper.BorderSize = 1;
                wrapper.BorderColor = Color.FromArgb(200, 200, 200);

                wrapper.Padding = new Padding(5, 4, 4, 4);

                this.Controls.Remove(tb);

                tb.Size = new Size(275, 27);
                tb.Dock = DockStyle.Top;
                tb.BorderStyle = BorderStyle.None;
                tb.BackColor = Color.White;

                wrapper.Controls.Add(tb);

                tb.Enter += Common_Enter;
                tb.Leave += Common_Leave;

                RegisterPanel.Controls.Add(wrapper);
                wrapper.BringToFront();
            }

            RegisterPanel.ResumeLayout(true);
            this.ResumeLayout(true);

            SetupPlaceholders();
        }

        //TextBox-okban a szövegek:
        private void SetupPlaceholders()
        {
            SendMessage(this.Username.Handle, 0x1501, 0, "Username");
            SendMessage(this.EmailBoxReg.Handle, 0x1501, 0, "email@domain.com");
            SendMessage(this.PhoneNumber.Handle, 0x1501, 0, "* Phone Number");
            SendMessage(this.PasswordReg.Handle, 0x1501, 0, "Password");
            SendMessage(this.ConfirmPasswordReg.Handle, 0x1501, 0, "Confirm Password");
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int radius = 30;
            RectangleF rect = new RectangleF(0.5f, 0.5f, this.Width - 1, this.Height - 1);

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();

                this.Region = new Region(path);

                using (PathGradientBrush pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = Color.Transparent;
                    pgb.SurroundColors = new Color[] { Color.FromArgb(6, 0, 0, 0) };
                    pgb.FocusScales = new PointF(0.99f, 0.99f);
                    e.Graphics.FillPath(pgb, path);
                }

                using (Pen borderPen = new Pen(Color.FromArgb(40, 0, 0, 0), 1))
                {
                    e.Graphics.DrawPath(borderPen, path);
                }
            }
        }

        // Színezése a keretnek állapot szerint:
        private void Common_Enter(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null && tb.Parent is ticketmaster.Controls.RoundedPanel rp)
            {
                rp.BorderColor = Color.DodgerBlue;
                rp.BorderSize = 1;
                rp.Invalidate();
            }
        }

        private void Common_Leave(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null && tb.Parent is ticketmaster.Controls.RoundedPanel rp)
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    // Hiba esetén piros keret
                    rp.BorderColor = Color.Red;
                    rp.BorderSize = 1;
                }
                else
                {
                    // Ha rendben van, vissza az alap szürke
                    rp.BorderColor = Color.FromArgb(230, 230, 230);
                    rp.BorderSize = 1;
                }
                rp.Invalidate();
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
            Form parentForm = this.FindForm();
            if (parentForm != null)
            {
                parentForm.WindowState = FormWindowState.Maximized;
            }
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

        //Login-ra visszalépés
        private void BackButtonReg_Click(object sender, EventArgs e)
        {
            if (this.ParentForm is TicketMaster mainForm)
            {
                mainForm.BackToLogin();
            }
        }

        //Animáció becsúszáshoz:
        private Timer slideTimer;
        private float currentY;
        private int targetY = 70;
        private int startY = 130;

        public void PlayInAnimation()
        {
            this.Visible = true;
            this.BringToFront();

            currentY = startY;
            RegisterPanel.Top = (int)currentY;
            RegisterPanel.Visible = true;

            if (slideTimer != null) slideTimer.Dispose();
            slideTimer = new Timer();

            slideTimer.Interval = 15;

            slideTimer.Tick += (s, e) =>
            {
                float diff = (targetY - currentY) * 0.15f;

                if (Math.Abs(diff) < 0.8f)
                {
                    diff = (targetY > currentY) ? 0.8f : -0.8f;
                }

                currentY += diff;
                RegisterPanel.Top = (int)Math.Round(currentY);

                if (Math.Abs(targetY - currentY) <= 1.0f)
                {
                    RegisterPanel.Top = targetY;
                    slideTimer.Stop();
                }
            };
            slideTimer.Start();
        }
    }
}