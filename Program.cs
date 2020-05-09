using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CompressH265 {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
     
            foreach(var item in args)
            {
                Console.WriteLine(item);

                if (item == "-contextmenu") {
                    installContextMenu();
                    System.Environment.Exit(1);
                }
                    
            }
            
            // ***this line is added***
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            
            
        }
        
        // ***also dllimport of that function***
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
        
        
        public static void installContextMenu() {
            RegistryKey item = null;
            try {
                item = Registry.ClassesRoot.CreateSubKey("*\\shell\\Compress to H.265 (HEVC)\\command");
                item.SetValue("", System.Reflection.Assembly.GetExecutingAssembly().Location + " -i \"%1\"");
            }
            catch(Exception ex)
            {
                
            }
            finally       
            {
                if(item != null)
                    item.Close();
            }        
        }
    }
}