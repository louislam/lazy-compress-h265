using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CompressH265 {
    public partial class Form1 : Form {

        private bool isFromContextMenu = false;
        private bool installed = false;
        
        public Form1() {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e) {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Text = Text + " v" + version.Major + "." + version.Minor;
            
            AddShieldToButton(buttonContextMenu);
    
            string[] args = Environment.GetCommandLineArgs();        

            var isInputFilePath = false;
            foreach (var arg in args) {
                if (arg == "-i") {
                    isInputFilePath = true;
                } else if (isInputFilePath) {
                    isFromContextMenu = true;
                    textBox1.Text = arg;
                    isInputFilePath = false;
                    button2_Click(null, null);
                }
            }

            UpdateContextMenuButton();
        }

        public void UpdateContextMenuButton() {
            var a = Registry.ClassesRoot.OpenSubKey("*\\shell\\Compress to H.265 (HEVC)");

            if (a == null) {
                buttonContextMenu.Text = "Install to Context Menu";
                installed = false;
            } else {
                buttonContextMenu.Text = "Uninstall from Context Menu";
                installed = true;
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e) {
            textBox1.Text = openFileDialog1.FileName;
        }


        private void Form1_DragDrop(object sender, DragEventArgs e) {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            textBox1.Text = fileList[0];
        }

        private void Form1_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void button2_Click(object sender, EventArgs e) {

            if (textBox1.Text.Trim() == "") {
                MessageBox.Show("Please select or drag-and-drop a file.");
                return;
            }
            
            if (!File.Exists(textBox1.Text)) {
                MessageBox.Show("File Not Found");
                return;
            }
            
            Process process = new Process();
            process.EnableRaisingEvents = true;
            //   process.StartInfo.RedirectStandardOutput = true;
            //    process.StartInfo.RedirectStandardError = true;

            var outputFilename = textBox1.Text + ".h265.mp4";
        
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/K ffmpeg.exe -i \"{textBox1.Text}\" -vcodec hevc -map_metadata 0 \"{outputFilename}\"";
            process.StartInfo.UseShellExecute = false;
            // process.StartInfo.CreateNoWindow = true;

            process.Exited += (sender2, e2) => {
                System.Environment.Exit(1);
            };
            
            //  process.OutputDataReceived += OnProcessOutput;
            //  process.ErrorDataReceived += OnProcessOutput;
            process.Start();
   
            // process.BeginErrorReadLine();
            // process.BeginOutputReadLine();
        }

        private void OnProcessOutput(object send, DataReceivedEventArgs args) {
         //   textBox2.Text += args.Data;
        }

        private void button3_Click(object sender, EventArgs e) {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            info.UseShellExecute = true;
            info.Verb = "runas"; // Provides Run as Administrator

            if (installed) {
                info.Arguments = "-uninstallcontextmenu";
            } else {
                info.Arguments = "-contextmenu";
            }

            try {
                if (Process.Start(info) != null) {
                    MessageBox.Show("Installed successfully.");
                    UpdateContextMenuButton();
                } else {
                    MessageBox.Show("Error");
                }
            } catch (Exception exception) {
                MessageBox.Show("Error: You have to allow UAC in order to install to the context menu.");
            }

        }
        
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
            uint Msg, int wParam, int lParam);

// Make the button display the UAC shield.
        public static void AddShieldToButton(Button btn)
        {
            const Int32 BCM_SETSHIELD = 0x160C;

            // Give the button the flat style and make it
            // display the UAC shield.
            btn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            SendMessage(btn.Handle, BCM_SETSHIELD, 0, 1);
        }


        private void Form1_Shown(object sender, EventArgs e) {
            if (isFromContextMenu) {
                Hide();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/louislam/lazy-compress-h265");
        }
    }
}