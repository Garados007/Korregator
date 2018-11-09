/* MIT LICENSE

Copyright 2018 Max Brauer (ma.brauer@live.de)

Permission is hereby granted, free of charge, to any person obtaining a copy of 
this software and associated documentation files (the "Software"), to deal in the 
Software without restriction, including without limitation the rights to use, copy, 
modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
and to permit persons to whom the Software is furnished to do so, subject to the 
following conditions:

The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace KorrekturHilfe
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadData();
            new FastInput(this).Show();
            textBox4.Text = Properties.Settings.Default.PdfPath;
        }

        List<Line> Data = new List<Line>();
        List<string> Cats = new List<string>();
        int TaskIndex = 0;
        bool Changes = false;

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = textBox1.Text;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                LoadData();
            }
        }

        void LoadData()
        {
            var file = textBox1.Text;
            var pdf = textBox4.Text;
            if (File.Exists(file) && File.Exists(pdf))
            {
                groupBox2.Enabled = groupBox3.Enabled = groupBox4.Enabled = true;
                var data = File.ReadAllText(file);
                data = Regex.Replace(data, @"\r\n|\n", (m) =>
                {
                    switch (m.Value)
                    {
                        case "\r\n": return "\n";
                        case "\n": return "\r\n";
                        default: return m.Value;
                    }
                });
                var parts = data.Split(new[] { "\r\n" }, StringSplitOptions.None);
                var tc = new Dictionary<string, int>();
                var lines = new List<Line>(parts.Length);
                var curTask = "";
                foreach (var part in parts)
                {
                    var line = new Line
                    {
                        Raw = part
                    };
                    if (lines.Count != 0 && !part.StartsWith("\"") && !part.StartsWith("--") && part != "")
                    {
                        var sub = tc.TryGetValue(curTask, out int s) ? s + 1 : 1;
                        tc[curTask] = sub;
                        //var p = Regex.Split(part, "(?<=^(([^\"]*\"){2})*[^\"]*);");
                        var p = LineSplitter(part);
                        line.IsTask = true;
                        line.Id = int.Parse(p[0]);
                        line.Id = int.Parse(p[0]);
                        line.Points = float.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        line.MaxPoints = int.Parse(p[2]);
                        line.State = int.Parse(p[4]);
                        line.TComment = p[5];
                        line.SComment = p[6];
                        line.File = p[7];
                        line.Submission = sub;
                        line.Task = curTask;
                    }
                    else if (part.StartsWith("--A"))
                    {
                        var ind = part.IndexOf('_');
                        curTask = part.Substring(ind + 1);
                    }
                    lines.Add(line);
                }
                foreach (var line in lines)
                    if (line.IsTask)
                        line.MaxSubmission = tc[line.Task];
                Data = lines;
                Cats = tc.Keys.ToList();
                TaskIndex = lines.FindIndex((l) => l.IsTask && l.State == 1);
                if (TaskIndex < 0)
                    TaskIndex = lines.FindIndex((l) => l.IsTask);
                PresentData();
                Changes = false;
            }
            else
            {
                groupBox2.Enabled = groupBox3.Enabled = groupBox4.Enabled = false;
            }
            SetTitle();
        }

        void PresentData()
        {
            var line = Data[TaskIndex];
            textBox2.Text = line.SComment;
            textBox3.Text = line.TComment;
            numericUpDown1.Value = (decimal)line.Points;
            comboBox1.SelectedIndexChanged -= comboBox1_SelectedIndexChanged;
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(Cats.ToArray());
            comboBox1.SelectedItem = line.Task;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            //label2.Text = line.Task;
            label4.Text = string.Format("{0}/{1}", line.Submission, line.MaxSubmission);
            checkBox1.Checked = line.State == 3;
            SetTitle();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        string[] LineSplitter(string line)
        {
            List<string> d = new List<string>();
            var sb = new StringBuilder();
            var text = false;
            for (int i = 0; i<line.Length; ++i) 
                switch (line[i])
                {
                    case ';':
                        if (text)
                            sb.Append(';');
                        else
                        {
                            d.Add(sb.ToString());
                            sb.Clear();
                        }
                        break;
                    case '"':
                        text = !text;
                        break;
                    default:
                        sb.Append(line[i]);
                        break;
                }
            if (sb.Length != 0)
                d.Add(sb.ToString());
            return d.ToArray();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var id = Data.FindLastIndex(TaskIndex - 1, (l) => l.IsTask);
            TaskIndex = id < 0 ? TaskIndex : id;
            PresentData();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var id = Data.FindIndex(TaskIndex + 1, (l) => l.IsTask);
            TaskIndex = id < 0 ? TaskIndex : id;
            PresentData();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            var line = Data[TaskIndex];
            line.SComment = textBox2.Text;
            Changes = true;
            SetTitle();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            var line = Data[TaskIndex];
            line.TComment = textBox3.Text;
            Changes = true;
            SetTitle();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            var line = Data[TaskIndex];
            line.Points = (float)numericUpDown1.Value;
            Changes = true;
            SetTitle();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            var line = Data[TaskIndex];
            if (checkBox1.Checked)
                line.State = 3;
            else line.State = line.State == 3 ? 2 : line.State;
            Changes = true;
            SetTitle();
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            Save();
        }

        void Save()
        {
            if (!Changes) return;
            var sb = new StringBuilder();
            foreach (var l in Data)
            {
                var t = Regex.Replace(l.ToString(), "\r\n|\n", "\\n");
                if (sb.Length == 0)
                    sb.Append(t);
                else
                {
                    sb.AppendFormat("\n{0}", t);
                }
            }
            using (var f = new FileStream(textBox1.Text, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            using (var w = new StreamWriter(f))
            {
                w.Write(sb.ToString());
                w.Flush();
            }
            Changes = false;
            SetTitle();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Save();
        }

        void SetTitle()
        {
            if (File.Exists(textBox1.Text))
            {
                Text = string.Format("{0}{1} - Korregator",
                    new FileInfo(textBox1.Text).Name,
                    Changes ? "*" : ""
                    );
            }
            else Text = "Korregator";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var line = Data[TaskIndex];
            var pdf = new Uri(new Uri(textBox1.Text), line.File).LocalPath;
            var args = string.Format(
                "\"{0}\"",
                pdf
            );
            System.Diagnostics.Process.Start(
                textBox4.Text,
                //@"C:\Program Files (x86)\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe",
                args
            );
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TaskIndex = Data.FindIndex((l) => l.IsTask && l.Task == comboBox1.SelectedItem.ToString());
            PresentData();
        }

        bool selMouse = false;

        private void numericUpDown1_Enter(object sender, EventArgs e)
        {
            numericUpDown1.Select();
            numericUpDown1.Select(0, numericUpDown1.Text.Length);
            if (MouseButtons == MouseButtons.Left)
                selMouse = true;
        }

        private void numericUpDown1_MouseDown(object sender, MouseEventArgs e)
        {
            if (selMouse)
            {
                selMouse = false;
                numericUpDown1.Select(0, numericUpDown1.Text.Length);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Save();
            openFileDialog2.FileName = textBox4.Text;
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.PdfPath = textBox4.Text = openFileDialog2.FileName;
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            Save();
            LoadData();
        }
    }

    class Line
    {
        public string Raw;
        public bool IsTask;

        public int Id, MaxPoints, State, Submission, MaxSubmission;
        public string Task, SComment, TComment, File;
        public float Points;

        public override string ToString()
        {
            return IsTask ?
                string.Format("{0};{1};{2};0;{3};\"{4}\";\"{5}\";{6}",
                    Id,
                    Points.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
                    MaxPoints,
                    State,
                    TComment.Replace("\"", "&quot;"),
                    SComment.Replace("\"", "&quot;"),
                    File
                ) : Raw;
        }
    }
}
