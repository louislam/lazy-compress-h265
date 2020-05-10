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
        private int nextTaskID = 0;

        private int runningProcessCount = 0;

        private LinkedList<Process> processList = new LinkedList<Process>();
        
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
            var inputFilename = textBox1.Text;
            var shortName = Path.GetFileName(inputFilename);
            
            if (inputFilename.Trim() == "") {
                MessageBox.Show("Please select or drag-and-drop a file.");
                return;
            }
            
            if (!File.Exists(inputFilename)) {
                MessageBox.Show("File Not Found");
                return;
            }

            var creationTime = File.GetCreationTime(inputFilename);
            var lastAccessTime = File.GetLastAccessTime(inputFilename);
            var lastWriteTime = File.GetLastWriteTime(inputFilename);
            
            Process process = new Process();
            process.EnableRaisingEvents = true;

            var outputFilename = inputFilename + ".h265.mp4";
            
            if (File.Exists(outputFilename)) {
                MessageBox.Show("The output file name is existing. Please move or delete it before converting. " + outputFilename);
                return;
            }
        
            process.StartInfo.FileName = "ffmpeg.exe";
            process.StartInfo.Arguments = $"-i \"{inputFilename}\" -vcodec hevc -map_metadata 0 -crf 28 -preset slow \"{outputFilename}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            var rowID = nextTaskID++;
            var msgQueue = new Queue<string>();
            dataGridView1.Rows.Insert(rowID, shortName, "Preparing...");
            var row = dataGridView1.Rows[rowID];
            
            process.StartInfo.RedirectStandardError = true;
            process.ErrorDataReceived += (errorEvent,errorArgs) => {
                if (errorArgs.Data == null || errorArgs.Data.Trim() == "") {
                    return;
                }
                
                msgQueue.Enqueue(errorArgs.Data);

                if (msgQueue.Count == 5) {
                    msgQueue.Dequeue();
                }
                
                row.Cells[1].Value = String.Join(" | ", msgQueue.Reverse());
            };
            
            process.Exited += (sender2, e2) => {
                
                if (File.Exists(outputFilename)) {
                    File.SetCreationTime(outputFilename, creationTime);
                    File.SetLastAccessTime(outputFilename, lastAccessTime);
                    File.SetLastWriteTime(outputFilename, lastWriteTime);
                }
                
                msgQueue.Enqueue("Finish");
                row.Cells[1].Value = String.Join(" | ", msgQueue.Reverse());
                
                if (isFromContextMenu) {
                    //System.Environment.Exit(1);
                }

                runningProcessCount--;
            };

            runningProcessCount++;
            process.Start();
            process.BeginErrorReadLine();

            processList.AddLast(process);
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
                //Hide();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/louislam/lazy-compress-h265");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            if (runningProcessCount > 0) {
                var result = MessageBox.Show("Are you sure want to stop the processing?", "Stop process", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes) {
                    foreach (var p in processList) {
                        p.Kill();
                    }
                } else {
                    e.Cancel = true;
                }
            }
        }
    }
}