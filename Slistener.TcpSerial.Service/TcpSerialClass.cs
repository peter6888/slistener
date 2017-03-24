using SListener;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Slistener.TcpSerial.Service
{
    class TcpSerialClass
    {
        public bool IsStarted { get; set; }

        public TcpSerialClass(IPAddress ipAddress, int ipPort, string comPort)
        {
            _listener = new TcpListener(ipAddress, ipPort);
            _terminal = TerminalClientFactory.GetInstance(comPort);
            IsStarted = false;
        }

        public void Start()
        {
            try
            {
                _listener.Start();
                if (!_terminal.Connect())
                {
                    System.Diagnostics.Debug.WriteLine("Connect to terminal failed:" + _terminal.Title);
                    return;
                }
                IsStarted = true;
                System.Diagnostics.Debug.WriteLine("Start {0} for port {1}", _listener.LocalEndpoint,_terminal.Title);

                while (true)
                {
                    var clientSocket = _listener.AcceptSocket();
                    var networkStream = new NetworkStream(clientSocket);
                    //sr = new StreamReader(ns);
                    var streamWriter = new StreamWriter(networkStream);
                    try
                    {
                        _terminal.DataReceivedEvent += TerminalClientDataReceivedEvent;
                        _logBuffer.Clear();
                        while (clientSocket.Connected)
                        {
                            if (_logBuffer.Length > 0)
                            {
                                streamWriter.Write(DateTime.Now.ToString(CultureInfo.InvariantCulture) + " : " +
                                    _logBuffer);
                                _logBuffer.Clear();
                                streamWriter.Flush();
                            }
                            Thread.Sleep(1000);
                        }
                        //_terminal.DataReceivedEvent -= TerminalClientDataReceivedEvent;
                    }
                    catch (OutOfMemoryException ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Problem occur while reading stream:-" + ex.Message);
                        System.Diagnostics.Debug.WriteLine("MyTCPTestServerService is Stopping");
                        //OnStop();
                    }
                    catch (IOException ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Client Connection Stopped:-" + ex.Message);
                    }
                }
            }
            catch(SocketException ex)
            {
                System.Diagnostics.Debug.WriteLine("socket error:" + ex.Message);
            }
        }

        private void TerminalClientDataReceivedEvent(string data)
        {
            _logBuffer.Append(data);
        }

        private readonly TcpListener _listener;

        private readonly ITerminalClient _terminal;

        private readonly StringBuilder _logBuffer = new StringBuilder();
    }
}
