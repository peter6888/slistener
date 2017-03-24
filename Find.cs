using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SListener
{
    public partial class Find : Form
    {
        private Form1 m_parentForm = null;

        public Find(Form1 parentForm)
        {
            InitializeComponent();
            m_parentForm = parentForm;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int start = m_parentForm.GetStart();
            int selectedLength = m_parentForm.GetSelectedLength();
            int totalLength = m_parentForm.GetLength();

            m_parentForm.CancelSelected();

            int startPosition = 0;
            if ((start + 1) < totalLength)
            {
                if(selectedLength != 0)
                {
                    startPosition = start + 1;
                }
                else
                {
                    startPosition = start;
                }
            }

            RichTextBoxFinds options = RichTextBoxFinds.None;

            if (checkBox1.Checked)
            {
                options |= RichTextBoxFinds.MatchCase;
            }

            if (checkBox2.Checked)
            {
                options |= RichTextBoxFinds.WholeWord;
            }

            if (checkBox3.Checked)
            {
                options |= RichTextBoxFinds.NoHighlight;
            }

            int result = m_parentForm.FindNext(textBox1.Text, startPosition, options);

            if (result < 0)
            {
                startPosition = 0;
                m_parentForm.FindNext(textBox1.Text, startPosition, options);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                button1.PerformClick();
            }
            else if (e.KeyChar == (char)27)
            {
                button2.PerformClick();
            }
        }
    }
}