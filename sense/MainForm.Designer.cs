namespace Shiro.Sense
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.openMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.findOrGoNextMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.highlightSelectionMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.cutMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.copyMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.projectMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.newProjectMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.saveProjectMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.saveProjectAsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.closeProjectMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
            this.addFileToProjectMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.removeFileFromProjectMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.evaluateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.evaluateMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.cleanMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.compileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.formatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoDoMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.quickParenMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.showAutocompleteMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.showHelptipMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.navigateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nextListMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.prevListMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.nextListSelectionMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.previousListSelectionMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.endOfListMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.selectToEOLMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showResultMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.registerWindowsExplorerContextMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.evalStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tree = new System.Windows.Forms.TreeView();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.editor = new ScintillaNET.Scintilla();
            this.bottomTabs = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.console = new ConsoleControl.ConsoleControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.terminal = new ConsoleControl.ConsoleControl();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveStateTimer = new System.Windows.Forms.Timer(this.components);
            this.saveProjectDialog = new System.Windows.Forms.SaveFileDialog();
            this.treeImages = new System.Windows.Forms.ImageList(this.components);
            this.editorTabs = new Shiro.Sense.Controls.DraggableTabControl();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.bottomTabs.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.projectMenu,
            this.evaluateToolStripMenuItem,
            this.formatToolStripMenuItem,
            this.navigateToolStripMenuItem,
            this.optionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1122, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newMenu,
            this.openMenu,
            this.saveMenu,
            this.saveAsMenu});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newMenu
            // 
            this.newMenu.Name = "newMenu";
            this.newMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newMenu.Size = new System.Drawing.Size(146, 22);
            this.newMenu.Text = "&New";
            this.newMenu.Click += new System.EventHandler(this.newMenu_Click);
            // 
            // openMenu
            // 
            this.openMenu.Name = "openMenu";
            this.openMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openMenu.Size = new System.Drawing.Size(146, 22);
            this.openMenu.Text = "&Open";
            this.openMenu.Click += new System.EventHandler(this.openMenu_Click);
            // 
            // saveMenu
            // 
            this.saveMenu.Name = "saveMenu";
            this.saveMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveMenu.Size = new System.Drawing.Size(146, 22);
            this.saveMenu.Text = "&Save";
            this.saveMenu.Click += new System.EventHandler(this.saveMenu_Click);
            // 
            // saveAsMenu
            // 
            this.saveAsMenu.Name = "saveAsMenu";
            this.saveAsMenu.Size = new System.Drawing.Size(146, 22);
            this.saveAsMenu.Text = "Save &As...";
            this.saveAsMenu.Click += new System.EventHandler(this.saveAsMenu_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoMenu,
            this.findToolStripMenuItem,
            this.toolStripMenuItem6,
            this.cutMenu,
            this.copyMenu,
            this.pasteMenu});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // undoMenu
            // 
            this.undoMenu.Name = "undoMenu";
            this.undoMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.undoMenu.Size = new System.Drawing.Size(144, 22);
            this.undoMenu.Text = "&Undo";
            this.undoMenu.Click += new System.EventHandler(this.undoMenu_Click);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findMenu,
            this.findOrGoNextMenu,
            this.toolStripMenuItem4,
            this.highlightSelectionMenu});
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.findToolStripMenuItem.Text = "&Search";
            // 
            // findMenu
            // 
            this.findMenu.Name = "findMenu";
            this.findMenu.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.F)));
            this.findMenu.Size = new System.Drawing.Size(214, 22);
            this.findMenu.Text = "&Find";
            this.findMenu.Click += new System.EventHandler(this.findMenu_Click);
            // 
            // findOrGoNextMenu
            // 
            this.findOrGoNextMenu.Name = "findOrGoNextMenu";
            this.findOrGoNextMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findOrGoNextMenu.Size = new System.Drawing.Size(214, 22);
            this.findOrGoNextMenu.Text = "Find/&Goto Next";
            this.findOrGoNextMenu.Click += new System.EventHandler(this.findOrGoNextMenu_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(211, 6);
            // 
            // highlightSelectionMenu
            // 
            this.highlightSelectionMenu.Name = "highlightSelectionMenu";
            this.highlightSelectionMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.highlightSelectionMenu.Size = new System.Drawing.Size(214, 22);
            this.highlightSelectionMenu.Text = "&Highlight Selected";
            this.highlightSelectionMenu.Click += new System.EventHandler(this.highlightSelectionMenu_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(141, 6);
            // 
            // cutMenu
            // 
            this.cutMenu.Name = "cutMenu";
            this.cutMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.cutMenu.Size = new System.Drawing.Size(144, 22);
            this.cutMenu.Text = "Cu&t";
            this.cutMenu.Click += new System.EventHandler(this.cutMenu_Click);
            // 
            // copyMenu
            // 
            this.copyMenu.Name = "copyMenu";
            this.copyMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyMenu.Size = new System.Drawing.Size(144, 22);
            this.copyMenu.Text = "&Copy";
            this.copyMenu.Click += new System.EventHandler(this.copyMenu_Click);
            // 
            // pasteMenu
            // 
            this.pasteMenu.Name = "pasteMenu";
            this.pasteMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteMenu.Size = new System.Drawing.Size(144, 22);
            this.pasteMenu.Text = "&Paste";
            this.pasteMenu.Click += new System.EventHandler(this.pasteMenu_Click);
            // 
            // projectMenu
            // 
            this.projectMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newProjectMenu,
            this.saveProjectMenu,
            this.saveProjectAsMenu,
            this.closeProjectMenu,
            this.toolStripMenuItem7,
            this.addFileToProjectMenu,
            this.removeFileFromProjectMenu});
            this.projectMenu.Name = "projectMenu";
            this.projectMenu.Size = new System.Drawing.Size(56, 20);
            this.projectMenu.Text = "&Project";
            // 
            // newProjectMenu
            // 
            this.newProjectMenu.Name = "newProjectMenu";
            this.newProjectMenu.Size = new System.Drawing.Size(210, 22);
            this.newProjectMenu.Text = "&New Project";
            this.newProjectMenu.Click += new System.EventHandler(this.newProjectMenu_Click);
            // 
            // saveProjectMenu
            // 
            this.saveProjectMenu.Name = "saveProjectMenu";
            this.saveProjectMenu.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveProjectMenu.Size = new System.Drawing.Size(210, 22);
            this.saveProjectMenu.Text = "&Save Project";
            this.saveProjectMenu.Click += new System.EventHandler(this.saveProjectMenu_Click);
            // 
            // saveProjectAsMenu
            // 
            this.saveProjectAsMenu.Name = "saveProjectAsMenu";
            this.saveProjectAsMenu.Size = new System.Drawing.Size(210, 22);
            this.saveProjectAsMenu.Text = "Save Project &As";
            this.saveProjectAsMenu.Click += new System.EventHandler(this.saveProjectAsMenu_Click);
            // 
            // closeProjectMenu
            // 
            this.closeProjectMenu.Name = "closeProjectMenu";
            this.closeProjectMenu.Size = new System.Drawing.Size(210, 22);
            this.closeProjectMenu.Text = "&Close Project";
            this.closeProjectMenu.Click += new System.EventHandler(this.closeProjectMenu_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(207, 6);
            // 
            // addFileToProjectMenu
            // 
            this.addFileToProjectMenu.Name = "addFileToProjectMenu";
            this.addFileToProjectMenu.ShortcutKeyDisplayString = "Ctrl+ +";
            this.addFileToProjectMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Oemplus)));
            this.addFileToProjectMenu.Size = new System.Drawing.Size(210, 22);
            this.addFileToProjectMenu.Text = "&Add File";
            this.addFileToProjectMenu.Click += new System.EventHandler(this.addFileToProjectMenu_Click);
            // 
            // removeFileFromProjectMenu
            // 
            this.removeFileFromProjectMenu.Name = "removeFileFromProjectMenu";
            this.removeFileFromProjectMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete)));
            this.removeFileFromProjectMenu.Size = new System.Drawing.Size(210, 22);
            this.removeFileFromProjectMenu.Text = "&Remove File";
            this.removeFileFromProjectMenu.Click += new System.EventHandler(this.removeFileFromProjectMenu_Click);
            // 
            // evaluateToolStripMenuItem
            // 
            this.evaluateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.evaluateMenu,
            this.cleanMenu,
            this.toolStripMenuItem3,
            this.compileMenu});
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
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(178, 6);
            // 
            // compileMenu
            // 
            this.compileMenu.Name = "compileMenu";
            this.compileMenu.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this.compileMenu.Size = new System.Drawing.Size(181, 22);
            this.compileMenu.Text = "Com&pile";
            this.compileMenu.Click += new System.EventHandler(this.compileMenu_Click);
            // 
            // formatToolStripMenuItem
            // 
            this.formatToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoDoMenu,
            this.quickParenMenu,
            this.toolStripMenuItem1,
            this.showAutocompleteMenu,
            this.showHelptipMenu});
            this.formatToolStripMenuItem.Name = "formatToolStripMenuItem";
            this.formatToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.formatToolStripMenuItem.Text = "Forma&t";
            // 
            // autoDoMenu
            // 
            this.autoDoMenu.Name = "autoDoMenu";
            this.autoDoMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.autoDoMenu.Size = new System.Drawing.Size(247, 22);
            this.autoDoMenu.Text = "Surround With &Do";
            this.autoDoMenu.Click += new System.EventHandler(this.autoDoMenu_Click);
            // 
            // quickParenMenu
            // 
            this.quickParenMenu.Name = "quickParenMenu";
            this.quickParenMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.quickParenMenu.Size = new System.Drawing.Size(247, 22);
            this.quickParenMenu.Text = "Quick &Parenthesis";
            this.quickParenMenu.Click += new System.EventHandler(this.quickParenMenu_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(244, 6);
            // 
            // showAutocompleteMenu
            // 
            this.showAutocompleteMenu.Name = "showAutocompleteMenu";
            this.showAutocompleteMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Space)));
            this.showAutocompleteMenu.Size = new System.Drawing.Size(247, 22);
            this.showAutocompleteMenu.Text = "Show &Autocomplete";
            this.showAutocompleteMenu.Click += new System.EventHandler(this.showAutocompleteMenu_Click);
            // 
            // showHelptipMenu
            // 
            this.showHelptipMenu.Name = "showHelptipMenu";
            this.showHelptipMenu.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.showHelptipMenu.Size = new System.Drawing.Size(247, 22);
            this.showHelptipMenu.Text = "Show &Helptip";
            this.showHelptipMenu.Click += new System.EventHandler(this.showHelptipMenu_Click);
            // 
            // navigateToolStripMenuItem
            // 
            this.navigateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nextListMenu,
            this.prevListMenu,
            this.nextListSelectionMenu,
            this.previousListSelectionMenu,
            this.endOfListMenu,
            this.selectToEOLMenu,
            this.toolStripMenuItem2});
            this.navigateToolStripMenuItem.Name = "navigateToolStripMenuItem";
            this.navigateToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.navigateToolStripMenuItem.Text = "&Navigate";
            // 
            // nextListMenu
            // 
            this.nextListMenu.Name = "nextListMenu";
            this.nextListMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.nextListMenu.Size = new System.Drawing.Size(273, 22);
            this.nextListMenu.Text = "Go to &Next List";
            this.nextListMenu.Click += new System.EventHandler(this.nextListMenu_Click);
            // 
            // prevListMenu
            // 
            this.prevListMenu.Name = "prevListMenu";
            this.prevListMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.prevListMenu.Size = new System.Drawing.Size(273, 22);
            this.prevListMenu.Text = "Go to &Previous List";
            this.prevListMenu.Click += new System.EventHandler(this.prevListMenu_Click);
            // 
            // nextListSelectionMenu
            // 
            this.nextListSelectionMenu.Name = "nextListSelectionMenu";
            this.nextListSelectionMenu.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.E)));
            this.nextListSelectionMenu.Size = new System.Drawing.Size(273, 22);
            this.nextListSelectionMenu.Text = "Next List (selection)";
            this.nextListSelectionMenu.Click += new System.EventHandler(this.nextListMenu_Click);
            // 
            // previousListSelectionMenu
            // 
            this.previousListSelectionMenu.Name = "previousListSelectionMenu";
            this.previousListSelectionMenu.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.Q)));
            this.previousListSelectionMenu.Size = new System.Drawing.Size(273, 22);
            this.previousListSelectionMenu.Text = "Previous List (selection)";
            this.previousListSelectionMenu.Click += new System.EventHandler(this.prevListMenu_Click);
            // 
            // endOfListMenu
            // 
            this.endOfListMenu.Name = "endOfListMenu";
            this.endOfListMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.endOfListMenu.Size = new System.Drawing.Size(273, 22);
            this.endOfListMenu.Text = "Go to &End of List";
            this.endOfListMenu.Click += new System.EventHandler(this.endOfListMenu_Click);
            // 
            // selectToEOLMenu
            // 
            this.selectToEOLMenu.Name = "selectToEOLMenu";
            this.selectToEOLMenu.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.W)));
            this.selectToEOLMenu.Size = new System.Drawing.Size(273, 22);
            this.selectToEOLMenu.Text = "Select to end of List";
            this.selectToEOLMenu.Click += new System.EventHandler(this.endOfListMenu_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(270, 6);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showResultMenu,
            this.toolStripMenuItem5,
            this.registerWindowsExplorerContextMenuToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "&Options";
            // 
            // showResultMenu
            // 
            this.showResultMenu.CheckOnClick = true;
            this.showResultMenu.Name = "showResultMenu";
            this.showResultMenu.Size = new System.Drawing.Size(291, 22);
            this.showResultMenu.Text = "&Show Result of Eval";
            this.showResultMenu.Click += new System.EventHandler(this.showResultMenu_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(288, 6);
            // 
            // registerWindowsExplorerContextMenuToolStripMenuItem
            // 
            this.registerWindowsExplorerContextMenuToolStripMenuItem.Name = "registerWindowsExplorerContextMenuToolStripMenuItem";
            this.registerWindowsExplorerContextMenuToolStripMenuItem.Size = new System.Drawing.Size(291, 22);
            this.registerWindowsExplorerContextMenuToolStripMenuItem.Text = "Register Windows Explorer Context Menu";
            this.registerWindowsExplorerContextMenuToolStripMenuItem.Click += new System.EventHandler(this.registerWindowsExplorerContextMenuToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.evalStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 670);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1122, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // evalStatusLabel
            // 
            this.evalStatusLabel.AutoSize = false;
            this.evalStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.evalStatusLabel.Name = "evalStatusLabel";
            this.evalStatusLabel.Size = new System.Drawing.Size(125, 17);
            this.evalStatusLabel.Text = "Not Evaluating";
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
            this.splitContainer1.Panel1.Controls.Add(this.tree);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1122, 646);
            this.splitContainer1.SplitterDistance = 234;
            this.splitContainer1.TabIndex = 3;
            // 
            // tree
            // 
            this.tree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tree.BackColor = System.Drawing.Color.Gray;
            this.tree.Font = new System.Drawing.Font("Lucida Sans Unicode", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tree.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.tree.ImageIndex = 2;
            this.tree.ImageList = this.treeImages;
            this.tree.ItemHeight = 32;
            this.tree.Location = new System.Drawing.Point(3, 3);
            this.tree.Name = "tree";
            this.tree.SelectedImageIndex = 0;
            this.tree.ShowRootLines = false;
            this.tree.Size = new System.Drawing.Size(228, 640);
            this.tree.TabIndex = 0;
            this.tree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tree_NodeMouseDoubleClick);
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
            this.splitContainer2.Panel1.Controls.Add(this.editorTabs);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.bottomTabs);
            this.splitContainer2.Size = new System.Drawing.Size(884, 646);
            this.splitContainer2.SplitterDistance = 397;
            this.splitContainer2.TabIndex = 0;
            // 
            // editor
            // 
            this.editor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editor.AutoCOrder = ScintillaNET.Order.PerformSort;
            this.editor.CaretForeColor = System.Drawing.Color.Yellow;
            this.editor.CaretLineBackColor = System.Drawing.Color.LemonChiffon;
            this.editor.Location = new System.Drawing.Point(3, 26);
            this.editor.Name = "editor";
            this.editor.Size = new System.Drawing.Size(878, 368);
            this.editor.TabIndex = 0;
            this.editor.CharAdded += new System.EventHandler<ScintillaNET.CharAddedEventArgs>(this.editor_CharAdded);
            this.editor.DoubleClick += new System.EventHandler<ScintillaNET.DoubleClickEventArgs>(this.editor_DoubleClick);
            this.editor.DwellEnd += new System.EventHandler<ScintillaNET.DwellEventArgs>(this.editor_DwellEnd);
            this.editor.DwellStart += new System.EventHandler<ScintillaNET.DwellEventArgs>(this.editor_DwellStart);
            this.editor.InsertCheck += new System.EventHandler<ScintillaNET.InsertCheckEventArgs>(this.editor_InsertCheck);
            this.editor.StyleNeeded += new System.EventHandler<ScintillaNET.StyleNeededEventArgs>(this.editor_StyleNeeded);
            this.editor.UpdateUI += new System.EventHandler<ScintillaNET.UpdateUIEventArgs>(this.editor_UpdateUI);
            this.editor.Click += new System.EventHandler(this.editor_Click);
            this.editor.KeyDown += new System.Windows.Forms.KeyEventHandler(this.editor_KeyPress);
            // 
            // bottomTabs
            // 
            this.bottomTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomTabs.Controls.Add(this.tabPage1);
            this.bottomTabs.Controls.Add(this.tabPage2);
            this.bottomTabs.ImageList = this.treeImages;
            this.bottomTabs.Location = new System.Drawing.Point(3, 3);
            this.bottomTabs.Name = "bottomTabs";
            this.bottomTabs.SelectedIndex = 0;
            this.bottomTabs.Size = new System.Drawing.Size(881, 242);
            this.bottomTabs.TabIndex = 0;
            this.bottomTabs.SelectedIndexChanged += new System.EventHandler(this.bottomTabs_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.txtInput);
            this.tabPage1.Controls.Add(this.console);
            this.tabPage1.Location = new System.Drawing.Point(4, 39);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(873, 199);
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
            this.txtInput.Location = new System.Drawing.Point(40, 175);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(798, 25);
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
            this.console.Size = new System.Drawing.Size(873, 200);
            this.console.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.terminal);
            this.tabPage2.Location = new System.Drawing.Point(4, 39);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(873, 199);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Terminal";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // terminal
            // 
            this.terminal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.terminal.IsInputEnabled = true;
            this.terminal.Location = new System.Drawing.Point(0, 0);
            this.terminal.Name = "terminal";
            this.terminal.SendKeyboardCommandsToProcess = true;
            this.terminal.ShowDiagnostics = false;
            this.terminal.Size = new System.Drawing.Size(873, 200);
            this.terminal.TabIndex = 1;
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "shr";
            this.saveFileDialog.Filter = "Shiro Files|*.shr|Everything|*.*";
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "shr";
            this.openFileDialog.Filter = "Shiro Files|*.shr*|Shiro Projects|*.shrp|Everything|*.*";
            // 
            // saveStateTimer
            // 
            this.saveStateTimer.Interval = 250;
            this.saveStateTimer.Tick += new System.EventHandler(this.saveStateTimer_Tick);
            // 
            // saveProjectDialog
            // 
            this.saveProjectDialog.DefaultExt = "shr";
            this.saveProjectDialog.Filter = "Shiro Projects|*.shrp";
            // 
            // treeImages
            // 
            this.treeImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("treeImages.ImageStream")));
            this.treeImages.TransparentColor = System.Drawing.Color.Transparent;
            this.treeImages.Images.SetKeyName(0, "appbar.folder.open.png");
            this.treeImages.Images.SetKeyName(1, "appbar.folder.png");
            this.treeImages.Images.SetKeyName(2, "appbar.page.small.png");
            // 
            // editorTabs
            // 
            this.editorTabs.AllowDrop = true;
            this.editorTabs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editorTabs.Location = new System.Drawing.Point(7, 4);
            this.editorTabs.Name = "editorTabs";
            this.editorTabs.SelectedIndex = 0;
            this.editorTabs.Size = new System.Drawing.Size(873, 24);
            this.editorTabs.TabIndex = 1;
            this.editorTabs.SelectedIndexChanged += new System.EventHandler(this.editorTabs_SelectedIndexChanged);
            this.editorTabs.MouseUp += new System.Windows.Forms.MouseEventHandler(this.editorTabs_MouseUp);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1122, 692);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "SENSE: the Elegant New Shiro Editor v0.3 -";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.bottomTabs.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
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
		private System.Windows.Forms.TabControl bottomTabs;
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
        private Shiro.Sense.Controls.DraggableTabControl editorTabs;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ToolStripMenuItem newMenu;
        private System.Windows.Forms.ToolStripMenuItem openMenu;
        private System.Windows.Forms.ToolStripMenuItem saveMenu;
        private System.Windows.Forms.ToolStripMenuItem saveAsMenu;
        private System.Windows.Forms.ToolStripStatusLabel evalStatusLabel;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem showAutocompleteMenu;
        private ConsoleControl.ConsoleControl terminal;
		private System.Windows.Forms.Timer saveStateTimer;
		private System.Windows.Forms.ToolStripMenuItem navigateToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem registerWindowsExplorerContextMenuToolStripMenuItem;
		private System.Windows.Forms.TreeView tree;
        private System.Windows.Forms.ToolStripMenuItem nextListMenu;
        private System.Windows.Forms.ToolStripMenuItem prevListMenu;
        private System.Windows.Forms.ToolStripMenuItem nextListSelectionMenu;
        private System.Windows.Forms.ToolStripMenuItem previousListSelectionMenu;
        private System.Windows.Forms.ToolStripMenuItem endOfListMenu;
        private System.Windows.Forms.ToolStripMenuItem selectToEOLMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem showResultMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem compileMenu;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutMenu;
        private System.Windows.Forms.ToolStripMenuItem copyMenu;
        private System.Windows.Forms.ToolStripMenuItem pasteMenu;
		private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem findMenu;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
		private System.Windows.Forms.ToolStripMenuItem findOrGoNextMenu;
		private System.Windows.Forms.ToolStripMenuItem undoMenu;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem highlightSelectionMenu;
        private System.Windows.Forms.ToolStripMenuItem showHelptipMenu;
        private System.Windows.Forms.ToolStripMenuItem projectMenu;
        private System.Windows.Forms.ToolStripMenuItem addFileToProjectMenu;
        private System.Windows.Forms.ToolStripMenuItem newProjectMenu;
        private System.Windows.Forms.ToolStripMenuItem closeProjectMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem removeFileFromProjectMenu;
        private System.Windows.Forms.ToolStripMenuItem saveProjectMenu;
        private System.Windows.Forms.SaveFileDialog saveProjectDialog;
        private System.Windows.Forms.ToolStripMenuItem saveProjectAsMenu;
        private System.Windows.Forms.ImageList treeImages;
    }
}

