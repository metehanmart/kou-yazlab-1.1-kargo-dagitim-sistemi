using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace yazlab1._1
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
            //Application.Run(new Form1());

            Thread thread1 = new Thread(Form1);
            Thread thread2 = new Thread(Form3);
            thread1.Start();
            thread2.Start();


        }
        public static void Form1()
        {
            Application.Run(new Form1());
        }
        public static void Form3()
        {
            Application.Run(new Form3());
        }
        

    }
}
