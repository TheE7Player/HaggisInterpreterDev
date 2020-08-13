using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Haggis_Interpreter
{
    public partial class InterpreterResults : Form
    {
        List<string> data;
        List<string> output;
       
        public InterpreterResults(List<string> data, List<string> output)
        {
            InitializeComponent();
            this.data = data;
            this.output = output;
        }

        private void SetupTimerAndGrid()
        {
            // Time label
            var t_idx = data.FindIndex(x => x.StartsWith("[time]"));
            
            var times = data[t_idx].Substring(data[t_idx].IndexOf(']') + 1)
                .Split('|')
                .Select(x => Math.Floor(Convert.ToDouble(x)))
                .ToArray();

            durationLabel.Text = $"{times[0]}m {times[1]}s {times[2]}ms";

            var variables_all = data.FindAll(x => x.StartsWith("[variable_"));                      
            var variables = variables_all.Select(y => y.Substring(y.IndexOf(']') + 1)).ToArray();

            variableGrid.Columns.Add("v_m", "Name");
            variableGrid.Columns.Add("v_v", "Value");
            variableGrid.Columns.Add("v_t", "Type");

            int i = 0;
            Color bg;
            bool isDecleared = false;
            List<string> _rdata;
            foreach (var row in variables)
            {

                isDecleared = (variables_all[i].Contains("decl"));

                _rdata = row.Split('|').ToList();
                _rdata.Add((isDecleared) ? "Declared" : "Input");

                bg = isDecleared ? Color.LightBlue : Color.AliceBlue;
                variableGrid.Rows.Add(_rdata.ToArray());

                variableGrid.Rows[i].DefaultCellStyle.BackColor = bg;

                i++;
            }

            variableGrid.Dock = DockStyle.Fill;
            variableGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            variableGrid.RowHeadersVisible = false; // Removes "current row" arrow
            //Highlight if it was a declared or inputed


            variableGrid.Refresh(); variableGrid.Update();
        }

        private void SetupInfo()
        {
            // Setup server information first
            richTextBox1.Text = "Information about the interpreter environment\n\n";

            string sInfo = data.First(f => f.StartsWith("[i_server]"));
            sInfo = sInfo.Substring(sInfo.IndexOf(']') + 1);

            string[] sInfo_A = sInfo.Split('|');

            richTextBox1.AppendText($"Server IP: {sInfo_A[0].Trim()}\n");
            richTextBox1.AppendText($"Server Port: {sInfo_A[1].Trim()}\n");
            richTextBox1.AppendText($"Server Traffic Protocol: {sInfo_A[2].Trim()}\n\n");

            sInfo = data.First(f => f.StartsWith("[i_version]"));
            sInfo = sInfo.Substring(sInfo.IndexOf(']') + 1);

            richTextBox1.AppendText($"Interpreter Version: {sInfo}\n\n");

            sInfo = data.First(f => f.StartsWith("[i_arguments]"));
            sInfo = sInfo.Substring(sInfo.IndexOf(']') + 1);

            richTextBox1.AppendText($"Interpreter Arguments:\n{sInfo}\n\n");
            sInfo = null;
            sInfo_A = null;
            GC.Collect();
        }

        private void SetupOutputs()
        {
            var sb = new StringBuilder();
            foreach (var line in output)
            {
                if(line.StartsWith("[O]"))
                {
                    sb.AppendLine(line.Replace("[O]", "").Trim());
                }
                else
                {
                    sb.AppendLine(line.Replace("[I]", "> ").Trim());
                }
            }
            textBox1.ReadOnly = false;
            textBox1.Text = sb.ToString();
            textBox1.ReadOnly = true;
        }

        private void InterpreterResults_Load(object sender, EventArgs e)
        {
            SetupTimerAndGrid(); SetupInfo(); SetupOutputs();
        }
    }
}
