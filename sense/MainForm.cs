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
using Microsoft.VisualBasic;
using Shiro.Support;

namespace Shiro.Sense
{
    public partial class MainForm : Form
    {
        internal static MainForm CurrentForm;

        #region Thread-safe UI Delegate Wrappers

        private delegate void NoParamDelegate();
        private delegate void OneStringParamDelegate(string text);

        public void Eval(string code, Action<Token> cb)
        {
            if (!editorTabs.SelectedTab.Text.TrimStart('*').Trim().StartsWith("new") && !IsProjectOpen)
                Directory.SetCurrentDirectory(Path.GetDirectoryName(DocumentManager.GetFileName(editorTabs.SelectedTab.Text.TrimStart('*').Trim())));

            var ts = new ThreadStart(() =>
            {
                Token res;
                var start = DateTime.Now;
                try
                {
                    lock (ShiroLock)
                    {
                        res = Shiro.Eval(code);
                        if (_showResult)
                            SafeWrite($"[Result]  {res.ToString()}{Environment.NewLine}");

                        if (_showRuntime)
                            SafeWrite($"[Run Duration]  {DateTime.Now.Subtract(start).Milliseconds} ms");
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

            editor.AutoCTypeSeparator = '|';

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

            editor.MouseDwellTime = 750;
            editor.Styles[Style.CallTip].SizeF = 10.25f;
            editor.Styles[Style.CallTip].ForeColor = Color.BlueViolet;
            editor.Styles[Style.CallTip].BackColor = Color.AntiqueWhite;
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
                else
                {
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

        //Highlight double-clicked word
        private void editor_DoubleClick(object sender, DoubleClickEventArgs e)
        {
            var text = editor.SelectedText;
            if (!string.IsNullOrEmpty(text))
                HighlightWord(text);
        }
        private void editor_Click(object sender, EventArgs e)
        {
            ClearHighlights();
        }

        private void editor_KeyPress(object sender, KeyEventArgs e)
        {
            ClearHighlights();
        }

        //Hover tips
        private void editor_DwellStart(object sender, DwellEventArgs e)
        {
            if (!editor.WordChars.Contains("-"))
                editor.WordChars += "?-+=></*.";

            var pos = e.Position;
            var wordStart = editor.WordStartPosition(pos, false);
            var wordEnd = editor.WordEndPosition(pos, false);
            var word = editor.GetTextRange(wordStart, wordEnd - wordStart).Trim().TrimStart('(');

            string tip = "";
            if (!string.IsNullOrEmpty(word) && !string.IsNullOrEmpty(tip = Helptips.GetFor(word)))
                editor.CallTipShow(e.Position, tip);
        }

        private void editor_DwellEnd(object sender, DwellEventArgs e)
        {
            editor.CallTipCancel();
        }

        //Helper to highlight words
        const int HIGHLIGHT_INDICATOR = 8;
        private void ClearHighlights()
        {
            editor.IndicatorCurrent = HIGHLIGHT_INDICATOR;
            editor.IndicatorClearRange(0, editor.TextLength);
        }

        private void HighlightWord(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            ClearHighlights();

            // Update indicator appearance
            editor.Indicators[HIGHLIGHT_INDICATOR].Style = IndicatorStyle.StraightBox;
            editor.Indicators[HIGHLIGHT_INDICATOR].Under = true;
            editor.Indicators[HIGHLIGHT_INDICATOR].ForeColor = Color.Aqua;
            editor.Indicators[HIGHLIGHT_INDICATOR].OutlineAlpha = 90;
            editor.Indicators[HIGHLIGHT_INDICATOR].Alpha = 70;

            // Search the document
            editor.TargetStart = 0;
            editor.TargetEnd = editor.TextLength;
            editor.SearchFlags = SearchFlags.None;
            while (editor.SearchInTarget(text) != -1)
            {
                // Mark the search results with the current indicator
                editor.IndicatorFillRange(editor.TargetStart, editor.TargetEnd - editor.TargetStart);

                // Search the remainder of the document
                editor.TargetStart = editor.TargetEnd;
                editor.TargetEnd = editor.TextLength;
            }
        }

        #endregion

        #region Code Navigation and Autocoding Stuff

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

        private void quickAutoLetMenu_Click(object sender, EventArgs e)
        {
            var startPos = editor.CurrentPosition - 1;
            for (var i = startPos; i >= 0; i--)
                if (editor.Text[i] == '(')
                {
                    editor.CurrentPosition = editor.SelectionStart = i + 1;
                    editor.InsertText(editor.CurrentPosition, "[]");
                    editor.CurrentPosition = editor.SelectionStart = editor.CurrentPosition + 1;
                    break;
                }
        }

        private void prevListMenu_Click(object sender, EventArgs e)
        {
            var startPos = editor.CurrentPosition - 1;
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
            for (var i = startPos; i < editor.Text.Length; i++)
                if (editor.Text[i] == '(')
                {
                    if (ModifierKeys == (Keys.Shift | Keys.Control))
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

                if (c == ')')
                {
                    if (depth == 0)
                    {
                        if (ModifierKeys == (Keys.Shift | Keys.Control))
                            editor.CurrentPosition = i + 1;
                        else
                            editor.CurrentPosition = editor.SelectionStart = i + 1;
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

        private void tree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var node = e.Node;
            var tag = node?.Tag?.ToString();
            if (!string.IsNullOrEmpty(tag))
            {
                OpenFile(tag);
            }
        }

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
                    DocumentManager.UpdateSavedContent(tabName, editor.Text);

                    saveStateTimer.Enabled = true;
                }
            }
            else
            {
                File.WriteAllText(DocumentManager.GetFileName(name), editor.Text);
                DocumentManager.UpdateSavedContent(name, editor.Text);

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
            DocumentManager.UpdateContent(editorTabs.SelectedTab.Text, editor.Text);

            for (var i=0; i<editorTabs.TabPages.Count; i++)
            {
                var tab = editorTabs.TabPages[i];
                var name = tab.Text.TrimStart('*').Trim();

                if(DocumentManager.GetSavedContent(name) != DocumentManager.GetDocumentContentCurrent(name))
                {
                    if (!tab.Text.StartsWith("*"))
                        tab.Text = $"* {tab.Text}";
                } else
                {
                    if (tab.Text.StartsWith("*"))
                        tab.Text = name;
                }
            }
        }

        #endregion

        #region Project Tree Drag and Drop and Misc Helpers

        private static bool ContainsNode(TreeNode node1, TreeNode node2)
        {
            if (node2.Parent == null) return false;
            if (node2.Parent.Equals(node1)) return true;

            return ContainsNode(node1, node2.Parent);
        }

        private void tree_DragDrop(object sender, DragEventArgs e)
        {
            Point targetPoint = tree.PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = tree.GetNodeAt(targetPoint);
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (draggedNode.Equals(targetNode))
                return;

            if (targetNode == null)
            {
                //Drag to top level (out of folder)
                draggedNode.Remove();
                tree.Nodes.Add(draggedNode);
            }
            else if (!ContainsNode(draggedNode, targetNode))
            {
                if (targetNode.Tag == null)
                {
                    //Dragging into a folder, everything is normal
                    draggedNode.Remove();
                    targetNode.Nodes.Add(draggedNode);
                    targetNode.Expand();
                }
                else
                {
                    //Dragged on top of an item -- drop into parent
                    if (targetNode.Parent == null)
                    {
                        //Top level
                        draggedNode.Remove();
                        tree.Nodes.Add(draggedNode);
                    }
                    else
                    {
                        //Parented Item
                        draggedNode.Remove();
                        targetNode.Parent.Nodes.Add(draggedNode);
                        targetNode.Expand();
                    }
                }
            }
            ProjectTree = ShiroProject.ExtractProjectTreeFromTreeViewOpenParenLOLCloseParen(Shiro, tree);
        }

        private void tree_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void tree_DragOver(object sender, DragEventArgs e)
        {
            Point targetPoint = tree.PointToClient(new Point(e.X, e.Y));
            tree.SelectedNode = tree.GetNodeAt(targetPoint);
        }

        private void tree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void addFolderMenu_Click(object sender, EventArgs e)
        {
            var parent = tree.SelectedNode;
            var name = Interaction.InputBox("Enter name of the folder you'd like to add", "Add Folder");
            if (string.IsNullOrEmpty(name?.Trim()))
                return;

            if (parent != null && parent.Tag != null)
                parent = parent.Parent;

            if (parent == null)
            {
                tree.Nodes.Add(new TreeNode(name)
                {
                    SelectedImageIndex = 1,
                    ImageIndex = 1
                });
            }
            else
            {
                parent.Nodes.Add(new TreeNode(name)
                {
                    SelectedImageIndex = 1,
                    ImageIndex = 1
                });
            }

            ProjectTree = ShiroProject.ExtractProjectTreeFromTreeViewOpenParenLOLCloseParen(Shiro, tree);
        }
        #endregion

        #region Simple Menu Handlers

        private void showResultMenu_Click(object sender, EventArgs e)
        {
            _showResult = showResultMenu.Checked;
        }
        private void showRunTimeMenu_Click(object sender, EventArgs e)
        {
            _showRuntime = showRunTimeMenu.Checked;
        }

        private void compileMenu_Click(object sender, EventArgs e)
        {
            //Tear down and setup the bin directory
            if (!IsProjectOpen)
                if (editorTabs.SelectedTab.Text.TrimStart('*').Trim().StartsWith("new"))
                {
                    MessageBox.Show("Please save this file before trying to compile it");
                    return;
                }
                else
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
                var c = new Compiler(editorTabs.SelectedTab.Text.TrimStart('*').Trim());
                c.AddShiroModule(editorTabs.SelectedTab.Text.TrimStart('*').Trim(), editor.Text);
                c.Compile(editorTabs.SelectedTab.Text.Split('.')[0] + ".exe", path, out ce);
            }
            else
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

        private void removeFileFromProjectMenu_Click(object sender, EventArgs e)
        {
            var node = tree.SelectedNode;
            if (node == null)
                return;

            if (string.IsNullOrEmpty(node.Tag?.ToString()))
                MessageBox.Show("I can't delete directories yet, Dan is lazy.");
            else if(DialogResult.Yes == MessageBox.Show($"Delete {node.Text} from the project (file will remain on disk)?", "Remove from Project?", MessageBoxButtons.YesNo))
            {
                ProjectTree.Children.GetProperty(Shiro, "proj").Children[0] = new Token(ShiroProject.DeleteFileFromTree(Shiro, node.Tag.ToString(), ProjectTree.Children.GetProperty(Shiro, "proj").Children[0]).ToArray());
                UpdateTree();
            }
        }

        private void closeProjectMenu_Click(object sender, EventArgs e)
        {
            if (!IsProjectOpen)
                return;

            if (DialogResult.Yes == MessageBox.Show("Save Project before closing?", "Last ditch save?", MessageBoxButtons.YesNo))
                SaveProject();

            IsProjectOpen = false;
            ProjectTree = null;
            ShiroProject.CurrentlyOpenProject = null;
            UpdateTree();
        }

        private void newProjectMenu_Click(object sender, EventArgs e)
        {
            if(IsProjectOpen)
                if (DialogResult.Yes == MessageBox.Show("There's already a project open, do you want to switch?", "Close Current Project?", MessageBoxButtons.YesNo))
                {
                    closeProjectMenu_Click(sender, e);
                    if (IsProjectOpen)
                        return;
                }
                else
                    return;

            string name = Interaction.InputBox("Enter a name for your project (this should be the file name of the project, but doesn't have to be)", "New Project", "", -1, -1);
            if(!string.IsNullOrWhiteSpace(name))
            {
                ShiroProject.CurrentlyOpenProject = null;
                ProjectTree = new Token(new Token("name", name), Token.NamedEmptyList("proj"));
                ProjectTree.Children[1].Children.Add(Token.EmptyList);
                IsProjectOpen = true;
                UpdateTree();
            }
        }

        private void saveProjectMenu_Click(object sender, EventArgs e)
        {
            if (!IsProjectOpen)
                if (DialogResult.Yes == MessageBox.Show("You haven't actually made a project yet.  Would you like to?", "Create a new project?", MessageBoxButtons.YesNo))
                {
                    newProjectMenu_Click(sender, e);
                    if (!IsProjectOpen)
                        return;
                }
                else
                    return;

            SaveProject();
        }

        private void saveProjectAsMenu_Click(object sender, EventArgs e)
        {
            if (!IsProjectOpen)
                if (DialogResult.Yes == MessageBox.Show("You haven't actually made a project yet.  Would you like to?", "Create a new project?", MessageBoxButtons.YesNo))
                {
                    newProjectMenu_Click(sender, e);
                    if (!IsProjectOpen)
                        return;
                }
                else
                    return;

            ShiroProject.CurrentlyOpenProject = null;
            SaveProject();
        }

        private void addFileToProjectMenu_Click(object sender, EventArgs e)
        {
            if (!IsProjectOpen)
                if (DialogResult.Yes == MessageBox.Show("No Project currently open, create a new one?", "New Project?", MessageBoxButtons.YesNo))
                {
                    newProjectMenu_Click(sender, e);
                    if (!IsProjectOpen)
                        return;
                }
                else
                    return;

            if (DialogResult.OK == openFileDialog.ShowDialog())
            {
                var file = openFileDialog.FileName;
                AddFileToProject(Path.GetFileName(file), file);
            }
        }

        private void evaluateMenu_Click(object sender, EventArgs e)
        {
            if (evalStatusLabel.Text == "Evaluating...")
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
                ShiroLexer.Shiro = Helptips.Shiro = Shiro;
                Shiro.CleanUpQueues();
                Shiro.RegisterAutoFunction("cls", (i, t) =>
                {
                    SafeClear();
                    return Token.Nil;
                }, "(cls)");
                Shiro.RegisterAutoFunction("input", (i, t) =>
                {
                    Input = "";
                    Inputting = true;

                    SafeShowInput();

                    while (Inputting)
                        Thread.Sleep(100);

                    console.WriteInput(Input, Color.Green, false);

                    return new Token(Input);
                }, "(input)  ; blocking operation");
                Shiro.RegisterAutoFunction("exit", (i, t) =>
                {
                    Application.Exit();
                    return Token.Nil;
                }, "(exit)");
            }
        }

        private void showHelptipMenu_Click(object sender, EventArgs e)
        {
            var startPos = editor.CurrentPosition - 1;
            var depth = 0;
            for (var i = startPos; i >= 0; i--)
                if (editor.Text[i] == '(')
                {
                    if (depth != 0)
                        depth -= 1;
                    else
                    {
                        if (!editor.WordChars.Contains("-"))
                            editor.WordChars += "?-+=></*.";

                        var word = editor.GetWordFromPosition(i + 1);
                        string tip = "";
                        if (!string.IsNullOrEmpty(word) && !string.IsNullOrEmpty(tip = Helptips.GetFor(word)))
                            editor.CallTipShow(editor.CurrentPosition, tip);

                        return;
                    }
                }
                else if (editor.Text[i] == ')')
                    depth += 1;
        }

        private string _activeFindThing = null;
        private void findMenu_Click(object sender, EventArgs e)
        {
            string text = Interaction.InputBox("What would you like to find?", "Find", editor.SelectedText ?? "", -1, -1);
            if (!string.IsNullOrEmpty(text))
            {
                HighlightWord(text);

                editor.TargetStart = 0;
                editor.TargetEnd = editor.TextLength;
                editor.SearchFlags = SearchFlags.None;

                if (editor.SearchInTarget(text) != -1)
                {
                    _activeFindThing = text;
                    editor.GotoPosition(editor.TargetStart);
                }
            }
        }
        private void findOrGoNextMenu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_activeFindThing))
                findMenu_Click(sender, e);
            else
            {
                HighlightWord(_activeFindThing);

                editor.TargetStart = editor.CurrentPosition + 1;
                editor.TargetEnd = editor.TextLength;
                editor.SearchFlags = SearchFlags.None;

                if (editor.SearchInTarget(_activeFindThing) != -1)
                    editor.GotoPosition(editor.TargetStart);
                else
                    _activeFindThing = null;
            }
        }

        private void highlightSelectionMenu_Click(object sender, EventArgs e)
        {
            var word = editor.SelectedText;
            if (string.IsNullOrWhiteSpace(word))
                return;
            HighlightWord(word);
        }

        private void undoMenu_Click(object sender, EventArgs e)
        {
            editor.Undo();
        }

        private void cutMenu_Click(object sender, EventArgs e)
        {
            editor.Cut();
        }

        private void copyMenu_Click(object sender, EventArgs e)
        {
            editor.Copy();
        }

        private void pasteMenu_Click(object sender, EventArgs e)
        {
            editor.Paste();
        }
        #endregion

        #region Misc Event Handlers (non-menu)

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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void bottomTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (bottomTabs.SelectedTab.Text == "Terminal" && !terminal.IsProcessRunning)
                terminal.StartProcess("cmd.exe", null);
        }
        #endregion

        #region Helpers

        private static TreeNode[] CreateProjectNode(string rootName, Token projectToken)
        {
            var retVal = new List<TreeNode>();
            if(projectToken.Children.Count > 0)
                foreach (var child in projectToken.Children[0].Children)
                {
                    if (child.Children.HasProperty(Shiro, "path"))
                    {
                        //It's a file
                        var node = new TreeNode(child.Children.GetProperty(Shiro, "name").ToString());
                        node.SelectedImageIndex = node.ImageIndex = 2;
                        node.Tag = child.Children.GetProperty(Shiro, "path").ToString();
                        retVal.Add(node);
                    }
                    else
                    {
                        //It's a folder
                        var name = child.Children.GetProperty(Shiro, "name").ToString();
                        var directoryNode = new TreeNode(name);
                        directoryNode.SelectedImageIndex = directoryNode.ImageIndex = 1;
                        directoryNode.Nodes.AddRange(CreateProjectNode(name, child.Children.GetProperty(Shiro, "files")));
                        retVal.Add(directoryNode);
                    }
                }
            return retVal.ToArray();
        }

        private void SaveProject()
        {
            if(string.IsNullOrEmpty(ShiroProject.CurrentlyOpenProject))
            {
                if (DialogResult.OK == saveProjectDialog.ShowDialog())
                    ShiroProject.CurrentlyOpenProject = saveProjectDialog.FileName;
                else
                    return;
            }

            File.WriteAllText(ShiroProject.CurrentlyOpenProject, ShiroProject.GetShiroProjectFileContent(Shiro, ProjectTree));
        }

        private void OpenProject(string file)
        {
            ShiroProject.ProjectFileDirectory = Path.GetDirectoryName(file);
            ShiroProject.CurrentlyOpenProject = file;
            Directory.SetCurrentDirectory(ShiroProject.ProjectFileDirectory);

            var content = File.ReadAllText(file);
            var projectTree = Shiro.Eval(content);

            if (!projectTree.Children.HasProperty(Shiro, "name") || !projectTree.Children.HasProperty(Shiro, "proj"))
                MessageBox.Show("Invalid project file, missing name or project structure");
            else
            {
                ProjectTree = projectTree;
                IsProjectOpen = true;
                UpdateTree();
            }
        }
        internal void OpenFile(string file)
        {
            if(!File.Exists(file))
            {
                MessageBox.Show("File '" + file + "' not found.  Sad Trombone.");
                return;
            }

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

        protected void AddFileToProject(string name, string path)
        {
            var proj = ProjectTree.Children.GetProperty(Shiro, "proj");
            proj.Children[0].Children.Add(new Token(new Token("name", name), new Token("path", path)));
            ProjectTree.Children.SetProperty(Shiro, "proj", proj);

            UpdateTree();
        }

        protected void UpdateTree()
        {
            tree.Nodes.Clear();
            if(IsProjectOpen)
                tree.Nodes.AddRange(CreateProjectNode(ProjectTree.Children.GetProperty(Shiro, "name").ToString(), ProjectTree.Children.GetProperty(Shiro, "proj")));
        }

        #endregion

        protected bool IsProjectOpen = false;
        protected Token ProjectTree;

        public static Interpreter Shiro;
        private static object ShiroLock = new object();

        private ShiroLexer Lexer = new ShiroLexer("static relet and or new atom undef error? switch queue? pub sub awaiting? enclose gv awaith hermeticAwait await len tcp impl implementer mixin impl? quack? try catch throw .c .call interpolate import do if json jsonv dejson pair print printnb pnb quote string str def set sod eval skw concat v . .? + - * / = ! != > < <= >= list? obj? num? str? def? fn? nil? let nop defn filter map apply kw params nth range while contains upper lower split fn => .s .set .d .def .sod telnet send sendTo sendAll stop http content route status rest ");
        private bool Inputting = false;
        private string Input = "";

        protected bool _showResult = false, _showRuntime = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Text += " interpreter version " + Interpreter.Version;
            CurrentForm = this;
            DocumentManager.Editor = editor;
            Interpreter.Output = s =>
            {
                SafeWrite(s);
            };
            Interpreter.LoadModule = (m, s) => {
                if (s.ToLower() == "shiro-project")
                {
                    ShiroProject.Libs = new List<string>();

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


        private void registerWindowsExplorerContextMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.RegistryKey key = null;
            try
            {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software");

                key = key.OpenSubKey("Classes");
                key = key.OpenSubKey("directory");
                key = key.OpenSubKey("shell");

                key = key.CreateSubKey("SENSE");
                key.SetValue("", "Open with SENSE");
                key.SetValue("command", Assembly.GetExecutingAssembly().Location);
                key.SetValue("icon", Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "shiro.ico"));
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You have to run SENSE as Administrator to register this");
            } finally
            {
                key.Close();
            }
        }
    }
}
