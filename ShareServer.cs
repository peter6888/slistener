using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace SListener
{
    class ShareServer
    {
        private StringBuilder m_buffer = null;

        private Thread m_thread = null;

        private TcpListener m_listener = null;

        //private ITerminalClient m_client = null;

        private Form1 m_form = null;

        private int m_diffLength = 0;

        private object m_lockObj = new object();

        public int DiffLength
        {
            set
            {
                lock (m_lockObj)
                {
                    m_diffLength += value;
                }
            }
        }

        public ShareServer(Form1 form)
        {
            m_buffer = form.GetBuffer();
            m_form = form;
        }

        public bool Run()
        {
            if (m_thread != null)
            {
                return true;
            }
            else
            {
                try
                {
                    m_listener = new TcpListener(IPAddress.Any,Config.GetInstance().GetConfig().SharePort);
                    m_listener.Start();
                }
                catch (Exception e)
                {
                    m_listener = null;
                    System.Windows.Forms.MessageBox.Show(e.Message);
                    return false;
                }
                m_thread = new Thread(new ThreadStart(ListenerThread));
                m_thread.Start();
                return true;
            }
        }

        public void Stop()
        {
            if (m_thread != null)
            {
                m_thread.Abort();
                m_thread = null;
                m_listener.Stop();
                m_listener = null;
            }
        }

        public bool IsRun()
        {
            return m_thread != null ? true : false;
        }

        public void ListenerThread()
        {
            while (true)
            {
                TcpClient tcpClient = m_listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(new WaitCallback(WriteThread), tcpClient);
                ThreadPool.QueueUserWorkItem(new WaitCallback(ReadThread), tcpClient);
            }
        }

        public void WriteThread(Object state)
        {
            int pos = 0;
            TcpClient tcpClient = state as TcpClient;
            NetworkStream ns = tcpClient.GetStream();
            while (true)
            {
                try
                {
                    if (!tcpClient.Connected)
                        break;
                    int currentLength = m_buffer.Length;
                    lock (m_lockObj)
                    {
                        if (m_diffLength != 0)
                        {
                            pos -= m_diffLength;
                            m_diffLength = 0;
                            if (pos < 0)
                            {
                                pos = 0;
                            }
                        }
                    }
                    if (pos < currentLength)
                    {
                        if (ns.CanWrite)
                        {
                            string strBuffer = m_buffer.ToString().Substring(pos, currentLength - pos);                            
                            byte[] tempBuffer = Encoding.Unicode.GetBytes(strBuffer);
                            byte[] len = BitConverter.GetBytes(tempBuffer.Length);
                            ns.Write(len, 0, len.Length);
                            ns.Write(tempBuffer, 0, tempBuffer.Length);
                            pos = currentLength;
                        } 
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        public void ReadThread(Object state)
        {
            TcpClient tcpClient = state as TcpClient;
            NetworkStream ns = tcpClient.GetStream();
            while (true)
            {
                try
                {
                    if (!tcpClient.Connected)
                        break;
                    if (m_form.GetClient() != null)
                    {
                        byte[] len = new byte[4];
                        int resLen = ns.Read(len, 0, len.Length);
                        if (0 == resLen)
                        {
                            break;
                        }
                        int length = BitConverter.ToInt32(len, 0);
                        byte[] buffer = new byte[length];
                        ns.Read(buffer, 0, length);
                        m_form.GetClient().SendCmd(Encoding.Unicode.GetString(buffer));
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                catch
                {
                    break;
                }
            }
        }
    }
}
