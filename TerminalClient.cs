using System;
using System.IO;
using System.Text;
using System.IO.Ports;
using System.Net.Sockets;
using System.Threading;

namespace SListener
{
    public delegate void DataReceivedHandler(string data);

    public interface ITerminalClient
    {
        String Title
        {
            get;
        }

        bool Connect();

        void Disconnect();

        bool IsConnected();

        void SendCmd(string str);

        event DataReceivedHandler DataReceivedEvent;
    }

    public class SerialClient : ITerminalClient
    {
        private readonly SerialParams _mSerialParams;

        private SerialPort _mSerialPort;

        public SerialClient(SerialParams sp)
        {
            _mSerialParams = sp;
        }

        public virtual String Title
        {
            get
            {
                return _mSerialParams.PortName;
            }
        }

        public virtual bool Connect()
        {
            if (_mSerialPort != null)
            {
                return true;
            }
            _mSerialPort = new SerialPort
                               {
                                   PortName = _mSerialParams.PortName,
                                   BaudRate = _mSerialParams.BaudRate,
                                   DataBits = _mSerialParams.DataBits,
                                   Parity = _mSerialParams.Parity,
                                   StopBits = _mSerialParams.StopBits,
                                   Handshake = _mSerialParams.Handshake
                               };
            _mSerialPort.DataReceived += MSerialPortDataReceived;
            try
            {
                _mSerialPort.Open();
                return true;
            }
            catch(Exception)
            {
                _mSerialPort = null;
                return false;
            }
        }

        void MSerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Chars)
            {
                if (DataReceivedEvent != null)
                {
                    DataReceivedEvent(_mSerialPort.ReadExisting());
                }
            }
        }

        public virtual void Disconnect()
        {
            if (_mSerialPort != null)
            {
                _mSerialPort.Close();
                _mSerialPort = null;
            }
        }

        public virtual bool IsConnected()
        {
            return _mSerialPort != null;
        }

        public virtual void SendCmd(string str)
        {
            if (IsConnected())
            {
                _mSerialPort.Write(str);
            }
        }

        public virtual event DataReceivedHandler DataReceivedEvent;
    }

    class NetworkClient : ITerminalClient
    {
        private readonly NkParams _mNkParams;

        private TcpClient _mClient;

        private Thread _mThread;

        public NetworkClient(NkParams np)
        {
            _mNkParams = np;
        }

        public virtual String Title
        {
            get
            {
                return _mNkParams.Address + "@" + _mNkParams.Port.ToString();
            }
        }

        public virtual bool Connect()
        {
            if(_mClient != null)
            {
                return true;
            }
            try
            {
                _mClient = new TcpClient();
                _mClient.Connect(_mNkParams.Address, _mNkParams.Port);
                _mThread = new Thread(DataReceived);
                _mThread.Start();
                return true;
            }
            catch (Exception)
            {
                _mClient = null;
                return false;
            }
        }

        public void DataReceived()
        {
            if (null == _mClient)
            {
                return;
            }
            try
            {
                NetworkStream ns = _mClient.GetStream();
                while (true)
                {
                    Thread.Sleep(100);
                    if (ns.DataAvailable && DataReceivedEvent != null)
                    {
                        var buffer = new byte[1024];
                        int bytesCount = ns.Read(buffer, 0, 1024);
                        DataReceivedEvent(Encoding.ASCII.GetString(buffer,0,bytesCount));
                    }
                }
            }
            catch
            {
                return;
            }
        }

        public virtual void Disconnect()
        {
            if (_mClient != null)
            {
                _mThread.Abort();
                _mClient.GetStream().Close();
                _mClient.Close();
                _mThread = null;
                _mClient = null;
            }
        }

        public virtual bool IsConnected()
        {
            return _mClient != null;
        }

        public virtual void SendCmd(string str)
        {
            if (IsConnected())
            {
                try
                {
                    var ns = _mClient.GetStream();
                    var writer = new StreamWriter(ns);
                    writer.WriteLine(str);
                    writer.Flush();
                }
                catch
                {
                    return;
                }
            }
        }

        public virtual event DataReceivedHandler DataReceivedEvent;
    }
}
