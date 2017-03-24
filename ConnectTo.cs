using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SListener
{
    public partial class ConnectTo : Form
    {
        public ConnectTo()
        {
            InitializeComponent();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                radioButton2.Checked = false;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
            }
            else
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                radioButton1.Checked = false;
                textBox3.Enabled = true;
            }
            else
            {
                textBox3.Enabled = false;
            }
        }

        private void ConnectTo_Load(object sender, EventArgs e)
        {
            radioButton2.Checked = true;
            textBox3.Enabled = true;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
        }

        public bool IsSerial
        {
            get
            {
                return radioButton2.Checked;
            }
        }

        public string Address
        {
            get
            {
                return textBox1.Text;
            }
        }

        public int Port
        {
            get
            {
                try
                {
                    return Int32.Parse(textBox2.Text);
                }
                catch
                {
                    return 8066;
                }
            }
        }

        public string SerialPort
        {
            get
            {
                return textBox3.Text;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                button1.PerformClick();
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                button1.PerformClick();
            }
        }
    }
}