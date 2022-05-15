using Newtonsoft.Json;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Principal;
using System.Collections.Generic;

namespace GWSL
{
    public partial class Form1 : Form
    {
        WSL manager = null;
        Form2 handler = null;
        Distro currOs = null;
        [DllImport(@"gwslgo.dll", EntryPoint = "generateOutput", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr generateOutput();
        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            setupSettings();
            updateWSLReport();
        }

        private void setupSettings()
        {
            if (!System.IO.File.Exists("gwslgo.dll"))
            {
                MessageBox.Show("Missing gwslgo.dll, copy the main.dll in go folder and put it in the same folder as the exe and rename it to   gwslgo.dll  ... exiting...", "WSL Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            textBox1.Text = @"C:\Users\" + System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split("\\")[1];

        }
        private void checkForWSL()
        {
            if (manager != null)
            {
                bool isElevated;
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
                }

                if (manager.getReady() == false)
                {
                    if (MessageBox.Show("WSL is not installed, Do you want to install it?", "WSL Missing", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        if (isElevated)
                        {
                            runCommand("dism.exe /online /enable-feature /featurename:Microsoft-Windows-Subsystem-Linux /all /norestart & wsl --install", "Installer", "Installing WSL....", true);

                        }
                        else
                        {
                            MessageBox.Show("Need to run as admin to install... exiting...", "WSL Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Need WSL to use this program... exiting...", "WSL Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }
                }
                if (manager.getVersion() == false)
                {
                    if (MessageBox.Show("WSL2 is not installed, Do you want to install it?", "WSL2 Missing", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        if (isElevated)
                        {
                            runCommand("dism.exe /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart & wsl --update", "Installer", "Installing WSL2....", true);

                        }
                        else
                        {
                            MessageBox.Show("Need to run as admin to install... exiting...", "WSL Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Need WSL2 to use this program... exiting...", "WSL Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }
                }
            }
        }

        private void runCommand(string cmd, string title, string disc, bool showPop)
        {
            this.Enabled = false;
            if (showPop)
            {
                handler = new Form2();
                handler.Show(title, disc);
            }
            backgroundWorker1.RunWorkerAsync(argument: cmd);
        }

        private void updateWSLReport()
        {
            listView1.Items.Clear();
            listView1.Items.Add("LOADING....");
            hideButtons(true);
            listView1.Update();
            listView1.Refresh();
            backgroundWorker2.RunWorkerAsync();
        }
        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string input = (string)e.Argument;
            BackgroundWorker worker = sender as BackgroundWorker;

            Process process = new Process();
            process.StartInfo.FileName = "cmd";
            process.StartInfo.Arguments = "/c " + input;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            while (!process.HasExited)
            {

                worker.ReportProgress(1);
                System.Threading.Thread.Sleep(500);
            }
            //process.BeginOutputReadLine();            
            //process.WaitForExit();
            process.Close();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (handler != null) {
                handler.updateProgress();
                handler.Focus();
            }

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(handler != null)
            {
                handler.completeTask();
                handler.Refresh();
                handler.Update();
                handler.Close();
                handler = null;
            }           
            this.Enabled = true;
            this.Focus();
            updateWSLReport();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            currOs = (Distro)listView1.SelectedItems[0].Tag;
            if (listView1.SelectedItems[0].Tag == null)
            {
                hideButtons(true);
            }
            else if (currOs.installed)
            {
                SetInstalledPage();
            }
            else
            {
                SetDownloadPage();
            }

        }
        private void SetDownloadPage()
        {
            button1.Text = "Install";
            label1.Text = currOs.name;
            hideButtons(true);
            showButtons(false);
        }
        private void SetInstalledPage()
        {
            button1.Text = "Set Defualt";
            label1.Text = (currOs.defualt ? "(Defualt) " : "") + currOs.name;             
            hideButtons(true);      
            showButtons(true);                
        }
        private void hideButtons(bool all)
        {
            if (all)
            {
                label1.Visible = false;
                button1.Visible = false;
            }
            label2.Visible = false;
            button3.Visible = false;
            label3.Visible = false;
            button10.Visible = false;
            label4.Visible = false;
            textBox1.Visible = false;
        }
        private void showButtons(bool all)
        {
            label1.Visible = true;
            button1.Visible = true;
            if (all)
            {
                label2.Visible = true;
                button3.Visible = true;
                label3.Visible = true;
                button10.Visible = true;
                label4.Visible = true;
                textBox1.Visible = true;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (currOs != null)
                if (button1.Text == "Install")
                {
                    runCommand("wsl --install -d " + currOs.cmdName, "Installer", "Uninstalling " + currOs.name + "....", true);

                }
                else
                {
                    runCommand("wsl --set-default " + currOs.cmdName, "GWSL", "Setting " + currOs.name + " as defualt....", false);
                }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            string input = (string)e.Argument;
            generateOutput();
            string data = Marshal.PtrToStringAnsi(generateOutput());
            Output rep = JsonConvert.DeserializeObject<Output>(data);
            manager = new WSL(rep, listView1);
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            manager.generateInstalled();
            manager.generateOnline();
            Update();
            Refresh();
            if (!manager.getReady())
            {
                checkForWSL();
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (currOs != null)
                runCommand("wsl --unregister " + currOs.cmdName, "Uninstaller", "Uninstalling " + currOs.name + "....", true);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (currOs != null)
                runCommand("wsl --shutdown " + currOs.cmdName, "Uninstaller", "Uninstalling " + currOs.name + "....", false);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (currOs != null)
            {
                startWSL();
                updateWSLReport();
            }                
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if(currOs!=null)
                runCommand("wsl --terminate " + currOs.cmdName, "Uninstaller", "Uninstalling " + currOs.name + "....", false);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (currOs != null)
            {
                runCommand("wsl --terminate " + currOs.cmdName, "Uninstaller", "Uninstalling " + currOs.name + "....", false);
                startWSL();
                updateWSLReport();
            }
        }

        private void startWSL()
        {
            if (currOs != null)
                if (Directory.Exists(textBox1.Text))
                {
                    System.Diagnostics.Process.Start("CMD.exe", "/C " + "wsl -d " + currOs.cmdName + " --cd " + textBox1.Text);
                }
                else
                {
                    System.Diagnostics.Process.Start("CMD.exe", "/C " + "wsl -d " + currOs.cmdName);
                }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (currOs != null)
                updateWSLReport();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "\\\\wsl.localhost\\" + currOs.cmdName);

        }
    }

    public class WSL
    {
        private ListView guiHandle;
        private Output myReport;
        private List<Distro> installed;
        public WSL(Output input, ListView gui)
        {
            guiHandle = gui;
            myReport = input;
            installed = new List<Distro>();
        }
        public void generateInstalled()
        {
            guiHandle.Items.Clear();
            guiHandle.Items.Add("INSTALLED");
            guiHandle.Items[0].BackColor = Color.Green;
            if (!myReport.Installed.Equals("error"))
            {
                myReport.Installed = myReport.Installed.Split("VERSION")[1];
                foreach (string item in myReport.Installed.Split("\n"))
                {                 
                    if (!item.Trim().Equals(""))
                    {  
                        string[] itemSafe = new Regex(@"\s+").Replace(item.Replace("*","").Trim(), "+").Split("+");                        
                        Distro tempOs = new Distro();
                        tempOs.defualt = item.Contains('*');
                        tempOs.cmdName = itemSafe[0];
                        tempOs.name = itemSafe[0];                                            
                        tempOs.running = !itemSafe[1].Equals("Stopped");
                        tempOs.version = int.Parse(itemSafe[2]);
                        tempOs.installed = true;   
                        ListViewItem temp = new ListViewItem((tempOs.defualt ? "*" : "") + tempOs.name + (tempOs.running ? " (Running)" : " (Stopped)"));
                        temp.Tag = tempOs;
                        installed.Add(tempOs);
                        guiHandle.Items.Add(temp);
                    }

                }
            }
            else
            {
                ListViewItem temp = new ListViewItem("none");
                temp.Tag = true;
                guiHandle.Items.Add(temp);
            }
        }
        public void generateOnline()
        {
            guiHandle.Items.Add("DOWNLOAD");
            guiHandle.Items[guiHandle.Items.Count - 1].BackColor = Color.Lime;
            myReport.Online = myReport.Online.Split("FRIENDLY NAME")[1];
            foreach (string item in myReport.Online.Split("\n"))
            {
                if (!item.Trim().Equals(""))
                {                    
                    string[] itemSafe = item.Split(" ");                   
                    Distro tempOs = new Distro();
                    tempOs.cmdName = itemSafe[0];
                    var regex = new Regex(Regex.Escape(item.Trim().ReplaceLineEndings().Split(" ")[0]));
                    tempOs.name = regex.Replace(item.ToString().Trim(), "", 1).Trim();
                    tempOs.installed = false;                    
                    if(!installed.Exists(x => x.cmdName == tempOs.cmdName)){
                        ListViewItem temp = new ListViewItem(tempOs.name);
                        temp.Tag = tempOs;
                        guiHandle.Items.Add(temp);
                    }                    
                }                
            }
            installed = null;
        }

        internal bool getReady()
        {
            return myReport.Ready;
        }

        internal bool getVersion()
        {
            return myReport.Wstwo;
        }
    }
    public class Distro
    {
        public bool defualt { get; set; }
        public string cmdName { get; set; }
        public string name { get; set; }
        public bool installed { get; set; }
        public int version { get; set; }
        public bool running { get; set; }
    }
    public class Output
    {
        public bool Ready { get; set; }
        public string Installed { get; set; }
        public string Online { get; set; }
        public bool Wstwo { get; set; }
    }


}