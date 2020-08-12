namespace Haggis_Interpreter
{
    partial class Settings
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
            this.resetConfig = new System.Windows.Forms.Button();
            this.InterpreterVersions = new System.Windows.Forms.ComboBox();
            this.AddInterp = new System.Windows.Forms.Button();
            this.removeInterp = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // resetConfig
            // 
            this.resetConfig.Location = new System.Drawing.Point(213, 26);
            this.resetConfig.Name = "resetConfig";
            this.resetConfig.Size = new System.Drawing.Size(145, 23);
            this.resetConfig.TabIndex = 0;
            this.resetConfig.Text = "Reset Config";
            this.resetConfig.UseVisualStyleBackColor = true;
            this.resetConfig.Click += new System.EventHandler(this.resetConfig_Click);
            // 
            // InterpreterVersions
            // 
            this.InterpreterVersions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.InterpreterVersions.FormattingEnabled = true;
            this.InterpreterVersions.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.InterpreterVersions.Items.AddRange(new object[] {
            "asdasd",
            "asd"});
            this.InterpreterVersions.Location = new System.Drawing.Point(525, 28);
            this.InterpreterVersions.Name = "InterpreterVersions";
            this.InterpreterVersions.Size = new System.Drawing.Size(210, 21);
            this.InterpreterVersions.TabIndex = 1;
            this.InterpreterVersions.SelectedIndexChanged += new System.EventHandler(this.InterpreterVersions_SelectedIndexChanged);
            this.InterpreterVersions.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.InterpreterVersions_KeyPress);
            // 
            // AddInterp
            // 
            this.AddInterp.Location = new System.Drawing.Point(62, 26);
            this.AddInterp.Name = "AddInterp";
            this.AddInterp.Size = new System.Drawing.Size(145, 23);
            this.AddInterp.TabIndex = 2;
            this.AddInterp.Text = "Add Another Interp Ver";
            this.AddInterp.UseVisualStyleBackColor = true;
            this.AddInterp.Click += new System.EventHandler(this.AddInterp_Click);
            // 
            // removeInterp
            // 
            this.removeInterp.Location = new System.Drawing.Point(363, 26);
            this.removeInterp.Name = "removeInterp";
            this.removeInterp.Size = new System.Drawing.Size(145, 23);
            this.removeInterp.TabIndex = 3;
            this.removeInterp.Text = "Remove Current Interpreter";
            this.removeInterp.UseVisualStyleBackColor = true;
            this.removeInterp.Click += new System.EventHandler(this.removeInterp_Click);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.removeInterp);
            this.Controls.Add(this.AddInterp);
            this.Controls.Add(this.InterpreterVersions);
            this.Controls.Add(this.resetConfig);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Settings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Settings_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button resetConfig;
        private System.Windows.Forms.ComboBox InterpreterVersions;
        private System.Windows.Forms.Button AddInterp;
        private System.Windows.Forms.Button removeInterp;
    }
}