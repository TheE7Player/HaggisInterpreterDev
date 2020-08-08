namespace Haggis_Interpreter
{
    partial class Form1
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
            this.HaggisTextBox = new ScintillaNET.Scintilla();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.newFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.loadIsolatedFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearIsolatedFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.interpreterMenuStrip = new System.Windows.Forms.ToolStripSplitButton();
            this.runREPLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.interpreterSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.versionText = new System.Windows.Forms.ToolStripStatusLabel();
            this.caretPosition = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // HaggisTextBox
            // 
            this.HaggisTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HaggisTextBox.Lexer = ScintillaNET.Lexer.Null;
            this.HaggisTextBox.Location = new System.Drawing.Point(0, 28);
            this.HaggisTextBox.Name = "HaggisTextBox";
            this.HaggisTextBox.Size = new System.Drawing.Size(1264, 628);
            this.HaggisTextBox.TabIndex = 0;
            this.HaggisTextBox.WrapMode = ScintillaNET.WrapMode.Word;
            this.HaggisTextBox.Zoom = 6;
            this.HaggisTextBox.AutoCCompleted += new System.EventHandler<ScintillaNET.AutoCSelectionEventArgs>(this.HaggisTextBox_AutoCCompleted);
            this.HaggisTextBox.BeforeDelete += new System.EventHandler<ScintillaNET.BeforeModificationEventArgs>(this.HaggisTextBox_BeforeDelete);
            this.HaggisTextBox.CharAdded += new System.EventHandler<ScintillaNET.CharAddedEventArgs>(this.HaggisTextBox_CharAdded);
            this.HaggisTextBox.UpdateUI += new System.EventHandler<ScintillaNET.UpdateUIEventArgs>(this.HaggisTextBox_UpdateUI);
            this.HaggisTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HaggisTextBox_KeyDown);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.interpreterMenuStrip});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1264, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newFileToolStripMenuItem,
            this.openFileToolStripMenuItem,
            this.saveFileToolStripMenuItem,
            this.toolStripSeparator1,
            this.loadIsolatedFileMenuItem,
            this.clearIsolatedFileMenuItem,
            this.toolStripSeparator2,
            this.exitMenuItem});
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(38, 22);
            this.toolStripLabel1.Text = "File";
            // 
            // newFileToolStripMenuItem
            // 
            this.newFileToolStripMenuItem.Name = "newFileToolStripMenuItem";
            this.newFileToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.newFileToolStripMenuItem.Text = "New File";
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.openFileToolStripMenuItem.Text = "Open File";
            // 
            // saveFileToolStripMenuItem
            // 
            this.saveFileToolStripMenuItem.Name = "saveFileToolStripMenuItem";
            this.saveFileToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.saveFileToolStripMenuItem.Text = "Save File";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(173, 6);
            // 
            // loadIsolatedFileMenuItem
            // 
            this.loadIsolatedFileMenuItem.Enabled = false;
            this.loadIsolatedFileMenuItem.Name = "loadIsolatedFileMenuItem";
            this.loadIsolatedFileMenuItem.Size = new System.Drawing.Size(176, 22);
            this.loadIsolatedFileMenuItem.Text = "Load Last Run Item";
            this.loadIsolatedFileMenuItem.Click += new System.EventHandler(this.loadIsolatedFileMenuItem_Click);
            // 
            // clearIsolatedFileMenuItem
            // 
            this.clearIsolatedFileMenuItem.Enabled = false;
            this.clearIsolatedFileMenuItem.Name = "clearIsolatedFileMenuItem";
            this.clearIsolatedFileMenuItem.Size = new System.Drawing.Size(176, 22);
            this.clearIsolatedFileMenuItem.Text = "Clear Last Run Item";
            this.clearIsolatedFileMenuItem.Click += new System.EventHandler(this.clearIsolatedFileMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(173, 6);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(176, 22);
            this.exitMenuItem.Text = "Exit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // interpreterMenuStrip
            // 
            this.interpreterMenuStrip.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runREPLToolStripMenuItem,
            this.interpreterSettingsToolStripMenuItem});
            this.interpreterMenuStrip.Name = "interpreterMenuStrip";
            this.interpreterMenuStrip.Size = new System.Drawing.Size(78, 22);
            this.interpreterMenuStrip.Text = "Interpreter";
            this.interpreterMenuStrip.ButtonClick += new System.EventHandler(this.interpreterMenuStrip_ButtonClick);
            // 
            // runREPLToolStripMenuItem
            // 
            this.runREPLToolStripMenuItem.Name = "runREPLToolStripMenuItem";
            this.runREPLToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.runREPLToolStripMenuItem.Text = "Run Current Script  CTRL+R";
            this.runREPLToolStripMenuItem.Click += new System.EventHandler(this.runREPLToolStripMenuItem_Click);
            // 
            // interpreterSettingsToolStripMenuItem
            // 
            this.interpreterSettingsToolStripMenuItem.Name = "interpreterSettingsToolStripMenuItem";
            this.interpreterSettingsToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.interpreterSettingsToolStripMenuItem.Text = "Interpreter Settings";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.versionText,
            this.caretPosition});
            this.statusStrip1.Location = new System.Drawing.Point(0, 659);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(1264, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // versionText
            // 
            this.versionText.AutoSize = false;
            this.versionText.Name = "versionText";
            this.versionText.Size = new System.Drawing.Size(1182, 17);
            this.versionText.Spring = true;
            this.versionText.Text = "ALPHA BUILD";
            // 
            // caretPosition
            // 
            this.caretPosition.Name = "caretPosition";
            this.caretPosition.Size = new System.Drawing.Size(67, 17);
            this.caretPosition.Text = "Ch:0   Sel: 0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.HaggisTextBox);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Haggis Interpreter";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ScintillaNET.Scintilla HaggisTextBox;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripLabel1;
        private System.Windows.Forms.ToolStripMenuItem newFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem loadIsolatedFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.ToolStripSplitButton interpreterMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem runREPLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem interpreterSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearIsolatedFileMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel versionText;
        private System.Windows.Forms.ToolStripStatusLabel caretPosition;
    }
}

