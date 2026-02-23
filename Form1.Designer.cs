using System;
using System.Drawing;
using System.Windows.Forms;

namespace ticketmaster
{
    using Controls = System.Windows.Forms.Control.ControlCollection;
    partial class TicketMaster
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TicketMaster));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            this.MoviesTable = new System.Windows.Forms.DataGridView();
            this.Title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Filmgenre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Runningtime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Language = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ProgramName = new System.Windows.Forms.Label();
            this.LoginText = new System.Windows.Forms.Label();
            this.useemail = new System.Windows.Forms.Label();
            this.EmailBox = new System.Windows.Forms.TextBox();
            this.CreateAccountButton = new System.Windows.Forms.LinkLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.picExit = new System.Windows.Forms.PictureBox();
            this.picMin = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.LoginButton = new ticketmaster.Controls.RoundedButton();
            this.tmrAnimate = new System.Windows.Forms.Timer(this.components);
            this.TicketButtonMain = new ticketmaster.Controls.RoundedButton();
            this.ScreeningsRounded = new ticketmaster.Controls.RoundedPanel();
            this.ScreeningsLabel = new System.Windows.Forms.Label();
            this.ScreeningsPanel = new ticketmaster.Controls.RoundedPanel();
            this.ScreeningsTable = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MoviesRounded = new ticketmaster.Controls.RoundedPanel();
            this.MoviesLabel = new System.Windows.Forms.Label();
            this.MoviesPanel = new ticketmaster.Controls.RoundedPanel();
            ((System.ComponentModel.ISupportInitialize)(this.MoviesTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picExit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMin)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.ScreeningsRounded.SuspendLayout();
            this.ScreeningsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ScreeningsTable)).BeginInit();
            this.MoviesRounded.SuspendLayout();
            this.SuspendLayout();
            // 
            // MoviesTable
            // 
            this.MoviesTable.AllowUserToAddRows = false;
            this.MoviesTable.BackgroundColor = System.Drawing.Color.White;
            this.MoviesTable.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MoviesTable.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.MoviesTable.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Inter Medium", 8F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.MoviesTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.MoviesTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.MoviesTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Title,
            this.Filmgenre,
            this.Runningtime,
            this.Language});
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Inter Medium", 7F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.MoviesTable.DefaultCellStyle = dataGridViewCellStyle6;
            this.MoviesTable.EnableHeadersVisualStyles = false;
            this.MoviesTable.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            resources.ApplyResources(this.MoviesTable, "MoviesTable");
            this.MoviesTable.Name = "MoviesTable";
            this.MoviesTable.ReadOnly = true;
            this.MoviesTable.RowHeadersVisible = false;
            this.MoviesTable.RowTemplate.Height = 35;
            this.MoviesTable.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.MoviesTable.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridView1_CellPainting);
            // 
            // Title
            // 
            this.Title.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.Title.DefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(this.Title, "Title");
            this.Title.Name = "Title";
            this.Title.ReadOnly = true;
            this.Title.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Title.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Filmgenre
            // 
            this.Filmgenre.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.Filmgenre.DefaultCellStyle = dataGridViewCellStyle3;
            resources.ApplyResources(this.Filmgenre, "Filmgenre");
            this.Filmgenre.Name = "Filmgenre";
            this.Filmgenre.ReadOnly = true;
            this.Filmgenre.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Filmgenre.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Runningtime
            // 
            this.Runningtime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Runningtime.DefaultCellStyle = dataGridViewCellStyle4;
            resources.ApplyResources(this.Runningtime, "Runningtime");
            this.Runningtime.Name = "Runningtime";
            this.Runningtime.ReadOnly = true;
            this.Runningtime.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Runningtime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Language
            // 
            this.Language.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Inter", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.Silver;
            this.Language.DefaultCellStyle = dataGridViewCellStyle5;
            resources.ApplyResources(this.Language, "Language");
            this.Language.Name = "Language";
            this.Language.ReadOnly = true;
            this.Language.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Language.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ProgramName
            // 
            resources.ApplyResources(this.ProgramName, "ProgramName");
            this.ProgramName.Name = "ProgramName";
            // 
            // LoginText
            // 
            resources.ApplyResources(this.LoginText, "LoginText");
            this.LoginText.Name = "LoginText";
            // 
            // useemail
            // 
            resources.ApplyResources(this.useemail, "useemail");
            this.useemail.Name = "useemail";
            // 
            // EmailBox
            // 
            resources.ApplyResources(this.EmailBox, "EmailBox");
            this.EmailBox.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.EmailBox.Name = "EmailBox";
            this.EmailBox.TextChanged += new System.EventHandler(this.EmailBox_TextChanged);
            // 
            // CreateAccountButton
            // 
            this.CreateAccountButton.ActiveLinkColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.CreateAccountButton, "CreateAccountButton");
            this.CreateAccountButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CreateAccountButton.ForeColor = System.Drawing.Color.Cyan;
            this.CreateAccountButton.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.CreateAccountButton.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(130)))), ((int)(((byte)(130)))));
            this.CreateAccountButton.Name = "CreateAccountButton";
            this.CreateAccountButton.TabStop = true;
            this.CreateAccountButton.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(130)))), ((int)(((byte)(130)))));
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(246)))), ((int)(((byte)(246)))));
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.richTextBox1, "richTextBox1");
            this.richTextBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(130)))), ((int)(((byte)(130)))));
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            // 
            // picExit
            // 
            resources.ApplyResources(this.picExit, "picExit");
            this.picExit.Name = "picExit";
            this.picExit.TabStop = false;
            this.picExit.Click += new System.EventHandler(this.picExit_Click);
            this.picExit.Paint += new System.Windows.Forms.PaintEventHandler(this.picExit_Paint);
            this.picExit.MouseEnter += new System.EventHandler(this.picExit_MouseEnter);
            this.picExit.MouseLeave += new System.EventHandler(this.picExit_MouseLeave);
            // 
            // picMin
            // 
            this.picMin.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.picMin, "picMin");
            this.picMin.Name = "picMin";
            this.picMin.TabStop = false;
            this.picMin.Click += new System.EventHandler(this.picMin_Click);
            this.picMin.Paint += new System.Windows.Forms.PaintEventHandler(this.picMin_Paint);
            this.picMin.MouseEnter += new System.EventHandler(this.picMin_MouseEnter);
            this.picMin.MouseLeave += new System.EventHandler(this.picMin_MouseLeave);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Name = "label4";
            // 
            // txtPassword
            // 
            resources.ApplyResources(this.txtPassword, "txtPassword");
            this.txtPassword.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.txtPassword.Name = "txtPassword";
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.LoginButton);
            this.pnlBottom.Controls.Add(this.CreateAccountButton);
            this.pnlBottom.Controls.Add(this.panel1);
            this.pnlBottom.Controls.Add(this.panel2);
            this.pnlBottom.Controls.Add(this.richTextBox1);
            resources.ApplyResources(this.pnlBottom, "pnlBottom");
            this.pnlBottom.Name = "pnlBottom";
            // 
            // LoginButton
            // 
            this.LoginButton.BackColor = System.Drawing.Color.Black;
            this.LoginButton.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(this.LoginButton, "LoginButton");
            this.LoginButton.ForeColor = System.Drawing.Color.White;
            this.LoginButton.Name = "LoginButton";
            this.LoginButton.UseVisualStyleBackColor = false;
            // 
            // tmrAnimate
            // 
            this.tmrAnimate.Interval = 10;
            this.tmrAnimate.Tick += new System.EventHandler(this.tmrAnimate_Tick);
            // 
            // TicketButtonMain
            // 
            this.TicketButtonMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.TicketButtonMain.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(this.TicketButtonMain, "TicketButtonMain");
            this.TicketButtonMain.ForeColor = System.Drawing.Color.White;
            this.TicketButtonMain.Name = "TicketButtonMain";
            this.TicketButtonMain.UseVisualStyleBackColor = false;
            // 
            // ScreeningsRounded
            // 
            this.ScreeningsRounded.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.ScreeningsRounded.BorderRadius = 10;
            this.ScreeningsRounded.Controls.Add(this.ScreeningsLabel);
            resources.ApplyResources(this.ScreeningsRounded, "ScreeningsRounded");
            this.ScreeningsRounded.Name = "ScreeningsRounded";
            // 
            // ScreeningsLabel
            // 
            resources.ApplyResources(this.ScreeningsLabel, "ScreeningsLabel");
            this.ScreeningsLabel.ForeColor = System.Drawing.Color.White;
            this.ScreeningsLabel.Name = "ScreeningsLabel";
            // 
            // ScreeningsPanel
            // 
            this.ScreeningsPanel.BackColor = System.Drawing.Color.White;
            this.ScreeningsPanel.BorderRadius = 20;
            this.ScreeningsPanel.Controls.Add(this.ScreeningsTable);
            resources.ApplyResources(this.ScreeningsPanel, "ScreeningsPanel");
            this.ScreeningsPanel.Name = "ScreeningsPanel";
            // 
            // ScreeningsTable
            // 
            this.ScreeningsTable.AllowUserToAddRows = false;
            this.ScreeningsTable.BackgroundColor = System.Drawing.Color.White;
            this.ScreeningsTable.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ScreeningsTable.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.ScreeningsTable.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Inter Medium", 8F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ScreeningsTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.ScreeningsTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ScreeningsTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3});
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Inter Medium", 7F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle11.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle11.SelectionBackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle11.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.ScreeningsTable.DefaultCellStyle = dataGridViewCellStyle11;
            this.ScreeningsTable.EnableHeadersVisualStyles = false;
            this.ScreeningsTable.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            resources.ApplyResources(this.ScreeningsTable, "ScreeningsTable");
            this.ScreeningsTable.Name = "ScreeningsTable";
            this.ScreeningsTable.ReadOnly = true;
            this.ScreeningsTable.RowHeadersVisible = false;
            this.ScreeningsTable.RowTemplate.Height = 35;
            this.ScreeningsTable.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ScreeningsTable.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.ScreeningsTable_CellPainting);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.dataGridViewTextBoxColumn1.DefaultCellStyle = dataGridViewCellStyle8;
            resources.ApplyResources(this.dataGridViewTextBoxColumn1, "dataGridViewTextBoxColumn1");
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.dataGridViewTextBoxColumn2.DefaultCellStyle = dataGridViewCellStyle9;
            resources.ApplyResources(this.dataGridViewTextBoxColumn2, "dataGridViewTextBoxColumn2");
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dataGridViewTextBoxColumn3.DefaultCellStyle = dataGridViewCellStyle10;
            resources.ApplyResources(this.dataGridViewTextBoxColumn3, "dataGridViewTextBoxColumn3");
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // MoviesRounded
            // 
            this.MoviesRounded.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.MoviesRounded.BorderRadius = 10;
            this.MoviesRounded.Controls.Add(this.MoviesLabel);
            resources.ApplyResources(this.MoviesRounded, "MoviesRounded");
            this.MoviesRounded.Name = "MoviesRounded";
            // 
            // MoviesLabel
            // 
            resources.ApplyResources(this.MoviesLabel, "MoviesLabel");
            this.MoviesLabel.ForeColor = System.Drawing.Color.White;
            this.MoviesLabel.Name = "MoviesLabel";
            // 
            // MoviesPanel
            // 
            this.MoviesPanel.BackColor = System.Drawing.Color.White;
            this.MoviesPanel.BorderRadius = 20;
            resources.ApplyResources(this.MoviesPanel, "MoviesPanel");
            this.MoviesPanel.Name = "MoviesPanel";
            // 
            // TicketMaster
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(246)))), ((int)(((byte)(246)))));
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.TicketButtonMain);
            this.Controls.Add(this.ScreeningsRounded);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ScreeningsPanel);
            this.Controls.Add(this.picMin);
            this.Controls.Add(this.picExit);
            this.Controls.Add(this.MoviesRounded);
            this.Controls.Add(this.EmailBox);
            this.Controls.Add(this.useemail);
            this.Controls.Add(this.LoginText);
            this.Controls.Add(this.ProgramName);
            this.Controls.Add(this.MoviesTable);
            this.Controls.Add(this.MoviesPanel);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.txtPassword);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TicketMaster";
            ((System.ComponentModel.ISupportInitialize)(this.MoviesTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picExit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMin)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.ScreeningsRounded.ResumeLayout(false);
            this.ScreeningsRounded.PerformLayout();
            this.ScreeningsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ScreeningsTable)).EndInit();
            this.MoviesRounded.ResumeLayout(false);
            this.MoviesRounded.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ticketmaster.Controls.RoundedPanel asd;
        private ticketmaster.Controls.RoundedPanel MoviesPanel;
        private System.Windows.Forms.DataGridView MoviesTable;
        private System.Windows.Forms.Label ProgramName;
        private System.Windows.Forms.Label LoginText;
        private System.Windows.Forms.Label useemail;
        private System.Windows.Forms.TextBox EmailBox;
        private ticketmaster.Controls.RoundedButton LoginButton;
        private System.Windows.Forms.LinkLabel CreateAccountButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private ticketmaster.Controls.RoundedPanel MoviesRounded;
        private System.Windows.Forms.Label MoviesLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn Title;
        private System.Windows.Forms.DataGridViewTextBoxColumn Filmgenre;
        private System.Windows.Forms.DataGridViewTextBoxColumn Runningtime;
        private System.Windows.Forms.DataGridViewTextBoxColumn Language;
        private System.Windows.Forms.PictureBox picExit;
        private System.Windows.Forms.PictureBox picMin;
        private ticketmaster.Controls.RoundedPanel ScreeningsPanel;
        private System.Windows.Forms.Label label4;
        private ticketmaster.Controls.RoundedPanel ScreeningsRounded;
        private System.Windows.Forms.Label ScreeningsLabel;
        private System.Windows.Forms.DataGridView ScreeningsTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private ticketmaster.Controls.RoundedButton TicketButtonMain;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Timer tmrAnimate;
    }
}

