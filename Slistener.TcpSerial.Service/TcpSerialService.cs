using System;
using System.Collections.Generic;
using System.Net;

namespace Slistener.TcpSerial.Service
{
    /// <summary>
    /// Service maintains all the serial and tcp:port mappings
    /// for each COM port it open and tcp:port to output (input in the future) to TcpClients
    /// </summary>
    public class TcpSerialService
    {
        public int StartTcpPort { get; set; }
        public int MaxPorts { get; set; }

        public TcpSerialService()
        {
            StartTcpPort = 30000;
            MaxPorts = 16;
        }

        public void Start()
        {
            _tcpSerialList = new List<TcpSerialClass>();
            var newTcpSerial = new TcpSerialClass(LocalIPAddress(), StartTcpPort, "fakecom");
            _tcpSerialList.Add(newTcpSerial);
            Action startAction = newTcpSerial.Start;
            startAction.BeginInvoke(null, null);

            for(int port=1; port<=MaxPorts; port++)
            {
                var tcpSerial = new TcpSerialClass(LocalIPAddress(), StartTcpPort + port, string.Format("COM{0}",port));
                _tcpSerialList.Add(tcpSerial);
                Action action = tcpSerial.Start;
                action.BeginInvoke(null, null);
            }
        }

        private static IPAddress LocalIPAddress()
        {
            foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    return ip;
                }
            }
            throw new NotSupportedException("No Ip Address for service to start");
        }

        List<TcpSerialClass> _tcpSerialList;
    }
}
