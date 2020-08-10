using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;
using HaggisInterpreter2;
using System.IO;
using AutocompleteMenuNS;

namespace Haggis_Interpreter
{
    public partial class Form1 : Form
    {
        private const string version = "0.0.1";
        private readonly string[] keywords = { "SET", "DECLEAR", "SEND", "RECEIVE" };
        private readonly string[] types = { "STRING", "INTEGER", "BOOLEAN", "REAL", "CHARACTER" };

        public Form1()
        {
            InitializeComponent();
            LexerSetup();
            AreaSetup();
        }

        private void RemoveCmds(ref Scintilla tb, Keys[] key) { for (int i = 0; i < key.Length; i++) { tb.ClearCmdKey(Keys.Control | key[i]); } }
            
        private void AreaSetup()
        {
            var isoStore = System.IO.IsolatedStorage.IsolatedStorageFile.GetStore(System.IO.IsolatedStorage.IsolatedStorageScope.User | System.IO.IsolatedStorage.IsolatedStorageScope.Assembly, null, null);

            if (isoStore.GetFileNames().Length == 1)
            {
                loadIsolatedFileMenuItem.Enabled = true;
                clearIsolatedFileMenuItem.Enabled = true;
            }

            // Remove hotkeys (Sends 'Unicode' variations of it)
            RemoveCmds(ref HaggisTextBox, new Keys[] { Keys.S, Keys.F, Keys.H});
            HaggisTextBox.ClearCmdKey(Keys.Control | Keys.R);
        }

        private void LexerSetup()
        {
            //HaggisTextBox.Lexer = Lexer.Container;

            HaggisTextBox.Margins[0].Width = 50;

            //HaggisTextBox.SetKeywords(0, "set declear initially send display to as from keyboard");
            //HaggisTextBox.SetKeywords(1, "string real integer character boolean");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Insert logic here
        }

        private void runREPLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Run the current script in the file
            try
            {
                if(HaggisTextBox.Text.Length<1) { MessageBox.Show("You need some code in order to run the REPL/Interpreter!"); return; }

                string[] file = HaggisTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                var win = new RunDialog(file, true);
                win.ShowDialog();

                loadIsolatedFileMenuItem.Enabled = true;
                clearIsolatedFileMenuItem.Enabled = true;
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
                MessageBox.Show("An error occured while attempting to prepare or launch the interpreter");
            }
        }

        private void loadIsolatedFileMenuItem_Click(object sender, EventArgs e)
        {
            var isoStore = System.IO.IsolatedStorage.IsolatedStorageFile.GetStore(System.IO.IsolatedStorage.IsolatedStorageScope.User | System.IO.IsolatedStorage.IsolatedStorageScope.Assembly, null, null);

            if (isoStore.GetFileNames().Length == 1)
            {
                var fileLoc = Path.Combine(isoStore.GetType().GetField("m_RootDir", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(isoStore).ToString(), "runfile.haggis");
                var lines = File.ReadAllLines(fileLoc);
                for (int i = 0; i < lines.Length; i++)
                {
                    HaggisTextBox.AppendText(lines[i]);

                    if (i < lines.Length)
                        HaggisTextBox.AppendText("\n");
                }

                fileLoc = null;
                lines = null;
                GC.Collect();
            }
        }

        private bool Dialog(string title, string message)
        {
            DialogResult dialogResult = MessageBox.Show(message, title, MessageBoxButtons.YesNo);
            return (dialogResult == DialogResult.Yes);
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            if(HaggisTextBox.Text.Length > 0)
            {
                var result = Dialog("Data in REPL", 
                                    "Are you sure you want to exit with text inside the REPL?\nYou'll lose all the data if you press \"Yes\"");
                if (!result)
                    return;
            }

            Application.Exit();
        }

        private void clearIsolatedFileMenuItem_Click(object sender, EventArgs e)
        {
            if (!Dialog("Clear Isolated File", 
                        "Clicking \"Yes\" will remove the last script the interpreter ran...\nYou sure you want to proceed?"))
                return;
            var isoStore = System.IO.IsolatedStorage.IsolatedStorageFile.GetStore(System.IO.IsolatedStorage.IsolatedStorageScope.User | System.IO.IsolatedStorage.IsolatedStorageScope.Assembly, null, null);
            string fileLoc = Path.Combine(isoStore.GetType().GetField("m_RootDir", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(isoStore).ToString(), "runfile.haggis");

            try
            {
                if (File.Exists(fileLoc))
                    File.Delete(fileLoc);

                Console.WriteLine("Isolated file was successfully removed.");
                loadIsolatedFileMenuItem.Enabled = false;
                clearIsolatedFileMenuItem.Enabled = false;
            }
            catch (Exception z)
            {
                Console.WriteLine($"A problem occured while attempting to clear isolated file\n{z.Message}");
            }

        }

        #region AutoComplete Logic
    
        bool keywordStart = false;
        bool requireType = false;

        int keywordCount = 0;
        string lastKeyword;
        string nextAutoWord;

        StringBuilder charBuilder = new StringBuilder();

        private void HaggisTextBox_CharAdded(object sender, CharAddedEventArgs e)
        {
            ScintillaNET.Scintilla editor = sender as ScintillaNET.Scintilla;
            UpdateToolBar();

            char ch = (char)e.Char;
           
            if (char.IsWhiteSpace(ch))
            {
                charBuilder.Clear();
                if (keywordStart && keywordCount == 2) 
                { 
                    keywordStart = false;
                    keywordCount = 0;
                    if (!(nextAutoWord is null))
                    {
                        editor.InsertText(editor.CurrentPosition, $"{nextAutoWord} ");
                        editor.GotoPosition((editor.CurrentPosition + nextAutoWord.Length) + 2);

                        int wordStartPos = editor.WordStartPosition(editor.CurrentPosition, true);

                        //Display the autocompletion list
                        var lenEntered = editor.CurrentPosition - wordStartPos;

                        if (!editor.AutoCActive && requireType)
                            editor.AutoCShow(lenEntered, string.Join(" ", types));

                        nextAutoWord = null;
                        requireType = false;
                    }
                    lastKeyword = null;
                } 
                else { keywordCount++; }
                
                if(!requireType)
                    return; 
            }

            if(ch == '\r' || ch == '\n') {  keywordStart = false; keywordCount = 0; return; }

            if (e.Char < 8 || e.Char >= 14 && e.Char <= 31 || e.Char == 127)
            {
                editor.Text = editor.Text.Replace(ch.ToString(), "").Trim();
            }

            int currentPos = editor.CurrentPosition;
            int currentLine = editor.CurrentLine;

            charBuilder.Append(ch);
            
            string ch_s = charBuilder.ToString();

            if (ch == '"') { editor.InsertText(currentPos, "\""); editor.GotoPosition((currentPos + 1)-1); return; }
            if (ch == '[') { editor.InsertText(currentPos, "]"); editor.GotoPosition((currentPos + 1) - 1); return; }
            if (ch == '(') { editor.InsertText(currentPos, ")"); editor.GotoPosition((currentPos + 1) - 1); return; }

            string currentText = editor.Lines[currentLine].Text;
            if (currentText.Contains("\""))
            {
                int startRange = currentText.IndexOf('"');
                int endRange = currentText.LastIndexOf('"');

                if (currentPos >= startRange && currentPos <= endRange)
                { currentText = null; return; }
            }
            currentText = null;
            if (char.IsUpper(ch) && !keywordStart)
            {
                var availablekeywords = keywords.Where(x => x.StartsWith(ch_s));
                int wordStartPos = editor.WordStartPosition(currentPos, true);

                //Display the autocompletion list
                var lenEntered = currentPos - wordStartPos;
                if (lenEntered > 0)
                {
                    if (!editor.AutoCActive)
                        editor.AutoCShow(lenEntered, string.Join(" ", availablekeywords));
                }            
            }
        }

        private void ResetAuto()
        {
            keywordStart = false;       
            requireType = false;

            nextAutoWord = null;
            lastKeyword = null;

            keywordCount = 0;

            charBuilder.Clear();
            
            UpdateToolBar();
            Console.WriteLine("Reset successfull (Empty line or condition meet to clear the data)");
        }

        private void HaggisTextBox_BeforeDelete(object sender, BeforeModificationEventArgs e)
        {
            var ht = sender as ScintillaNET.Scintilla;

            if (ht.Lines[ht.CurrentLine].Text.Length < 1 || charBuilder.Length < 1) { ResetAuto(); return; }

            var text = charBuilder.ToString();

            if (charBuilder.Length > 0)
            { 
                int textStart = text.IndexOf(e.Text);
                int textLength = e.Text.Length;

                if (textStart == -1) { ResetAuto(); return; }

                charBuilder.Remove(textStart, textLength);
            }

            UpdateToolBar();
        }

        private void HaggisTextBox_AutoCCompleted(object sender, AutoCSelectionEventArgs e)
        {
            ScintillaNET.Scintilla editor = sender as ScintillaNET.Scintilla;
            
            if( e.Text == "INITIALLY" )
            { editor.InsertText(editor.CurrentPosition, " "); editor.GotoPosition(editor.CurrentPosition + 2); keywordStart = false; return; }

            keywordStart = true; keywordCount = 1; lastKeyword = e.Text;

            if ( lastKeyword == "SET" ) { Console.WriteLine("TO auto-place is in play"); nextAutoWord = "TO"; }

            if ( lastKeyword == "DECLEAR" ) { Console.WriteLine("AS auto-place is in play"); nextAutoWord = "AS"; requireType = true; }

            if(e.Text == "SEND")
            {
                editor.InsertText(editor.CurrentPosition, " ");
                int nicespot = editor.CurrentPosition;

                editor.GotoPosition(nicespot + 1); editor.InsertText(editor.CurrentPosition, "\"\" TO DISPLAY"); editor.GotoPosition(nicespot + 2);
            }

            if (e.Text == "RECEIVE")
            {
                //editor.InsertText(editor.CurrentPosition, " ");
                int nicespot = editor.CurrentPosition;

                //editor.GotoPosition(nicespot);  
                editor.InsertText(editor.CurrentPosition, "  FROM () KEYBOARD"); editor.GotoPosition(nicespot + 1);
                nextAutoWord = "receive-datatype";
            }

            if ( types.Contains(e.Text) )
            {
                if (!editor.AutoCActive && editor.Lines[editor.CurrentLine].Text.Contains("DECLEAR")) { editor.InsertText(editor.CurrentPosition, " "); editor.GotoPosition(editor.CurrentPosition + 2); editor.AutoCShow(0, "INITIALLY"); }
            }

            UpdateToolBar();
        }
        
        private void HaggisTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            ScintillaNET.Scintilla editor = sender as ScintillaNET.Scintilla;
            if (e.KeyData == Keys.Tab && nextAutoWord == "receive-datatype")
            {
                e.SuppressKeyPress = true; // Prevents the actual input of a tab space (\t or 4 empty gaps)
                nextAutoWord = null;

                int l_idx = editor.Lines[editor.CurrentLine].Position;
                Console.WriteLine(l_idx);
                // Auto move to allow use to input data type when pressing "tab"
                string text = editor.Lines[editor.CurrentLine].Text;
                int idx = text.IndexOf("()");
                int new_loc = (editor.CurrentPosition + (idx - editor.CurrentPosition)) + 1;

                editor.GotoPosition(l_idx + new_loc); // Move to the correct position
                
                editor.AutoCShow(0, string.Join(" ", types));
            }

            if(e.KeyData == (Keys.Tab | Keys.Control))
            {
                editor.GotoPosition(editor.Lines[editor.CurrentLine].Length);
                editor.AppendText("\n");
                e.SuppressKeyPress = true; // Prevents the actual input
                editor.GotoPosition(editor.Lines[editor.CurrentLine+1].Position);
                ResetAuto();
            }

            if(e.KeyData == (Keys.Control | Keys.R))
            {
                // Prevent input
                e.SuppressKeyPress = true;

                // Lazy press the toolbar
                runREPLToolStripMenuItem.PerformClick();

                ResetAuto();
            }
        }

        #endregion

        private void interpreterMenuStrip_ButtonClick(object sender, EventArgs e)
        {
            // Lazy why to run it, code is already written. Fight me.
            runREPLToolStripMenuItem.PerformClick();
        }

        private void UpdateToolBar()
        {
            int caretPos, anchorPos;
            caretPos = HaggisTextBox.CurrentPosition; anchorPos = HaggisTextBox.AnchorPosition;
            caretPosition.Text = $"Ch: {caretPos}  Sel: {Math.Abs(anchorPos - caretPos)}";
        }

        private void HaggisTextBox_UpdateUI(object sender, UpdateUIEventArgs e)
        {
            UpdateToolBar();
        }
    }
}
