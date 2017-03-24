using System;
using System.Windows.Forms;

namespace SListener
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string[] args = Environment.GetCommandLineArgs();
            if(args.Length > 1)
            {
                Application.Run(new Form1(args[1],args[2]));
            }
            else
            {
                Application.Run(new Form1());
            }
        }
    }
}