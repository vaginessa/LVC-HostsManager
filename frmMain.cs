﻿/*  This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.If not, see<http://www.gnu.org/licenses/>.

    Idea:   Tobias B. Besemer
    Coding: Dennis M. Heine
    
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HostsManager
{
    public partial class frmMain : Form
    {
        private String fileText = "";
        public String ipFrom = "0.0.0.0";
        public String ipTo = "0.0.0.0";
        private String hostsURL = "https://raw.githubusercontent.com/StevenBlack/hosts/master/hosts";

        public frmMain()
        {            
            InitializeComponent();
            loadSettings();
        }

        private bool isAntivir()
        {
            if (Process.GetProcessesByName("avgnt").Length > 0 || Process.GetProcessesByName("inststub").Length > 0 || Process.GetProcessesByName("uiStub").Length > 0 || Process.GetProcessesByName("KLAgent").Length > 0 || Process.GetProcessesByName("vsserv").Length > 0 || Process.GetProcessesByName("VisthAux").Length > 0 || Process.GetProcessesByName("avastui").Length > 0)
                return true;
            else
                return false;
        }

        private void loadSettings()
        {
            Microsoft.Win32.RegistryKey mexampleRegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("HostsManager");
            if (mexampleRegistryKey != null)
            {
                hostsURL = (String)mexampleRegistryKey.GetValue("URL");
                if (hostsURL == null)
                    hostsURL = "";
                ipFrom = (String)mexampleRegistryKey.GetValue("ipFrom");
                if (ipFrom == null)
                    ipFrom = "";
                ipTo = (String)mexampleRegistryKey.GetValue("ipTo");
                if (ipTo == null)
                    ipTo = "";
            }
        }

        private void saveSettings()
        {
            Microsoft.Win32.RegistryKey exampleRegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("HostsManager");
            exampleRegistryKey.SetValue("URL", hostsURL);
            exampleRegistryKey.SetValue("ipFrom", ipFrom);
            exampleRegistryKey.SetValue("ipTo", ipTo);
            exampleRegistryKey.Close();
        }

        private void updateHostsFile()
        {
            try
            {
                toolStripProgressBar1.Visible = true;
                SecurityIdentifier id = new SecurityIdentifier("S-1-5-32-544");
                string adminGroupName = id.Translate(typeof(NTAccount)).Value;

                System.Net.WebClient wc = new System.Net.WebClient();
                if (fileText == "")
                    fileText = wc.DownloadString(hostsURL);  // wc.DownloadFile(hostsURL, "hosts.tmp");                    
                fileText = fileText.Replace(ipTo, ipFrom);
                System.IO.File.Delete("hosts.tmp");
                System.IO.File.WriteAllText("hosts.tmp", fileText);

                FileSecurity fs = System.IO.File.GetAccessControl(Environment.GetEnvironmentVariable("windir") + "\\system32\\drivers\\etc\\hosts");
                fs.AddAccessRule(new FileSystemAccessRule(adminGroupName, FileSystemRights.FullControl, AccessControlType.Allow));
                fs.RemoveAccessRule(new FileSystemAccessRule(adminGroupName, FileSystemRights.Write, AccessControlType.Deny));
                System.IO.File.SetAccessControl(Environment.GetEnvironmentVariable("windir") + "\\system32\\drivers\\etc\\hosts", fs);

                System.IO.File.Copy("hosts.tmp", Environment.GetEnvironmentVariable("windir") + "\\system32\\drivers\\etc\\hosts", true);
                System.IO.File.Delete("hosts.tmp");                

                fs.RemoveAccessRule(new FileSystemAccessRule(adminGroupName, FileSystemRights.FullControl, AccessControlType.Allow));
                fs.AddAccessRule(new FileSystemAccessRule(adminGroupName, FileSystemRights.Write, AccessControlType.Deny));
                toolStripProgressBar1.Visible = false;
                MessageBox.Show("Hosts file updated.");

            }
            catch (Exception ex)
            {
                String add = "";
                if (isAntivir())
                    add = "Antivirus found. Please disable it during hosts file update.\nRead the manual for further informations.\n";
                MessageBox.Show("Error: " + add + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            updateHostsFile();               
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bnUpdate.Select();            
            if (isAntivir())  
                MessageBox.Show("Antivirus found. Please disable it during hosts file update.\nRead the manual for further informations.\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
         
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updateHostsFile();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmOptions o = new frmOptions();
            o.fileText = fileText;
            o.url = hostsURL;            
            o.convFrom = ipFrom;
            o.convTo = ipTo;
            o.ShowDialog();
            if(o.DialogResult==DialogResult.OK)
            {
                if (o.fileText != "")
                    fileText = o.fileText;
                if (o.convTo != "")
                    ipTo = o.convTo;
                if (o.convFrom != "")
                    ipFrom = o.convFrom;
                if (o.url != "")
                    hostsURL = o.url;
                saveSettings();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            Application.Exit();
        }
    }
}
