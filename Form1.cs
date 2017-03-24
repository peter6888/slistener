using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SListener
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public Form1(string ip, string port)
        {
            InitializeComponent();
            Config.GetInstance().GetConfig().NtPort = int.Parse(port);
            Config.GetInstance().GetConfig().NtAddress = ip;
            Config.GetInstance().GetConfig().ConType = ConnectionType.Network;
            Config.GetInstance().SaveConfig();
        }

        public static readonly string FormTitle = "SListener - Preview";

        private ITerminalClient m_terminalClient = null;

        private StringBuilder m_stringBuffer = new StringBuilder();

        private int m_breakPoint = 0;

        private BoolObject m_isLock = new BoolObject();

        private bool m_isShowFindForm = false;

        private ShareServer m_shareServer = null;

        private bool m_isActived = true;

        public StringBuilder GetBuffer()
        {
            return m_stringBuffer;
        }

        internal ITerminalClient GetClient()
        {
            return m_terminalClient;
        }

        public void SetTitle(string title)
        {
            if (title != null)
            {
                this.Text = FormTitle + " - " + title;
            }
            else
            {
                this.Text = FormTitle;
            }
        }

        public delegate void AppendTextHandler(string data);

        public void AppendText(string data)
        {
            if (this.InvokeRequired)
            {
                if ( IsErrorString(data) )
                {
                    this.Invoke( new MethodInvoker( delegate
                    {
                        richTextBox1.SelectionColor = System.Drawing.Color.Red;
                    }));
                    
                    this.Invoke(new AppendTextHandler(AppendText), new object[] { data });
                }
                else if (IsWarningString(data))
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        richTextBox1.SelectionColor = System.Drawing.Color.YellowGreen;
                    }));

                    this.Invoke(new AppendTextHandler(AppendText), new object[] { data });
                }
                else
                {
                    this.Invoke(new AppendTextHandler(AppendText), new object[] { data });
                }
            }
            else
            {                
                if (richTextBox1.TextLength > Config.GetInstance().GetConfig().ScreenMaxLength)
                {
                    richTextBox1.Text = richTextBox1.Text.Remove(0, richTextBox1.TextLength - Config.GetInstance().GetConfig().ScreenMaxLength/2);
                }
                richTextBox1.AppendText(data);
                if (!m_isActived)
                {
                    if (data.IndexOf("\r") != -1 || data.IndexOf("\n") != -1)
                    {
                        richTextBox1.ScrollToCaret();
                    }
                }
            }
        }

        private bool IsErrorString(string s)
        {
            List<string> errors = new List<string>();
            errors.Add("error");
            errors.Add("exception");
            foreach (string error in errors)
            {
                if (s.ToLower().Contains(error))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsWarningString(string s)
        {
            List<string> errors = new List<string>();
            errors.Add("warning");
            foreach (string error in errors)
            {
                if (s.ToLower().Contains(error))
                {
                    return true;
                }
            }
            return false;
        }

        public delegate int FindHandler(string str, int start, RichTextBoxFinds options);

        public int FindNext(string str, int start, RichTextBoxFinds options)
        {
            if (this.InvokeRequired)
            {
                return (int)this.Invoke(new FindHandler(FindNext), new object[] { str, start, options });
            }
            else
            {
                return richTextBox1.Find(str, start, options);
            }
        }

        public delegate int GetLengthHandler();

        public int GetLength()
        {
            if (this.InvokeRequired)
            {
                return (int)this.Invoke(new GetLengthHandler(GetLength));
            }
            else
            {
                return richTextBox1.TextLength;
            }
        }

        public int GetStart()
        {
            if (this.InvokeRequired)
            {
                return (int)this.Invoke(new GetLengthHandler(GetStart));
            }
            else
            {
                return richTextBox1.SelectionStart;
            }
        }

        public int GetSelectedLength()
        {
            if (this.InvokeRequired)
            {
                return (int)this.Invoke(new GetLengthHandler(GetSelectedLength));
            }
            else
            {
                return richTextBox1.SelectionLength;
            }
        }

        public delegate void FunctionHandler();

        public void CancelSelected()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new FunctionHandler(CancelSelected));
            }
            else
            {
                richTextBox1.Select(richTextBox1.SelectionStart,0);
            }            
        }

        public void SetFormSize()
        {
            this.Size = new Size(Config.GetInstance().GetConfig().Width, Config.GetInstance().GetConfig().Height);
        }

        public void CheckAutoConnect()
        {
            if (Config.GetInstance().GetConfig().AutoConnect)
            {
                if (Config.GetInstance().GetConfig().ConType == ConnectionType.Serial)
                {
                    SerialParams sp = Config.GetInstance().GetConfig().SerialParams;
                    if ( !TryConnectTerminalClient(sp) )
                    {
                        m_terminalClient = null;
                        sp.PortName = "COM2";
                        if(!TryConnectTerminalClient(sp))
                        {
                            m_terminalClient = null;
                        }
                    }
                }
                else
                {
                    ConnectWithNkParams(Config.GetInstance().GetConfig().NtAddress,
                                        Config.GetInstance().GetConfig().NtPort);
                }
            }            
        }

        private bool TryConnectTerminalClient(SerialParams sp)
        {
            m_terminalClient = new SerialClient(sp);
            m_terminalClient.DataReceivedEvent += MTerminalClientDataReceivedEvent;
            if (m_terminalClient.Connect())
            {
                SetTitle(m_terminalClient.Title);
                SetConnectState();
                return true;
            }
            MessageBox.Show(String.Format("Connecting to {0} failed.",sp.PortName));
            return false;
        }

        void AppendTextToBuffer(string data)
        {
            m_stringBuffer.Append(data);
            if (GetBufferLength() > Config.GetInstance().GetConfig().BufferMaxLength)
            {
                int tDiff = GetBufferLength() - Config.GetInstance().GetConfig().BufferMaxLength;
                m_stringBuffer.Remove(0, tDiff);
                m_breakPoint -= tDiff;
                m_shareServer.DiffLength = tDiff;
                if (m_breakPoint < 0)
                {
                    m_breakPoint = 0;
                }
            }
        }

        int GetBufferLength()
        {
            return m_stringBuffer.Length;
        }

        void MTerminalClientDataReceivedEvent(string data)
        {
            lock (m_isLock)
            {
                if (m_isLock.Boolean)
                {
                    AppendTextToBuffer(data);
                }
                else
                {
                    AppendTextToBuffer(data);
                    //AppendText(data);
                    m_breakPoint = GetBufferLength();
                }
            }
            if (m_breakPoint == GetBufferLength())
            {
                AppendText(data);
            }
        }

        public void CheckAutoCapture()
        {
            if (Config.GetInstance().GetConfig().AutoCapture)
            {
                RestoreCapture();
            }
        }

        public void CheckAutoShare()
        {
            m_shareServer = new ShareServer(this);
            if (Config.GetInstance().GetConfig().AutoShare)
            {
                if (m_shareServer.Run())
                {
                    SetShareState();
                }
                else
                {
                    SetDisableShareState();
                }
            }
        }

        public void Initialize()
        {
            disconnectToolStripMenuItem.Enabled = false;
            disableShareToolStripMenuItem.Enabled = false;
            toolStripButton4.Enabled = false;
            toolStripButton6.Enabled = false;
            SetStop();
            SetTitle(null);
            m_isShowFindForm = false;
        }

        public void SetConnectState()
        {
            connectToToolStripMenuItem.Enabled = false;
            disconnectToolStripMenuItem.Enabled = true;
            toolStripButton3.Enabled = false;
            toolStripButton4.Enabled = true;
        }

        public void SetDisconnectState()
        {
            connectToToolStripMenuItem.Enabled = true;
            disconnectToolStripMenuItem.Enabled = false;
            toolStripButton3.Enabled = true;
            toolStripButton4.Enabled = false;
        }

        public void SetShareState()
        {
            shareToolStripMenuItem.Enabled = false;
            disableShareToolStripMenuItem.Enabled = true;
            toolStripButton5.Enabled = false;
            toolStripButton6.Enabled = true;
        }

        public void SetDisableShareState()
        {
            shareToolStripMenuItem.Enabled = true;
            disableShareToolStripMenuItem.Enabled = false;
            toolStripButton5.Enabled = true;
            toolStripButton6.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Initialize();            
            Config.GetInstance().LoadConfig();
            SetFormSize();
            CheckAutoConnect();
            CheckAutoCapture();
            CheckAutoShare();
            CheckAutoShare();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_terminalClient != null)
            {
                m_terminalClient.Disconnect();
                m_terminalClient = null;
            }
            if (m_shareServer != null)
            {
                m_shareServer.Stop();
            }
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            m_isActived = true;
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            m_isActived = false;
        }

        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (m_terminalClient != null)
            {
                lock (m_isLock)
                {
                    if (!m_isLock.Boolean)
                    {
                        m_terminalClient.SendCmd(e.KeyChar.ToString());
                    }
                }
            }
        }

        private void richTextBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lock (m_isLock)
                {                    
                    if (m_isLock.Boolean)
                    {
                        RestoreCapture();
                        if (m_stringBuffer.Length > m_breakPoint)
                        {
                            richTextBox1.AppendText(m_stringBuffer.ToString().Substring(m_breakPoint));
                            m_breakPoint = m_stringBuffer.Length;
                        }
                    }
                    else
                    {
                        SetStop();
                    }
                }
            }
        } 
       
        public void SetStopState()
        {
            stopToolStripMenuItem.Enabled = false;
            toolStripButton9.Enabled = false;
            stopToolStripMenuItem1.Enabled = false;
            captureToolStripMenuItem.Enabled = true;
            captureToolStripMenuItem1.Enabled = true;
            toolStripButton8.Enabled = true;
            richTextBox1.BackColor = SystemColors.Control;
        }

        public void RestoreCaptureState()
        {
            stopToolStripMenuItem.Enabled = true;
            toolStripButton9.Enabled = true;
            stopToolStripMenuItem1.Enabled = true;
            captureToolStripMenuItem.Enabled = false;
            captureToolStripMenuItem1.Enabled = false;
            toolStripButton8.Enabled = false;
            richTextBox1.BackColor = Color.White;
        }

        public void SetStop()
        {
            m_isLock.Boolean = true;
            SetStopState();
        }

        public void RestoreCapture()
        {
            m_isLock.Boolean = false;
            RestoreCaptureState();
        }        

        #region MainMenuItem

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.CheckPathExists = true;
            sfd.Filter = "Text Documents (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                RichTextBoxStreamType rtbsType;
                if (sfd.FilterIndex == 1)
                {
                    rtbsType = RichTextBoxStreamType.UnicodePlainText;
                }
                else if (sfd.FilterIndex == 2)
                {
                    rtbsType = RichTextBoxStreamType.RichText;
                }
                else
                {
                    rtbsType = RichTextBoxStreamType.PlainText;
                }
                richTextBox1.SaveFile(sfd.FileName, rtbsType);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!m_isShowFindForm)
            {
                Find f = new Find(this);
                f.FormClosed += new FormClosedEventHandler(f_FormClosed);
                m_isShowFindForm = true;
                f.Show();
            }
        }

        void f_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_isShowFindForm = false;
        }

        private void goToToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (m_isLock)
            {
                m_stringBuffer.Remove(0, m_stringBuffer.Length);
                richTextBox1.Clear();
                m_breakPoint = 0;
            }
        }

        private void clearScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (m_isLock)
            {
                richTextBox1.Clear();
            }
        }

        private void connectToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_terminalClient != null)
            {
                return;
            }
            ConnectTo ct = new ConnectTo();
            if (ct.ShowDialog() == DialogResult.OK)
            {
                if (ct.IsSerial)
                {
                    SerialParams sp = Helper.GetDefaultSerialParams();
                    sp.PortName = ct.SerialPort.ToUpper();
                    if (!TryConnectTerminalClient(sp) )
                    {
                        m_terminalClient = null;
                    }
                }
                else
                {
                    ConnectWithNkParams(ct.Address, ct.Port);
                }                
            }
        }

        private void ConnectWithNkParams(string address, int port)
        {
            var np = new NkParams {Address = address, Port = port};
            m_terminalClient = new NetworkClient(np);
            m_terminalClient.DataReceivedEvent += new DataReceivedHandler(MTerminalClientDataReceivedEvent);
            if (m_terminalClient.Connect())
            {
                SetTitle(m_terminalClient.Title);
                SetConnectState();
            }
            else
            {
                MessageBox.Show(String.Format("Connection to {0}:{1} failed.",np.Address,np.Port));
                m_terminalClient = null;
            }
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_terminalClient != null)
            {
                m_terminalClient.Disconnect();
                m_terminalClient = null;                
                SetTitle(null);
            }
            SetDisconnectState();
        }

        private void shareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_shareServer.Run())
            {
                SetShareState();
            }
            else
            {
                SetDisableShareState();
            }
        }

        private void disableShareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_shareServer.IsRun())
            {
                m_shareServer.Stop();
            }
            SetDisableShareState();
        }

        private void captureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (m_isLock)
            {
                if (m_isLock.Boolean)
                {
                    RestoreCapture();
                    if (m_stringBuffer.Length > m_breakPoint)
                    {
                        richTextBox1.AppendText(m_stringBuffer.ToString().Substring(m_breakPoint));
                        m_breakPoint = m_stringBuffer.Length;
                    }
                }
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (m_isLock)
            {
                if (!m_isLock.Boolean)
                {
                    SetStop();
                }
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        #endregion        

        #region ContextMenuItem

        private void captureToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            captureToolStripMenuItem_Click(null, null);
        }

        private void stopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            stopToolStripMenuItem_Click(null, null);
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }

        private void findToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            findToolStripMenuItem_Click(null, null);
        }

        private void selectAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
        }

        private void clearToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            clearToolStripMenuItem_Click(null, null);
        }

        private void clearScreenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            clearScreenToolStripMenuItem_Click(null, null);
        }

        #endregion             
   
        #region ToolStrip

        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            toNotepadToolStripMenuItem_Click(null, null);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            connectToToolStripMenuItem_Click(null, null);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            disconnectToolStripMenuItem_Click(null, null);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            shareToolStripMenuItem_Click(null,null);
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            disableShareToolStripMenuItem_Click(null, null);
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            clearToolStripMenuItem_Click(null, null);
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            clearScreenToolStripMenuItem_Click(null, null);
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            captureToolStripMenuItem_Click(null, null);
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            stopToolStripMenuItem_Click(null, null);
        }

        #endregion  

        private void toNotepadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tempfilename = string.Format("{0} - {1}.txt",Text,Guid.NewGuid());
            richTextBox1.SaveFile(tempfilename, RichTextBoxStreamType.PlainText);
            System.Diagnostics.Process.Start(@"notepad.exe", tempfilename);
        }
    }
}