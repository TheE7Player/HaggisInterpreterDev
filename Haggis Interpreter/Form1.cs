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
        private readonly string[] keywords = { "SET", "DECLEAR", "SEND", "RECEIVE", "INITIALLY", "DISPLAY", "TO", "AS", "FROM", "KEYBOARD", "END", "IF", "ELSE", "OR", "AND", "NOT", "FUNCTION", "PROCEDURE", "WHILE", "REPEAT", "DO", "UNTIL", "RETURN", "THEN",
                                               "STRING", "INTEGER", "BOOLEAN", "REAL", "CHARACTER" };
        private readonly string[] types = { "STRING", "INTEGER", "BOOLEAN", "REAL", "CHARACTER" };

        private string currentFile = "";
        private bool needsSaved = false;

        private HaggisLexer _lex;


        public Form1()
        {
            InitializeComponent();
            LexerSetup();
            AreaSetup();
        }

        #region General/Other

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
            RemoveCmds(ref HaggisTextBox, new Keys[] { Keys.S, Keys.F, Keys.H, Keys.R, Keys.N, Keys.O});
            //HaggisTextBox.ClearCmdKey(Keys.Control | Keys.R);

            interpreterLocationLink.Text = $"Locate Current Interpreter ({Properties.Settings.Default.currentInterpreterVersion})";
        }

        private void LexerSetup()
        {
            _lex = new HaggisLexer(string.Join(" ", keywords));

            HaggisTextBox.Margins[0].Width = 50;

            HaggisTextBox.Styles[HaggisLexer.StyleDefault].ForeColor = Color.Black;

            // VS19 Keyword Colour
            HaggisTextBox.Styles[HaggisLexer.StyleKeyword].ForeColor = Color.FromArgb(46, 123, 186);

            HaggisTextBox.Styles[HaggisLexer.StyleIdentifier].ForeColor = Color.Teal;
            HaggisTextBox.Styles[HaggisLexer.StyleNumber].ForeColor = Color.Purple;

            // VS19 String Quote Colour -> " "
            HaggisTextBox.Styles[HaggisLexer.StyleString].ForeColor = Color.FromArgb(206,139,111);
            HaggisTextBox.Styles[HaggisLexer.StyleComment].ForeColor = Color.Green;

            HaggisTextBox.Lexer = Lexer.Container;
        }

        private void HaggisTextBox_StyleNeeded(object sender, StyleNeededEventArgs e)
        {
            var startPos = HaggisTextBox.GetEndStyled();
            var endPos = e.Position;

            _lex.Style(HaggisTextBox, startPos, endPos);
        }

        private double fixVersion(string version)
        {
            var c_arr = version.ToCharArray().ToList();
            int lastDot = version.LastIndexOf('.');

            c_arr.RemoveAt(lastDot);

            var num = Convert.ToDouble(string.Join("", c_arr));

            return num;
        }

        public static Task<string> GetInterpreterVersion(string pathLoc)
        {       
            string info = string.Empty;
            var process = new System.Diagnostics.Process();
            var tcs = new TaskCompletionSource<string>();
            try
            {
              
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                process.StartInfo.FileName = pathLoc;
                process.StartInfo.Arguments = "-get-interpreter-version";
                process.EnableRaisingEvents = true;

                process.OutputDataReceived += (o, s) => { if (tcs.Task.Status != TaskStatus.RanToCompletion) { tcs.SetResult(s.Data); } };

                process.Start();
                process.BeginOutputReadLine();
                
                return tcs.Task;
            }
            catch (Exception)
            {
                tcs.SetResult(string.Empty);
                return tcs.Task;
            }
        }

        public static void AddInterpreterDialog(bool firstRun = false, bool exitOnFail = false)
        {
            FolderBrowserDialog fbdb;
            string loc;
            string version;
            bool valid = false;
            int loopMax = 3;
            bool forceExit = false;
            while (!valid)
            {
                loopMax--;

                if (loopMax == -1) { forceExit = true; break; }

                fbdb = new FolderBrowserDialog
                {
                    ShowNewFolderButton = false,
                };
                fbdb.Description = $"Locate the folder where the interpreter is installed (HaggisInterpreter2Run.exe)\nNote: Best to move it into a folder where it cannot be accidently deleted!\n{loopMax + 1} attempts left";
                var action = fbdb.ShowDialog(new Form { TopMost = true });

                if (action == DialogResult.Cancel) { forceExit = true; break; }

                if (Directory.Exists(fbdb.SelectedPath))
                {
                    var files = Directory.GetFiles(fbdb.SelectedPath);

                    if (!files.Any(x => Path.GetFileName(x) == "HaggisInterpreter2Run.exe"))
                    {
                        fbdb.Dispose();
                        fbdb = null;
                        continue;
                    }

                    loc = files.First(t => t.Contains("HaggisInterpreter2Run.exe"));

                    // Now we get its version
                    var resultVersion = GetInterpreterVersion(loc);
                    //resultVersion.Start();
                    resultVersion.Wait();
                    System.Diagnostics.Process[] anyRunning;
                    if (!resultVersion.Result.StartsWith("version"))
                    {
                        MessageBox.Show(new Form { TopMost = true },
                        $"Hmm, I didn't expected that - I needed a interpreter version, but I got this instead:\n{resultVersion.Result}",
                        "Error parsing verison",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.None);
                        
                        anyRunning = System.Diagnostics.Process.GetProcessesByName("HaggisInterpreter2Run");
                        foreach (var proc in anyRunning) { proc.Kill(); }

                        break;
                    }

                    anyRunning = System.Diagnostics.Process.GetProcessesByName("HaggisInterpreter2Run");
                    foreach (var proc in anyRunning) { proc.Kill(); }

                    version = resultVersion.Result.Substring(9);

                    if(Properties.Settings.Default.interpreterVersions.Contains(loc))
                    {
                        MessageBox.Show("This version of the interpreter is already ammended, please delete the reoccuring one to replace to this one!");
                        return;
                    }

                    if (firstRun) 
                    { 
                        Properties.Settings.Default.currentInterpreterPath = loc;
                        Properties.Settings.Default.currentInterpreterVersion = version;
                    }

                    if(firstRun)
                        Properties.Settings.Default.interpreterVersions = $"[{version}]{loc}|";
                    else
                        Properties.Settings.Default.interpreterVersions = $"{Properties.Settings.Default.interpreterVersions}[{version}]{loc}|";

                    Properties.Settings.Default.Save();
                    Properties.Settings.Default.Reload();
                    valid = true;
                }
            }

            if (forceExit)
            {
                MessageBox.Show(new Form { TopMost = true },
                    "Sorry, you cannot run this program without a interpreter being installed or located. I am afraid I have to jump out of the window now... adios...",
                    "No can do B0$$",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.None);
                if (exitOnFail) { Application.Exit(); }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Check for latest version (if possible)
            string[] data = new string[0];
            try
            {
#if !DEBUG
                Uri.TryCreate("https://raw.githubusercontent.com/TheE7Player/HaggisInterpreterDev/master/Haggis%20Interpreter/versions.txt", UriKind.RelativeOrAbsolute, out Uri version_location);
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                    client.Encoding = Encoding.UTF8;
                    string s = client.DownloadString(version_location);

                    data = s.Split(Environment.NewLine.ToCharArray());         
                }

                // Gets rid of [exe] tag
                string c_version = data[0].Substring(5);


                    // Only do a check if not in debug mode

                    if (fixVersion(c_version) > fixVersion(version))
                    {
                        bool update = Dialog("Update available", $" Version {version} is now available!\n Would you like to go to the download page?");

                        if (update)
                        {
                            //www.github.com/TheE7Player/HaggisInterpreterDev/releases/latest
                            System.Diagnostics.Process.Start("www.github.com/TheE7Player/HaggisInterpreterDev/releases/latest");
                        }
                    }
#endif

                // Check if first time run (Both properties here should be null/empty)
                if (string.IsNullOrEmpty(Properties.Settings.Default.currentInterpreterPath) || string.IsNullOrEmpty(Properties.Settings.Default.currentInterpreterVersion))
                {
                    MessageBox.Show("This appears to be your first time running this application!\r\n\nPlease locate where you downloaded the interperter in order to run this application smoothly\r\n\nIt can be found here ( grab the latest one! )\r\n\n -> www.github.com/TheE7Player/HaggisInterpreter/releases/latest");

                    AddInterpreterDialog(true, true);

                    interpreterLocationLink.Text = $"Locate Current Interpreter ({Properties.Settings.Default.currentInterpreterVersion})";
                }

            }
            catch (Exception _)
            {
                Console.WriteLine("Issue getting updates");
                Console.WriteLine(_.Message);
            }
            
        }

        private bool Dialog(string title, string message)
        {
            DialogResult dialogResult = MessageBox.Show(new Form { TopMost = true }, message, title, MessageBoxButtons.YesNo);
            return (dialogResult == DialogResult.Yes);
        }
        
        private void HaggisTextBox_UpdateUI(object sender, UpdateUIEventArgs e)
        {
            UpdateToolBar();

            if (needsSaved && !this.Text.EndsWith(" *"))
                this.Text = $"Haggis Interpreter ~ {(string.IsNullOrEmpty(currentFile) ? "UNSAVED SCRIPT" : Path.GetFileNameWithoutExtension(currentFile))}  *";

            if (HaggisTextBox.Text.Length < 1)
            { this.Text = "Haggis Interpreter"; needsSaved = false; }
        }
        
        #endregion

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

            if(!needsSaved)
                needsSaved = true;

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

            if (!needsSaved)
                needsSaved = true;

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

            if(e.Text == "PROCEDURE" || e.Text == "FUNCTION")
            {
                editor.InsertText(editor.CurrentPosition, " ");
                int nicespot = editor.CurrentPosition;

                editor.GotoPosition(nicespot + 1); 
                editor.InsertText(editor.CurrentPosition, (e.Text == "FUNCTION") ? " () RETURNS <DATA TYPE>\n\nEND FUNCTION\n" : " ()\n\nEND PROCEDURE\n"); 
                editor.GotoPosition(nicespot + 1);
            }

            if (e.Text == "WHILE" || e.Text == "REPEAT")
            {
                editor.InsertText(editor.CurrentPosition, " ");
                int nicespot = editor.CurrentPosition;

                editor.GotoPosition(nicespot + 1); 
                editor.InsertText(editor.CurrentPosition, (e.Text == "WHILE") ? " DO\n\t\nEND WHILE\n" : "\n\t\nUNTIL < condition >"); 
                editor.GotoPosition((e.Text == "WHILE") ? nicespot + 1 : nicespot + 3);
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

            if(e.KeyData == (Keys.Control | Keys.O))
            {
                // Prevent input
                e.SuppressKeyPress = true;

                openFileToolStripMenuItem.PerformClick();

                ResetAuto();
            }

            if (e.KeyData == (Keys.Control | Keys.S))
            {
                // Prevent input
                e.SuppressKeyPress = true;

                saveFileToolStripMenuItem.PerformClick();

                ResetAuto();
            }

            if (e.KeyData == (Keys.Control | Keys.N))
            {
                // Prevent input
                e.SuppressKeyPress = true;

                newFileToolStripMenuItem.PerformClick();

                ResetAuto();
            }

        }

        #endregion

        #region Toolbar Related

        private void runREPLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Run the current script in the file
            try
            {
                if(HaggisTextBox.Text.Length<1) { MessageBox.Show("You need some code in order to run the REPL/Interpreter!"); return; }

                string[] file = HaggisTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                var win = new RunDialog(file, string.IsNullOrEmpty(currentFile)?true:false, string.IsNullOrEmpty(currentFile) ? "" : currentFile);
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
        
        private void interpreterLocationLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(Path.GetDirectoryName(Properties.Settings.Default.currentInterpreterPath));
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofdg = new OpenFileDialog
            {
                Filter = "Haggis Script File | *.haggis",
                InitialDirectory = (string.IsNullOrEmpty(Properties.Settings.Default.lastOpenLocation)) ? @"C:\" : Properties.Settings.Default.lastOpenLocation,
                Title = "Please select Haggis Interpreter Script"
             };

            var r = ofdg.ShowDialog();

            if(r == DialogResult.OK)
            {
                if(HaggisTextBox.Text.Length > 0 || !string.IsNullOrEmpty(currentFile))
                if (!Dialog("Opening file with contents", "There is currently text in the REPL - Opening this file will erase what you did\nAre you sure?!"))
                    return;

                string[] text = File.ReadAllText(ofdg.FileName).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                string _text = $"{text[0]}\r\n";
                
                HaggisTextBox.Text = "";
                HaggisTextBox.AppendText(_text);

                for (int i = 1; i < text.Length; i++) { _text = $"{text[i]}\r\n"; HaggisTextBox.AppendText( _text); HaggisTextBox.Update(); }

                //HaggisTextBox.Text = File.ReadAllText(ofdg.FileName);
                currentFile = ofdg.FileName;
                Properties.Settings.Default.lastOpenLocation = Path.GetDirectoryName(ofdg.FileName);
                Properties.Settings.Default.Save(); Properties.Settings.Default.Reload();

                HaggisTextBox.Refresh();
                this.Text = $"Haggis Interpreter ~ {Path.GetFileNameWithoutExtension(ofdg.FileName)}";
            }
        }

        private void settingsMenuItem_Click(object sender, EventArgs e)
        {
            var sw = new Settings();
            sw.ShowDialog();
        }

        private void newFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(HaggisTextBox.Text.Length > 0)
            {
                var r = Dialog("Clear current text", "Are you sure you want to start new?! You haven't saved anything...");

                if (!r)
                    return;

                if (!string.IsNullOrEmpty(currentFile))
                {

                    r = Dialog("Open file still in use", "Are you sure you want to start a new script despite one being open/edited at this trying time?!");
                    if (!r)
                        return;
                }

                currentFile = null;
                this.Text = "Haggis Interpreter";
                HaggisTextBox.ClearAll();
            }
        }

        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(currentFile))
            { 
                var sfg = new SaveFileDialog
                {
                    Filter = "Haggis Script File | *.haggis",
                    InitialDirectory = (string.IsNullOrEmpty(Properties.Settings.Default.lastSafeLocation)) ? @"C:\" : Properties.Settings.Default.lastSafeLocation,
                    Title = "Please save current Haggis Interpreter Script"
                };

                var r = sfg.ShowDialog(new Form { TopMost = true });

                if (r == DialogResult.OK)
                {
                    File.WriteAllText(sfg.FileName, HaggisTextBox.Text);
                    currentFile = sfg.FileName;
                    Properties.Settings.Default.lastSafeLocation = Path.GetDirectoryName(sfg.FileName);
                    Properties.Settings.Default.Save(); Properties.Settings.Default.Reload();

                    this.Text = $"Haggis Interpreter ~ {Path.GetFileNameWithoutExtension(sfg.FileName)}";
                }
                return;
            }

            try
            {
                File.WriteAllText(currentFile, HaggisTextBox.Text);

                needsSaved = false;

                this.Text = this.Text.Replace("*", "").Trim();
            }
            catch (Exception _)
            {
                MessageBox.Show("SAVING FAILED, Please manual copy all the script and manually save it please! My bad bruh.");
                Console.WriteLine(_.Message);
            }
        }

        private void saveAsNewScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfg = new SaveFileDialog
            {
                Filter = "Haggis Script File | *.haggis ",
                InitialDirectory = (string.IsNullOrEmpty(Properties.Settings.Default.lastSafeLocation)) ? @"C:\" : Properties.Settings.Default.lastSafeLocation,
                Title = "Please save current Haggis Interpreter Script"
            };

            var r = sfg.ShowDialog(new Form { TopMost = true });

            if (r == DialogResult.OK)
            {
                File.WriteAllText(sfg.FileName, HaggisTextBox.Text);
                currentFile = sfg.FileName;
                Properties.Settings.Default.lastSafeLocation = Path.GetDirectoryName(sfg.FileName);
                Properties.Settings.Default.Save(); Properties.Settings.Default.Reload();

                this.Text = $"Haggis Interpreter ~ {Path.GetFileNameWithoutExtension(sfg.FileName)}";
            }
        }

        #endregion

    }
}
