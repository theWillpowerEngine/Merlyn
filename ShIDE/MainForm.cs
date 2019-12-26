using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ScintillaNET;
using Shiro;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ShIDE
{
    public partial class MainForm : Form
    {
        internal static MainForm ActiveForm;

        public static Interpreter Shiro;
        private static object ShiroLock = new object();

        public MainForm()
        {
            InitializeComponent();
        }

        private ShiroLexer Lexer = new ShiroLexer("impl implementer mixin try catch throw .c .call interpolate import do if json jsonv dejson pair print printnb pnb quote string str def set sod eval skw concat v . .? + - * / = ! != > < <= >= list? obj? num? str? def? fn? nil? let nop qnop defn filter map apply kw params nth range while contains upper lower split fn => .s .set .d .def .sod telnet send sendTo sendAll http content route status rest");
        private bool Inputting = false;
        private string Input = "";

        private Dictionary<string, Document> tabDocuments = new Dictionary<string, Document>();
        private Dictionary<string, string> savedDocuments = new Dictionary<string, string>();
        private Dictionary<string, string> savedDocumentPaths = new Dictionary<string, string>();

        #region Thread-safe UI Delegate Wrappers

        private delegate void ShowInput();
        private delegate void WriteConsole(string text);

        public void Eval(string code, Action<Token> cb)
        {
            var ts = new ThreadStart(() =>
                {
                    Token res;
                    try
                    {
                        lock (ShiroLock)
                        {
                            res = Shiro.Eval(code);
                        }
                        cb(res);
                    }
                    catch (Exception ex)
                    {
                        SafeError(ex.Message);
                    }

                });

            new Thread(ts).Start();
        }

        private void SafeShowInput()
        {
            if (txtInput.InvokeRequired)
            {
                var d = new ShowInput(SafeShowInput);
                txtInput.Invoke(d, new object[] { });
            }
            else
            {
                txtInput.Show();
                txtInput.Focus();
            }
        }
        private void SafeClear()
        {
            if (txtInput.InvokeRequired)
            {
                var d = new ShowInput(SafeClear);
                txtInput.Invoke(d, new object[] { });
            }
            else
            {
                console.ClearOutput();
            }
        }
        private void SafeWrite(string text)
        {
            if (console.InvokeRequired)
            {
                var d = new WriteConsole(SafeWrite);
                console.Invoke(d, new object[] { text });
            }
            else
            {
                console.WriteOutput(text, Color.Ivory);
            }
        }
        private void SafeError(string text)
        {
            if (console.InvokeRequired)
            {
                var d = new WriteConsole(SafeError);
                console.Invoke(d, new object[] { Environment.NewLine + text + Environment.NewLine });
            }
            else
            {
                console.WriteOutput(text, Color.Red);
            }
        }
        private void SafeStyleEditor()
        {
            if (console.InvokeRequired)
            {
                var d = new ShowInput(SafeStyleEditor);
                console.Invoke(d, new object[] { });
            }
            else
            {
                editor_StyleNeeded(null, null);
            }
        }
        #endregion

        #region Scintilla Stuff

        private void SetupScintilla()
        {
            editor.StyleResetDefault();
            editor.Styles[Style.Default].Font = "Consolas";
            editor.Styles[Style.Default].Size = 11;
            editor.Styles[Style.Default].BackColor = Color.FromArgb(25, 25, 25);
            editor.Styles[Style.Default].ForeColor = Color.Ivory;

            editor.StyleClearAll();

            editor.Styles[Style.BraceLight].BackColor = Color.LightGray;
            editor.Styles[Style.BraceLight].ForeColor = Color.BlueViolet;
            editor.Styles[Style.BraceBad].ForeColor = Color.Red;

            editor.Styles[ShiroLexer.StyleKeyword].ForeColor = Color.CornflowerBlue;
            editor.Styles[ShiroLexer.StyleString].ForeColor = Color.Cyan;
            editor.Styles[ShiroLexer.StyleNumber].ForeColor = Color.GreenYellow;
            editor.Styles[ShiroLexer.StyleComment].ForeColor = Color.MediumOrchid;
            editor.Styles[ShiroLexer.StyleFunction].ForeColor = Color.LightSteelBlue;
            editor.Styles[ShiroLexer.StyleVariable].ForeColor = Color.OrangeRed;
        }
        private void editor_StyleNeeded(object sender, StyleNeededEventArgs e)
        {
            //var startPos = editor.GetEndStyled();
            //var endPos = e.Position;

            var startPos = 0;
            var endPos = editor.TextLength;

            Lexer.Style(editor, startPos, endPos);
        }

        int lastCaretPos = 0;
        private static bool IsBrace(int c)
        {
            switch (c)
            {
                case '(':
                case ')':
                case '{':
                case '}':
                    return true;
            }

            return false;
        }
        private void editor_UpdateUI(object sender, UpdateUIEventArgs e)
        {
            var caretPos = editor.CurrentPosition;
            if (lastCaretPos != caretPos)
            {
                lastCaretPos = caretPos;
                var bracePos1 = -1;
                var bracePos2 = -1;

                // Is there a brace to the left or right?
                if (caretPos > 0 && IsBrace(editor.GetCharAt(caretPos - 1)))
                    bracePos1 = (caretPos - 1);
                else if (IsBrace(editor.GetCharAt(caretPos)))
                    bracePos1 = caretPos;

                if (bracePos1 >= 0)
                {
                    // Find the matching brace
                    bracePos2 = editor.BraceMatch(bracePos1);
                    if (bracePos2 == Scintilla.InvalidPosition)
                        editor.BraceBadLight(bracePos1);
                    else
                        editor.BraceHighlight(bracePos1, bracePos2);
                }
                else
                {
                    // Turn off brace matching
                    editor.BraceHighlight(Scintilla.InvalidPosition, Scintilla.InvalidPosition);
                }
            }
        }

        private void editor_InsertCheck(object sender, InsertCheckEventArgs e)
        {
            if ((e.Text.EndsWith("\r") || e.Text.EndsWith("\n")))
            {
                var curLine = editor.LineFromPosition(e.Position);
                var curLineText = editor.Lines[curLine].Text;

                if (curLineText.Trim() == "")
                    e.Text += curLineText.TrimEnd('\r', '\n');
                else {
                    var indent = Regex.Match(curLineText, @"^\s*");
                    e.Text += indent.Value; // Add indent following "\r\n"

                    // Current line end with bracket?
                    if (Regex.IsMatch(curLineText, @"{\s*$"))
                        e.Text += "\t"; // Add tab
                }
            }
        }

        private void editor_CharAdded(object sender, CharAddedEventArgs e)
        {
            if (e.Char == '(')
                if (!editor.AutoCActive)
                    editor.AutoCShow(0, ShiroLexer.GetAutoCompleteItems());
        }

        private void showAutocompleteMenu_Click(object sender, EventArgs e)
        {
            if (editor.AutoCActive)
                return;

            var currentPos = editor.CurrentPosition;
            var wordStartPos = editor.WordStartPosition(currentPos, true);

            var lenEntered = currentPos - wordStartPos;
            if (lenEntered > 0)
            {
                if (!editor.AutoCActive)
                    editor.AutoCShow(lenEntered, ShiroLexer.GetAutoCompleteItems());
            }
        }

        #endregion

        #region Document Management (editor tabs, save/load/new, etc.)

        private void ListDirectory(TreeView treeView, string path)
        {
            treeView.Nodes.Clear();
            var rootDirectoryInfo = new DirectoryInfo(path);
            treeView.Nodes.Add(CreateDirectoryNode(rootDirectoryInfo));
        }

        private static TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeNode(directoryInfo.Name);
            foreach (var directory in directoryInfo.GetDirectories())
                directoryNode.Nodes.Add(CreateDirectoryNode(directory));
            foreach (var file in directoryInfo.GetFiles())
                directoryNode.Nodes.Add(new TreeNode(file.Name));
            return directoryNode;
        }

        private void OpenFolder(string path)
        {
            tree.Nodes.Add(CreateDirectoryNode(new DirectoryInfo(path)));
        }
        internal void OpenFile(string file)
        {
            var content = File.ReadAllText(file);

            var tabName = Path.GetFileName(file);
            savedDocuments.Add(tabName, content);
            savedDocumentPaths.Add(tabName, file);

            if (editorTabs.SelectedTab != null)
            {
                var name = editorTabs.SelectedTab.Text;
                tabDocuments[name] = editor.Document;
                editor.AddRefDocument(tabDocuments[name]);
            }

            editor.Document = Document.Empty;
            editor.Text = content;
            tabDocuments.Add(tabName, editor.Document);

            editorTabs.TabPages.Add(new TabPage(tabName));
            _previousTab = tabName;
            _suppressTabChanged = true;
            editorTabs.SelectedIndex = editorTabs.TabPages.Count - 1;

            saveStateTimer.Enabled = true;
        }

        private void openFolder_Click(object sender, EventArgs e)
        {

        }

        private void tree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var node = e.Node;

        }

        private string _previousTab = "new";
        private bool _suppressTabChanged = false;
        private void editorTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressTabChanged)
            {
                _suppressTabChanged = false;
                return;
            }

            var name = editorTabs.SelectedTab.Text;
            tabDocuments[_previousTab] = editor.Document;
            editor.AddRefDocument(tabDocuments[_previousTab]);

            editor.Document = tabDocuments[name];
            editor.ReleaseDocument(tabDocuments[name]);

            _previousTab = name;
        }

        private void newMenu_Click(object sender, EventArgs e)
        {
            var name = editorTabs.SelectedTab.Text;
            tabDocuments[name] = editor.Document;
            editor.AddRefDocument(tabDocuments[name]);

            editor.Document = Document.Empty;

            var newName = "";
            if (!tabDocuments.ContainsKeyInSomeFashion("new"))
                tabDocuments.Add(newName = "new", editor.Document);
            else
            {
                var idx = 1;
                while (tabDocuments.ContainsKeyInSomeFashion("new " + idx))
                    idx += 1;

                tabDocuments.Add(newName = ("new " + idx), editor.Document);
            }
            editorTabs.TabPages.Add(new TabPage(newName));
            _suppressTabChanged = true;
            editorTabs.SelectedIndex = editorTabs.TabPages.Count - 1;
        }

        private void saveMenu_Click(object sender, EventArgs e)
        {
            var name = editorTabs.SelectedTab.Text;
            if (!savedDocuments.ContainsKey(name))
            {
                if (DialogResult.OK == saveFileDialog.ShowDialog())
                {
                    var file = saveFileDialog.FileName;
                    File.WriteAllText(file, editor.Text);

                    var tabName = Path.GetFileName(file);
                    savedDocuments.Add(tabName, editor.Text);
                    savedDocumentPaths.Add(tabName, file);

                    editorTabs.SelectedTab.Text = tabName;

                    var doc = tabDocuments[name];
                    tabDocuments.Remove(name);
                    tabDocuments.Add(tabName, doc);

                    saveStateTimer.Enabled = true;
                }
            }
            else
            {
                File.WriteAllText(savedDocumentPaths[name], editor.Text);
                savedDocuments[name] = editor.Text;

                saveStateTimer.Enabled = true;
            }
        }


        private void openMenu_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openFileDialog.ShowDialog())
            {
                var file = openFileDialog.FileName;
                OpenFile(file);
            }
        }

        private void saveAsMenu_Click(object sender, EventArgs e)
        {
            MessageBox.Show("I haven't done this yet I'm lazy and it's mildly annoying");
        }

        private void saveStateTimer_Tick(object sender, EventArgs e)
        {
            //TODO:  Implement the little * to tell you what to save.
        }

        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            ActiveForm = this;
            Interpreter.Output = s =>
            {
                SafeWrite(s);
            };

            cleanMenu_Click(null, null);

            txtInput.Hide();

            SetupScintilla();

            SafeWrite("Your output will go here.  Shiro Version:  " + Interpreter.Version + Environment.NewLine + Environment.NewLine);

            Show();

            if (Program.ThingToOpen != null)
                OpenFile(Program.ThingToOpen);
            else
            {
                editorTabs.TabPages.Add("new");
                tabDocuments.Add("new", editor.Document);
            }

            editor.Focus();
        }

        private void txtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                Input = txtInput.Text;
                Inputting = false;
                txtInput.Hide();
                txtInput.Text = "";
            }
        }

        private void evaluateMenu_Click(object sender, EventArgs e)
        {
            evalStatusLabel.Text = "Evaluating...";
            Eval(editor.Text, t =>
            {
                evalStatusLabel.Text = "Not Evaluating";
                SafeStyleEditor();
            });
        }

        private void cleanMenu_Click(object sender, EventArgs e)
        {
            lock (ShiroLock)
            {
                Shiro = new Interpreter();
                ShiroLexer.Shiro = Shiro;
                Shiro.RegisterAutoFunction("cls", (i, t) =>
                {
                    SafeClear();
                    return Token.Nil;
                });
                Shiro.RegisterAutoFunction("input", (i, t) =>
                {
                    Input = "";
                    Inputting = true;

                    SafeShowInput();

                    while (Inputting)
                        Thread.Sleep(100);

                    console.WriteInput(Input, Color.Green, false);

                    return new Token(Input);
                });
                Shiro.RegisterAutoFunction("exit", (i, t) =>
                {
                    Application.Exit();
                    return Token.Nil;
                });
            }
        }

        private void autoDoMenu_Click(object sender, EventArgs e)
        {
            var sbDo = new StringBuilder(@"(do 
");
            var lines = editor.SelectedText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                sbDo.AppendLine("    " + line);
            }
            sbDo.Append(")");

            editor.ReplaceSelection(sbDo.ToString());
            editor.SetEmptySelection(editor.CurrentPosition - 1);
        }

        private void quickParenMenu_Click(object sender, EventArgs e)
        {
            editor.ReplaceSelection(@"(" + editor.SelectedText + ")");
            editor.SetEmptySelection(editor.CurrentPosition - 1);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void bottomTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (bottomTabs.SelectedTab.Text == "Terminal" && !terminal.IsProcessRunning)
                terminal.StartProcess("cmd.exe", null);
        }

        private void registerWindowsExplorerContextMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.RegistryKey key = null;
            try
            {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software");

                key = key.OpenSubKey("Classes");
                key = key.OpenSubKey("directory");
                key = key.OpenSubKey("shell");

                key = key.CreateSubKey("ShIDE");
                key.SetValue("", "Open with ShIDE");
                key.SetValue("command", Assembly.GetExecutingAssembly().Location);
                key.SetValue("icon", Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "shiro.ico"));
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You have to run ShIDE as Administrator to register this");
            } finally
            {
                key.Close();
            }
        }
    }
}
