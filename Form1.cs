using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompressH265 {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
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
            Process process = new Process();
            process.EnableRaisingEvents = true;
            //   process.StartInfo.RedirectStandardOutput = true;
            //    process.StartInfo.RedirectStandardError = true;

            var outputFilename = textBox1.Text + ".h265.mp4";
        
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/K ffmpeg.exe -i \"{textBox1.Text}\" -vcodec hevc  \"{outputFilename}\"";
            process.StartInfo.UseShellExecute = false;
            // process.StartInfo.CreateNoWindow = true;
            
            //  process.OutputDataReceived += OnProcessOutput;
            //  process.ErrorDataReceived += OnProcessOutput;
            process.Start();
   
            // process.BeginErrorReadLine();
            // process.BeginOutputReadLine();
        }

        private void OnProcessOutput(object send, DataReceivedEventArgs args) {
         //   textBox2.Text += args.Data;
        }
    }
}