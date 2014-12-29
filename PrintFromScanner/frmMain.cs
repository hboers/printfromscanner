
/*  Program Name   :  PrintFromScanner
 *  Author         :  Robert Richardson
 *  Date           :  Decemeber 2014
 *  Description    :  Monitors folder for new files.  When a new .pdf appears in the folder, the program will send the 
 *                    pdf to the default printer. 
*/


using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace PrintFromScanner
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void InitializeWatcher()
        {
            WriteEvent(String.Format("Monitoring Folder {0}.", Properties.Settings.Default.ScanFolder));
            FileSystemWatcher fsWatch = new FileSystemWatcher(Properties.Settings.Default.ScanFolder, "*.pdf");
            fsWatch.EnableRaisingEvents = true;
            fsWatch.Created += fsWatch_Created; 
        }

        private void SendToPrinter(string FilePath)
        {
            try
            {
                WriteEvent(String.Format("Sending File To Printer {0}.",FilePath));
                ProcessStartInfo info = new ProcessStartInfo();
                info.Verb = "print";
                info.FileName = FilePath;
                info.CreateNoWindow = true;
                info.WindowStyle = ProcessWindowStyle.Hidden;

                Process p = new Process();
                p.StartInfo = info;
                p.Start();               
            }
            catch(Exception ex)
            {
                WriteEvent(ex.ToString());
            }
        }       

        private void fsWatch_Created(object sender, FileSystemEventArgs e)
        {          
            SendToPrinter(e.FullPath);
        }        

        private void WriteEvent(string msg)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(txtEvents.AppendText), new object[] { msg });
                return;
            }
                    
            txtEvents.AppendText(String.Format("{0}: {1}\n", DateTime.Now.ToShortTimeString(), msg));           
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            DialogResult result;
            
            result = folder.ShowDialog();

            if(result == DialogResult.OK)
            {
                Properties.Settings.Default.ScanFolder = folder.SelectedPath;
                Properties.Settings.Default.Save();
                txtFolder.Refresh();                              
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult result;

            result = MessageBox.Show("Are you sure you want to exit?", "Exit Program", MessageBoxButtons.YesNo);

            if(result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
       
        private void btnStart_Click(object sender, EventArgs e)
        {            
            if (Directory.Exists(Properties.Settings.Default.ScanFolder))
            {
                DialogResult result;
                result = MessageBox.Show("Are you sure you want to start active monitoring?  Any PDF files created in the monitored folder will be sent to the printer.", 
                                         "Start Monitor",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    InitializeWatcher();
                }
            }
            else
            {
                MessageBox.Show("You must select a folder to monitor!", "Select Folder!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnPrintAll_Click(object sender, EventArgs e)
        {
            DialogResult result;

            result = MessageBox.Show("Are you sure you want to print all the .PDF files in the selected folder?",
                                     "Print All Files", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (Directory.Exists(Properties.Settings.Default.ScanFolder))
                {
                    string[] Files = Directory.GetFiles(Properties.Settings.Default.ScanFolder, "*.pdf");

                    if (Files.Length > 0)
                    {
                        WriteEvent("Printing All Files.");

                        foreach (string s in Files)
                        {
                            SendToPrinter(s);
                        }
                    }
                    else
                    {
                        WriteEvent("No .PDF files were found in the folder.");
                    }
                }
            }

        }
    }
}
