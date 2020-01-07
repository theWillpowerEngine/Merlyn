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
using Shiro.Build;

namespace ShIDE
{
    public partial class MainForm : Form
    {
        internal static MainForm CurrentForm;

        protected bool IsProjectOpen = false;
        protected Token ProjectTree;

        public static Interpreter Shiro;
        private static object ShiroLock = new object();

        public MainForm()
        {
            InitializeComponent();
        }

        private ShiroLexer Lexer = new ShiroLexer("atom undef error? switch queue? pub sub awaiting? enclose gv awaith hermeticAwait await len tcp impl implementer mixin impl? quack? try catch throw .c .call interpolate import do if json jsonv dejson pair print printnb pnb quote string str def set sod eval skw concat v . .? + - * / = ! != > < <= >= list? obj? num? str? def? fn? nil? let nop defn filter map apply kw params nth range while contains upper lower split fn => .s .set .d .def .sod telnet send sendTo sendAll stop http content route status rest ");
        private bool Inputting = false;
        private string Input = "";

        #region Thread-safe UI Delegate Wrappers

        private delegate void NoParamDelegate();
        private delegate void OneStringParamDelegate(string text);

        public void Eval(string code, Action<Token> cb)
        {
            if(!editorTabs.SelectedTab.Text.StartsWith("new"))
                Directory.SetCurrentDirectory(Path.GetDirectoryName(DocumentManager.GetFileName(editorTabs.SelectedTab.Text)));

            var ts = new ThreadStart(() =>
                {
                    Token res;
                    try
                    {
                        lock (ShiroLock)
                        {
                            res = Shiro.Eval(code);
                            if (_showResult)
                                SafeWrite($"[Result]  {res.ToString()}{Environment.NewLine}");
                        }
                        cb(res);
                    }
                    catch (Exception ex)
                    {
                        SafeError(ex.Message);
                        cb(null);
                    }

                });

            new Thread(ts).Start();
        }

        private void SafeShowInput()
        {
            if (txtInput.InvokeRequired)
            {
                var d = new NoParamDelegate(SafeShowInput);
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
                var d = new NoParamDelegate(SafeClear);
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
                var d = new OneStringParamDelegate(SafeWrite);
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
                var d = new OneStringParamDelegate(SafeError);
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
                var d = new NoParamDelegate(SafeStyleEditor);
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

            editor.SetSelectionBackColor(true, Color.Ivory);
            editor.SetSelectionForeColor(true, Color.FromArgb(25, 25, 25));

            editor.StyleClearAll();

            editor.Styles[Style.BraceLight].BackColor = Color.BlueViolet;
            editor.Styles[Style.BraceLight].ForeColor = Color.LightGray;
            editor.Styles[Style.BraceBad].ForeColor = Color.Yellow;
            editor.Styles[Style.BraceBad].BackColor = Color.Red;

            editor.Styles[ShiroLexer.StyleKeyword].ForeColor = Color.CornflowerBlue;
            editor.Styles[ShiroLexer.StyleString].ForeColor = Color.Cyan;
            editor.Styles[ShiroLexer.StyleNumber].ForeColor = Color.GreenYellow;
            editor.Styles[ShiroLexer.StyleComment].ForeColor = Color.MediumOrchid;
            editor.Styles[ShiroLexer.StyleFunction].ForeColor = Color.LightSteelBlue;
            editor.Styles[ShiroLexer.StyleVariable].ForeColor = Color.OrangeRed;
        }

        //Code Styling
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

        //Brace Matching
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

        //Auto-indent
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

        //Autocomplete trigger
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

        #region Code Navigation and Autocoding Stuff

        private void prevListMenu_Click(object sender, EventArgs e)
        {
            var startPos = editor.CurrentPosition -1;
            for (var i = startPos; i >= 0; i--)
                if (editor.Text[i] == '(')
                {
                    if (ModifierKeys == (Keys.Shift | Keys.Control))
                        editor.CurrentPosition = i;
                    else
                        editor.CurrentPosition = editor.SelectionStart = i;
                    break;
                }
        }

        private void nextListMenu_Click(object sender, EventArgs e)
        {
            var startPos = editor.CurrentPosition + 1;
            for(var i=startPos; i<editor.Text.Length; i++)
                if(editor.Text[i] == '(')
                {
                    if(ModifierKeys == (Keys.Shift | Keys.Control))
                        editor.CurrentPosition = i;
                    else
                        editor.CurrentPosition = editor.SelectionStart = i;
                    break;
                }
        }

        private void endOfListMenu_Click(object sender, EventArgs e)
        {
            int depth = 0;
            var startPos = editor.CurrentPosition + 1;
            for (var i = startPos; i < editor.Text.Length; i++)
            {
                var c = editor.Text[i];
                if (c == '(')
                    depth += 1;

                if(c == ')')
                {
                    if(depth == 0)
                    {
                        if (ModifierKeys == (Keys.Shift | Keys.Control))
                            editor.CurrentPosition = i+1;
                        else
                            editor.CurrentPosition = editor.SelectionStart = i+1;
                        break;
                    }
                    else
                        depth -= 1;
                }
            }
        }

        #endregion

        #region Document Management (editor tabs, save/load/new, etc.)

        private void editorTabs_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < editorTabs.TabCount; i++)
                {
                    Rectangle r = editorTabs.GetTabRect(i);
                    if (r.Contains(e.Location))
                    {
                        DocumentManager.ReleaseDocument(editorTabs.TabPages[i].Text);
                        editorTabs.TabPages.Remove(editorTabs.TabPages[i]);
                        break;
                    }
                }
            }
        }

        private static TreeNode CreateProjectNode(string rootName, Token projectToken)
        {
            var directoryNode = new TreeNode(rootName);

            foreach(var child in projectToken.Children[0].Children)
            {
                if(child.Children.HasProperty(Shiro, "path"))
                {
                    //It's a file
                    var node = new TreeNode(child.Children.GetProperty(Shiro, "name").ToString());
                    node.Tag = child.Children.GetProperty(Shiro, "path").ToString();
                    directoryNode.Nodes.Add(node);
                }
                else
                {
                    //It's a folder
                    var name = child.Children.GetProperty(Shiro, "name").ToString();
                    directoryNode.Nodes.Add(CreateProjectNode(name, child.Children.GetProperty(Shiro, "files")));
                }
            }
            return directoryNode;
        }

        private void OpenProject(string file)
        {
            ShiroProject.ProjectFileDirectory = Path.GetDirectoryName(file);
            Directory.SetCurrentDirectory(ShiroProject.ProjectFileDirectory);

            var content = File.ReadAllText(file);
            var projectTree = Shiro.Eval(content);

            if (!projectTree.Children.HasProperty(Shiro, "name") || !projectTree.Children.HasProperty(Shiro, "proj"))
                MessageBox.Show("Invalid project file, missing name or project structure");
            else
            {
                tree.Nodes.Clear();
                tree.Nodes.Add(CreateProjectNode(projectTree.Children.GetProperty(Shiro, "name").ToString(), projectTree.Children.GetProperty(Shiro, "proj")));
                IsProjectOpen = true;
                ProjectTree = projectTree;
            }
        }
        internal void OpenFile(string file)
        {
            if (file.EndsWith(".shrp"))
                OpenProject(file);
            else
            {
                var content = File.ReadAllText(file);
                var tabName = Path.GetFileName(file);

                editorTabs.TabPages.Add(new TabPage(DocumentManager.AddDocument(tabName, file, content)));
                _suppressTabChanged = true;
                editorTabs.SelectedIndex = editorTabs.TabPages.Count - 1;

                saveStateTimer.Enabled = true;
            }

            editor.Focus();
        }

        private void tree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var node = e.Node;
            var tag = node?.Tag?.ToString();
            if(!string.IsNullOrEmpty(tag))
            {
                OpenFile(tag);
            }
        }

        private string _previousTab = "new";
        private bool _suppressTabChanged = false;
        private void editorTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressTabChanged || editorTabs.Dragging)
            {
                _suppressTabChanged = false;
                return;
            }

            if (editorTabs.SelectedTab == null)
                return;

            var name = editorTabs.SelectedTab.Text;
            DocumentManager.Switch(name);
        }

        private void newMenu_Click(object sender, EventArgs e)
        {
            editorTabs.TabPages.Add(new TabPage(DocumentManager.AddDocument("new")));
            _suppressTabChanged = true;
            editorTabs.SelectedIndex = editorTabs.TabPages.Count - 1;
        }

        private void saveMenu_Click(object sender, EventArgs e)
        {
            var name = editorTabs.SelectedTab.Text;
            if (!DocumentManager.HasFileName(name))
            {
                if (DialogResult.OK == saveFileDialog.ShowDialog())
                {
                    var file = saveFileDialog.FileName;
                    File.WriteAllText(file, editor.Text);

                    var tabName = Path.GetFileName(file);

                    editorTabs.SelectedTab.Text = DocumentManager.Rename(name, tabName, file);

                    saveStateTimer.Enabled = true;
                }
            }
            else
            {
                File.WriteAllText(DocumentManager.GetFileName(name), editor.Text);

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
            CurrentForm = this;
            DocumentManager.Editor = editor;
            Interpreter.Output = s =>
            {
                SafeWrite(s);
            };
            Interpreter.LoadModule = (m, s) => {
                if (s.ToLower() == "shiro-project")
                {
                    if (!Shiro.IsFunctionName("sh-project"))
                        new ShiroProject().RegisterAutoFunctions(Shiro);
                }
                else
                    return Interpreter.DefaultModuleLoader(m, s);

                return true;
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
                DocumentManager.AddDocument("new");
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
            if(evalStatusLabel.Text == "Evaluating...")
            {
                MessageBox.Show("Shiro is already evaluating something (probably a network server), sorry");
                return;
            }

            evalStatusLabel.Text = "Evaluating...";
            Eval(editor.Text, t =>
            {
                evalStatusLabel.Text = "Not Evaluating";
                SafeStyleEditor();
            });
        }

        private void cleanMenu_Click(object sender, EventArgs e)
        {
            if (evalStatusLabel.Text == "Evaluating...")
            {
                MessageBox.Show("Can't clean the interpreter while shiro is evaluating something.  You might have a network server running somewhere");
                return;
            }
            lock (ShiroLock)
            {
                Shiro = new Interpreter();
                ShiroLexer.Shiro = Shiro;
                Shiro.CleanUpQueues();
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

        protected bool _showResult = false;

        private void showResultMenu_Click(object sender, EventArgs e)
        {
            _showResult = showResultMenu.Checked;
        }

        private void compileMenu_Click(object sender, EventArgs e)
        {
            //Tear down and setup the bin directory
            if (!IsProjectOpen)
                if (editorTabs.SelectedTab.Text.StartsWith("new"))
                {
                    MessageBox.Show("Please save this file before trying to compile it");
                    return;
                } else 
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(DocumentManager.GetFileName(editorTabs.SelectedTab.Text)));

            var path = Directory.GetCurrentDirectory() + "\\bin";
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
                MessageBox.Show("Compilation failed -- it's probable that the bin directory or something in it is locked");
                return;
            }

            AssemblyName[] a = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            foreach (AssemblyName an in a)
                if (an.FullName.ToLower().Contains("shiro"))
                {
                    if (File.Exists(path + "\\Shiro.Lang.dll"))
                        File.Delete(path + "\\Shiro.Lang.dll");

                    var shiroPath = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
                    File.Copy(shiroPath + "\\Shiro.Lang.dll", path + "\\Shiro.Lang.dll");
                }

            foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                if (file.EndsWith(".dll"))
                    File.Copy(file, file.Replace(Directory.GetCurrentDirectory(), path));
            }

            //Now do the compile
            System.CodeDom.Compiler.CompilerError ce = null;
            if (!IsProjectOpen)
            {
                //Single file compile, nice and easy
                var c = new Compiler(editorTabs.SelectedTab.Text);
                c.AddShiroModule(editorTabs.SelectedTab.Text, editor.Text);
                c.Compile(editorTabs.SelectedTab.Text.Split('.')[0] + ".exe", path, out ce);
            } else
            {
                //Project compile.  Slightly trickier
                var pt = ProjectTree;

            }

            if (ce == null)
                SafeWrite("Compile success");
            else
            {
                SafeWrite("Compile failed: " + ce.ErrorText);
            }
        }
    }
}
