namespace SListener
{
    using System;
    using System.Threading;

    public class FakeSerialClient : ITerminalClient
    {
        public string Title
        {
            get { return "FakeSerialClient"; }
        }

        public bool Connect()
        {
            _isConnected = true;
            Action action = SendLogEveryThreeSeconds;
            action.BeginInvoke(null, null);
            return true;
        }

        public void Disconnect()
        {
            _isConnected = false;
        }

        public bool IsConnected()
        {
            return _isConnected;
        }

        public void SendCmd(string str)
        {
            Console.WriteLine(str);
        }

        private void SendLogEveryThreeSeconds()
        {
            while (true)
            {
                if (DataReceivedEvent != null)
                {
                    DataReceivedEvent("DateReceived");
                }
                Thread.Sleep(3000);
            }
        }

        public event DataReceivedHandler DataReceivedEvent;

        private bool _isConnected;
    }

    public class TerminalClientFactory
    {
        public static ITerminalClient GetInstance(string comport)
        {
            if (comport.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
            {
                return new SerialClient(Helper.GetDefaultSerialParams(comport));
            }
            return new FakeSerialClient();
        }
    }
}
