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

namespace KorrekturHilfe
{
    public partial class FastInput : Form
    {
        Form1 form1;

        public FastInput(Form1 form1)
        {
            InitializeComponent();
            this.form1 = form1;
            form1.groupBox3.EnabledChanged += GroupBox3_EnabledChanged;
            form1.numericUpDown1.TextChanged += NumericUpDown1_TextChanged;
        }

        private void NumericUpDown1_TextChanged(object sender, EventArgs e)
        {
            textBox1.TextChanged -= textBox1_TextChanged;
            textBox1.Text = form1.numericUpDown1.Text;
            textBox1.TextChanged += textBox1_TextChanged;
        }

        private void GroupBox3_EnabledChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = form1.groupBox3.Enabled;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            form1.numericUpDown1.TextChanged -= NumericUpDown1_TextChanged;
            form1.numericUpDown1.Text = textBox1.Text;
            form1.numericUpDown1.TextChanged += NumericUpDown1_TextChanged;
        }

        bool entered = false;

        private void textBox1_Enter(object sender, EventArgs e)
        {
            textBox1.SelectAll();
            entered = true;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                form1.checkBox1.Checked = true;
                form1.button4.PerformClick();
                form1.button2.PerformClick();
                new Task(async () =>
                {
                    entered = true;
                    await Task.Delay(1000);
                    Invoke(new Action(() =>
                    {
                        this.Activate();
                        Show();
                        Focus();
                        textBox1.Show();
                        textBox1.Select();
                        textBox1.Focus();
                        textBox1.SelectAll();
                    }));

                }).Start();
            }
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (entered)
                textBox1.SelectAll();
            entered = false;
        }
    }
}
