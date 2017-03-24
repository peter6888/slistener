using Slistener.TcpSerial.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpSerialServiceConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new TcpSerialService();
            service.Start();
            Console.ReadLine();
        }
    }
}
