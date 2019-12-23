namespace ShIDE
{
	partial class MainForm
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
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.evaluateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.evaluateMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.cleanMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.formatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.autoDoMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.editor = new ScintillaNET.Scintilla();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.txtInput = new System.Windows.Forms.TextBox();
			this.console = new ConsoleControl.ConsoleControl();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.quickParenMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.evaluateToolStripMenuItem,
            this.formatToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(938, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// evaluateToolStripMenuItem
			// 
			this.evaluateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.evaluateMenu,
            this.cleanMenu});
			this.evaluateToolStripMenuItem.Name = "evaluateToolStripMenuItem";
			this.evaluateToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
			this.evaluateToolStripMenuItem.Text = "E&valuate";
			// 
			// evaluateMenu
			// 
			this.evaluateMenu.Name = "evaluateMenu";
			this.evaluateMenu.ShortcutKeys = System.Windows.Forms.Keys.F5;
			this.evaluateMenu.Size = new System.Drawing.Size(181, 22);
			this.evaluateMenu.Text = "E&valuate";
			this.evaluateMenu.Click += new System.EventHandler(this.evaluateMenu_Click);
			// 
			// cleanMenu
			// 
			this.cleanMenu.Name = "cleanMenu";
			this.cleanMenu.ShortcutKeys = System.Windows.Forms.Keys.F4;
			this.cleanMenu.Size = new System.Drawing.Size(181, 22);
			this.cleanMenu.Text = "&Clean Interpreter";
			this.cleanMenu.Click += new System.EventHandler(this.cleanMenu_Click);
			// 
			// formatToolStripMenuItem
			// 
			this.formatToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoDoMenu,
            this.quickParenMenu});
			this.formatToolStripMenuItem.Name = "formatToolStripMenuItem";
			this.formatToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
			this.formatToolStripMenuItem.Text = "Forma&t";
			// 
			// autoDoMenu
			// 
			this.autoDoMenu.Name = "autoDoMenu";
			this.autoDoMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
			this.autoDoMenu.Size = new System.Drawing.Size(211, 22);
			this.autoDoMenu.Text = "Surround With &Do";
			this.autoDoMenu.Click += new System.EventHandler(this.autoDoMenu_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Location = new System.Drawing.Point(0, 538);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(938, 22);
			this.statusStrip1.TabIndex = 2;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 24);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = new System.Drawing.Size(938, 514);
			this.splitContainer1.SplitterDistance = 312;
			this.splitContainer1.TabIndex = 3;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.editor);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.tabControl1);
			this.splitContainer2.Size = new System.Drawing.Size(622, 514);
			this.splitContainer2.SplitterDistance = 316;
			this.splitContainer2.TabIndex = 0;
			// 
			// editor
			// 
			this.editor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.editor.CaretForeColor = System.Drawing.Color.Yellow;
			this.editor.CaretLineBackColor = System.Drawing.Color.LemonChiffon;
			this.editor.Location = new System.Drawing.Point(3, 3);
			this.editor.Name = "editor";
			this.editor.Size = new System.Drawing.Size(616, 310);
			this.editor.TabIndex = 0;
			this.editor.StyleNeeded += new System.EventHandler<ScintillaNET.StyleNeededEventArgs>(this.editor_StyleNeeded);
			this.editor.UpdateUI += new System.EventHandler<ScintillaNET.UpdateUIEventArgs>(this.editor_UpdateUI);
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Location = new System.Drawing.Point(3, 3);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(619, 191);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.txtInput);
			this.tabPage1.Controls.Add(this.console);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(611, 165);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Console";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// txtInput
			// 
			this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.txtInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtInput.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtInput.ForeColor = System.Drawing.Color.LightGray;
			this.txtInput.Location = new System.Drawing.Point(40, 141);
			this.txtInput.Name = "txtInput";
			this.txtInput.Size = new System.Drawing.Size(536, 25);
			this.txtInput.TabIndex = 1;
			this.txtInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtInput_KeyPress);
			// 
			// console
			// 
			this.console.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.console.IsInputEnabled = true;
			this.console.Location = new System.Drawing.Point(0, 0);
			this.console.Name = "console";
			this.console.SendKeyboardCommandsToProcess = false;
			this.console.ShowDiagnostics = false;
			this.console.Size = new System.Drawing.Size(611, 166);
			this.console.TabIndex = 0;
			// 
			// tabPage2
			// 
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(611, 165);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "tabPage2";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// quickParenMenu
			// 
			this.quickParenMenu.Name = "quickParenMenu";
			this.quickParenMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
			this.quickParenMenu.Size = new System.Drawing.Size(211, 22);
			this.quickParenMenu.Text = "Quick &Parenthesis";
			this.quickParenMenu.Click += new System.EventHandler(this.quickParenMenu_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(938, 560);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "MainForm";
			this.Text = "ShIDE, a shiro code editor";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private ScintillaNET.Scintilla editor;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private ConsoleControl.ConsoleControl console;
		private System.Windows.Forms.TextBox txtInput;
		private System.Windows.Forms.ToolStripMenuItem evaluateToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem evaluateMenu;
		private System.Windows.Forms.ToolStripMenuItem cleanMenu;
		private System.Windows.Forms.ToolStripMenuItem formatToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem autoDoMenu;
		private System.Windows.Forms.ToolStripMenuItem quickParenMenu;
	}
}

