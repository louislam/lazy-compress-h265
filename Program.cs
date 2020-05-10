using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CompressH265 {
    static class Program {
        private static string subkey = "*\\shell\\Compress to H.265 (HEVC)";
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (path != null) {
                Environment.CurrentDirectory = path;
            }
        

            foreach(var item in args)
            {
                Console.WriteLine(item);

                if (item == "-contextmenu") {
                    installContextMenu();
                    System.Environment.Exit(1);
                } else if (item == "-uninstallcontextmenu") {
                    uninstallContextMenu();
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
                item = Registry.ClassesRoot.CreateSubKey(subkey + "\\command");
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

        public static void uninstallContextMenu() {
            Registry.ClassesRoot.DeleteSubKeyTree(subkey);
        }
    }
}