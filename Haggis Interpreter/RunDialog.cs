using HaggisInterpreter2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Haggis_Interpreter
{
    public partial class RunDialog : Form
    {
        #region GUI Properties
        string[] file;
        Process process;
        bool CreateFile;
        string safeLocation;
        IsolatedStorageFile isoStore;
        #endregion

        #region Socket Properties
        IPAddress ip;
        IPEndPoint ep;
        Socket receiver;
        #endregion

        List<string> socketData;

        public RunDialog(string[] contents, bool createFile, string fileLocation = null)
        {
            CloseInstance();     
            file = contents;
            CreateFile = createFile;
            safeLocation = (ReferenceEquals(fileLocation, null)) ? null : fileLocation;
            InitializeComponent();
        }

        #region GUI Logic
        private void RunDialog_Load(object sender, EventArgs e)
        {
            if (CreateFile)
                CreateSafeFile();

            this.ip = IPAddress.Parse("127.0.0.1");
            this.ep = new IPEndPoint(ip, 595);
            receiver = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socketData = new List<string>(1);

            try
            {
                process = new Process();

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.CreateNoWindow = true;

                process.StartInfo.FileName = Properties.Settings.Default.currentInterpreterPath;

                //$"\"-input_output_only\" \"{safeLocation}\"";
                process.StartInfo.Arguments = $"\"-input_output_only\" \"-socket\" \"{safeLocation}\"";

                process.EnableRaisingEvents = true;

                process.OutputDataReceived += new DataReceivedEventHandler(AppendText);

                process.Exited += (s, x) =>
                {
                    this.BeginInvoke(new MethodInvoker(() =>
                    {
                        Console.WriteLine("[HIv2] Finish");
                        this.Text += " ~ Finished";
                        this.textBox2.Enabled = false;
                    }));
                };

                process.Start();
                process.BeginOutputReadLine();
                Console.WriteLine("[HIv2] Started");

                backgroundWorker1.RunWorkerAsync();

            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
            }
        }

        private void CreateSafeFile()
        {
            isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

            isoStore.CreateFile("runfile.haggis").Close();

            safeLocation = Path.Combine(isoStore.GetType().GetField("m_RootDir", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(isoStore).ToString(), "runfile.haggis");
            File.WriteAllLines(safeLocation, file);

            Console.WriteLine($"Created safe file of the script at: {safeLocation}");

            isoStore.Close();
        }

        private void AppendText(object s, DataReceivedEventArgs e)
        {
            try
            {
                this.BeginInvoke(new MethodInvoker(() =>
                {
                    richTextBox1.AppendText($"{e.Data}\n" ?? string.Empty);
                }));
            }
            catch (Exception x)
            {
                Console.WriteLine($"[OUTPUT EXCEPTION] {x.Message}");
            }          
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (ReferenceEquals(process, null))
                    return;

                if (e.KeyData == Keys.Enter)
                {
                    Console.WriteLine($"Input: {textBox2.Text}");
                    process.StandardInput.WriteLine(textBox2.Text);
                    textBox2.Clear();
                    e.SuppressKeyPress = true; // Prevent windows thinking you cannot press enter (Makes a ding noise!)
                }              
            }
            catch (Exception x)
            {
                Console.WriteLine($"[INPUT EXCEPTION] {x.Message}");
            }
        }

        private void CloseInstance(bool isEnd = false)
        {
            if(!ReferenceEquals(process, null))
                if (!process.HasExited)
                {
                    Console.WriteLine((isEnd) ? "FORCE CLOSING THE INTERPRETER" : "Process is already running, attempting to close down");
                    process.Kill();
                }

            var any_mischief_instances = Process.GetProcessesByName("HaggisInterpreter2");
            if (any_mischief_instances.Length == 0)
            { Console.WriteLine((isEnd) ? "FORCE CLOSING WAS SUCCESSFUL :)" : "Process is closed, starting to run again..."); }
            else
            {
                foreach (Process proc in any_mischief_instances) { proc.Kill(); }
                Console.WriteLine($"HAD TO ITERATE OVER INSTANCES, MORE THAN 1! ({any_mischief_instances.Length})");
            }
        }

        private void RunDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseInstance(true);
        }

        #endregion

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            bool socketWaitMessage = false;

            try
            {
                Thread.Sleep(100);
                receiver.Connect(ep);
                Console.WriteLine("[HIv2] Connected to the socket");
                bool running = true;
                byte[] messageReceived = new byte[1024];
                int byteRecv = 0;
                string message = "";
                while (running)
                {
                    byteRecv = receiver.Receive(messageReceived);
                    message = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
                    socketData.Add(message);
                    Console.WriteLine($"[HIv2] Got data back: {message}");

                    if(!socketWaitMessage)
                    {
                        socketWaitMessage = true;
                        this.BeginInvoke(new MethodInvoker(() =>
                        {
                            richTextBox1.AppendText("Waiting to collect the interpreters lifespan data...\n");
                        }));
                    }

                    if (message.StartsWith("[time]"))
                        running = false;
                }
                Console.WriteLine("[HIv2] Reached sockets lifespan - Finished.");
                this.BeginInvoke(new MethodInvoker(() =>
                {
                    string[] cat_message = {
                        "",
                        "  |------------------------------|",
                        "  | YOU CAN NOW   |",
                        "  | SAFELY EXIT       |",
                        "  | THIS WINDOW   |",
                        "  | BOSS   :3              |",
                        "  |------------------------------|",
                        @"     (\__/) ||",
                        "      (•ㅅ•) ||",
                        "      / 　 づ"
                    };
                    richTextBox1.AppendText(string.Join("\n", cat_message));
                }));
            }
            catch (Exception)
            {
                richTextBox1.AppendText("An error occured - woops!");
            }

            var results = new InterpreterResults(socketData);
            results.StartPosition = FormStartPosition.CenterScreen;
            results.TopMost = true;
            results.ShowDialog();
        }
    }
}
