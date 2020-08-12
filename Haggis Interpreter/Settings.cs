using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Haggis_Interpreter
{
    public partial class Settings : Form
    {
        private Dictionary<string, string> interpreterVer = new Dictionary<string, string>(1);

        private bool Dialog(string title, string message)
        {
            DialogResult dialogResult = MessageBox.Show(new Form { TopMost = true }, message, title, MessageBoxButtons.YesNo);
            return (dialogResult == DialogResult.Yes);
        }

        private void UpdateVersion()
        {
            var iv = Properties.Settings.Default.interpreterVersions.Split('|');
            var r = @"\[(.+)\](.+)";
            Match m;
            InterpreterVersions.Items.Clear();
            foreach (string item in iv)
            {
                if (string.IsNullOrEmpty(item))
                    continue;

                m = new Regex(r).Match(item);
                if(!interpreterVer.ContainsKey(m.Groups[1].Value))
                    interpreterVer.Add(m.Groups[1].Value, m.Groups[2].Value);

                if(!InterpreterVersions.Items.Contains(m.Groups[1].Value))
                    InterpreterVersions.Items.Add(m.Groups[1].Value);
            }

            int currIndex = InterpreterVersions.Items.IndexOf(Properties.Settings.Default.currentInterpreterVersion);
            InterpreterVersions.SelectedIndex = currIndex;
        }

        public Settings()
        {
            InitializeComponent();

            UpdateVersion();
        }

        private void resetConfig_Click(object sender, EventArgs e)
        {
            if (Dialog("Reset User Configuration", "Doing so would mean setting up the interpreter again!\nAre you sure - boss??"))
            {
                Properties.Settings.Default.currentInterpreterPath = null;
                Properties.Settings.Default.currentInterpreterVersion = null;
                Properties.Settings.Default.interpreterVersions = null;
                Properties.Settings.Default.Save();
                MessageBox.Show("Haggis Interpreter now has to exit to allow this change - you'll need to setup the interpreter location again!");
                Application.Exit();
            }

        }

        private void Settings_Load(object sender, EventArgs e)
        {

        }

        private void AddInterp_Click(object sender, EventArgs e)
        {
            Form1.AddInterpreterDialog(false, false);
            UpdateVersion();
            var anyRunning = System.Diagnostics.Process.GetProcessesByName("HaggisInterpreter2Run");
            foreach (var proc in anyRunning) { proc.Kill(); }
        }

        private void InterpreterVersions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(InterpreterVersions.Text != Properties.Settings.Default.currentInterpreterVersion)
            {
                if(Dialog("Change Activate Interpreter", "Are you sure you want to change the current interpreter to this one?\n(Restart is required)"))
                {
                    Properties.Settings.Default.currentInterpreterPath = interpreterVer[InterpreterVersions.Text];
                    Properties.Settings.Default.currentInterpreterVersion = InterpreterVersions.Text;
                    Properties.Settings.Default.Save();
                    MessageBox.Show("Changed Interpreter - Please restart this application for the change to happen!");
                }
            }
        }

        private void removeInterp_Click(object sender, EventArgs e)
        {
            if(Dialog("Remove Interpreter Instance", $"Are you sure you want to remove {InterpreterVersions.Text} from the list of available interpreters?"))
            {

                string version = Properties.Settings.Default.interpreterVersions;
                var x = version.Split('|').Where(z => !string.IsNullOrEmpty(z));

                var loc = x.First(y => y.StartsWith($"[{InterpreterVersions.Text}]"));
                version = version.Replace(loc, "").Trim();
                
                interpreterVer.Remove(InterpreterVersions.Text);
                InterpreterVersions.Items.Remove(InterpreterVersions.Text);
                Properties.Settings.Default.interpreterVersions = version;
                Properties.Settings.Default.Save();

                if(InterpreterVersions.Items.Count == 0)
                {
                    MessageBox.Show("There is no interpreter selected, application will now close for you to select a interpreter!");
                    Application.Exit();
                }
                else
                {
                    InterpreterVersions.SelectedIndex = InterpreterVersions.Items.Count - 1;
                }
            }
        }

        private void InterpreterVersions_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Prevents keys from being inputted (Tells the event its already handled!) *yeet*
            e.Handled = true;
        }
    }
}
