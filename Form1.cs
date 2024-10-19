using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace myPlayer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }

        private void vlcControl1_VlcLibDirectoryNeeded(object sender, Vlc.DotNet.Forms.VlcLibDirectoryNeededEventArgs e)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;

            if (currentDirectory == null)
                return;
            if (IntPtr.Size == 4)
                e.VlcLibDirectory = new DirectoryInfo(Path.GetFullPath(@".\libvlc\win-x86\"));
            else
                e.VlcLibDirectory = new DirectoryInfo(Path.GetFullPath(@".\libvlc\win-x64\"));

            if (!e.VlcLibDirectory.Exists)
            {
                var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
                folderBrowserDialog.Description = "Select Vlc libraries folder.";
                folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
                folderBrowserDialog.ShowNewFolderButton = true;
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    e.VlcLibDirectory = new DirectoryInfo(folderBrowserDialog.SelectedPath);
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            WebClient client = new WebClient();
            client.DownloadFile("http://192.168.100.1:35455/tv.m3u", "tv.m3u");
            ParseM3U("tv.m3u");
            //vlcControl1.Play("http://192.168.100.1:35455/itv/1000000001000023315.m3u8?cdn=ystenlive");
        }
        private void ParseM3U(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var channelGroups = new Dictionary<string, TreeNode>();

            TreeNode currentGroup = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("#EXTINF"))
                {
                    var name = line.Substring(line.IndexOf(',') + 1);
                    currentGroup = new TreeNode { Text = name };
                    channelGroups[name] = currentGroup;
                    treeView1.Nodes.Add(currentGroup);
                }
                else if (!string.IsNullOrWhiteSpace(line) && Uri.IsWellFormedUriString(line, UriKind.Absolute))
                {
                    if (currentGroup != null)
                    {
                        currentGroup.Nodes.Add(line);
                    }
                }
                else if (line.StartsWith("#EXTM3U"))
                {
                    // Ignore this line
                }
                else
                {
                    currentGroup = null;
                }
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Text.StartsWith("http"))
            {
                vlcControl1.Play(e.Node.Text);
                vlcControl1.Video.FullScreen = true;
            }
        }
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public static IntPtr HWND_TOPMOST = new IntPtr(-1);
        public enum SWP : uint
        {
            NOMOVE = 0x0001,
            NOSIZE = 0x0002,
        }

        bool fullScreen = true;
        Rectangle resolution;
        public Screen[] screens = Screen.AllScreens;
        private void vlcControl1_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                vlcControl1.Parent = splitContainer1.Panel2;
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                //vlcControl1.Dock = DockStyle.Fill;
                vlcControl1.Width = splitContainer1.Panel2.Width;
                vlcControl1.Height = splitContainer1.Panel2.Height;
            }
            else
            {
                vlcControl1.Parent = this.FindForm();
                resolution = screens[0].Bounds;
                vlcControl1.Location = new Point(0, 0);
                TopMost = true;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                vlcControl1.Width = resolution.Width;
                vlcControl1.Height = resolution.Height;
                vlcControl1.BringToFront();

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            vlcControl1.Video.IsMouseInputEnabled = false;
            vlcControl1.Video.IsKeyInputEnabled = false;// 这行代码最重要
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

            vlcControl1.Play("http://192.168.100.1:35455/itv/1000000001000023315.m3u8?cdn=ystenlive");
            vlcControl1.Video.Teletext = 1;
        }

        private void vlcControl1_Click(object sender, EventArgs e)
        {
            if (vlcControl1.IsPlaying)
            { 
                vlcControl1.Pause();
                
            }else
            {
                vlcControl1.Play();
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            new Task(() =>
            {
                this.vlcControl1.Stop();//这里要开线程处理，不然会阻塞播放

            }).Start();
        }

        

    }
}
