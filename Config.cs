using System;
using System.IO.Ports;

namespace SListener
{
    class Config
    {
        private static readonly Config MInstance = new Config();
        private readonly Configuration _mConfiguration;

        private Config() {
            _mConfiguration = new Configuration();
            _mConfiguration.SetDefault();
        }

        public static Config GetInstance()
        {
            return MInstance;
        }

        public void LoadConfig()
        {
        }

        public void SaveConfig()
        {
        }

        public void RestoreDefault()
        {
            _mConfiguration.SetDefault();
        }

        public Configuration GetConfig()
        {
            return _mConfiguration;
        }
    }

    class BoolObject
    {
        public bool Boolean { get; set; }
    }

    public enum ConnectionType
    {
        Serial,
        Network
    }

    public class NkParams
    {
        public string Address { get; set; }
        public int Port { get; set; }
    }

    public class SerialParams
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Handshake Handshake { get; set; }
    }

    public class Helper
    {
        public static SerialParams GetDefaultSerialParams()
        {
            return GetDefaultSerialParams("COM1");
        }
        public static SerialParams GetDefaultSerialParams(string comPort)
        {
            SerialParams sp = new SerialParams();
            sp.PortName = comPort;
            sp.BaudRate = 115200;
            sp.DataBits = 8;
            sp.Parity = Parity.None;
            sp.StopBits = StopBits.One;
            sp.Handshake = Handshake.None;
            return sp;
        }
    }

    class Configuration
    {
        public void SetDefault()
        {
            AutoShare = false;
            SharePort = 8066;
            AutoConnect = true;
            ConType = ConnectionType.Serial;
            NtAddress = null;
            NtPort = SharePort;
            Width = 1024;
            Height = 768;
            AutoCapture = true;
            SerialParams = Helper.GetDefaultSerialParams();
            BufferMaxLength = Int16.MaxValue * 32;
            ScreenMaxLength = Int16.MaxValue * 32;
        }

        public bool AutoShare { get; set; }
        public int SharePort { get; set; }
        public bool AutoConnect { get; set; }
        public ConnectionType ConType { get; set; }
        public string NtAddress { get; set; }
        public int NtPort { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool AutoCapture { get; set; }
        public int BufferMaxLength { get; set; }
        public int ScreenMaxLength { get; set; }
        public SerialParams SerialParams { get; private set; }
    }
}
