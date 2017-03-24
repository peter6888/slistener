using System.ServiceProcess;
using Slistener.TcpSerial.Service;

namespace TcpSerialWin32Service
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var service = new TcpSerialService();
            service.Start();
        }

        protected override void OnStop()
        {
        }
    }
}
