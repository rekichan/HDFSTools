using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using Microsoft.Hadoop.WebHDFS;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace HDFSTools
{
    public partial class frm_Main : Form
    {

        #region Properties
        private cls_HDFS HDFS;
        private cls_Config config;
        private bool linkStatus;
        private bool initTaskFlag = true;
        private object lockObject;
        private ImageList smallImageList;
        private ImageList largeImageList;
        #endregion

        #region API
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void PostMessage(IntPtr hWnd, int msg, IntPtr wParam, int lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        #endregion

        #region Constructor
        public frm_Main()
        {
            InitializeComponent();
        }
        #endregion

        #region FormEvent
        private void frm_Main_Load(object sender, EventArgs e)
        {
            //实例化锁对象
            lockObject = new object();

            //获取窗口句柄
            cls_Msg.hwndFrmMain = this.Handle;

            //实例化config
            InitConfig();

            //实例化HDFS
            HDFS = cls_HDFS.getInstance();
            InitHDFS();

            //实例化ImageList并插入图片
            InitImageList();
        }
        #endregion

        #region Event
        private void lv_ShowFile_MouseDoubleClick(object sender, MouseEventArgs e)
        {


            if (lv_ShowFile.SelectedItems.Count <= 0)
                return;

            string currentPath = tstb_CurrentPath.Text;
            if (!currentPath.EndsWith("/"))
                currentPath += "/";
            string info = lv_ShowFile.SelectedItems[0].Tag.ToString();
            string[] infos = Regex.Split(info, "~>");
            if ("FILE".Equals(infos[6]))
                return;

            string newPath = currentPath + infos[0] + "/";
            tstb_CurrentPath.Text = currentPath + infos[0];

            lv_ShowFile.Clear();
            lv_ShowFile.AllowColumnReorder = true;

            lv_ShowFile.Columns.Add("name", "name", 250);
            lv_ShowFile.Columns.Add("size", "size", 100);
            lv_ShowFile.Columns.Add("permission", "permission", 100);
            lv_ShowFile.Columns.Add("owner", "owner", 100);
            lv_ShowFile.Columns.Add("group", "group", 100);
            lv_ShowFile.Columns.Add("replication", "replication", lv_ShowFile.Width - 650);

            HDFS.client.GetDirectoryStatus(newPath)
                .ContinueWith(ds =>
                {
                    ds.Result.Entries.ToList()
                    .ForEach(f =>
                    {
                        string permission = f.Permission;
                        string owner = f.Owner;
                        string group = f.Group;
                        string size = f.Length.ToString();
                        string replication = f.Replication.ToString();
                        string name = f.PathSuffix;
                        string type = f.Type;
                        StringBuilder sb = new StringBuilder();
                        sb.Append(name)
                        .Append("~>")
                        .Append(size)
                        .Append("~>")
                        .Append(permission)
                        .Append("~>")
                        .Append(owner)
                        .Append("~>")
                        .Append(group)
                        .Append("~>")
                        .Append(replication)
                        .Append("~>")
                        .Append(type);
                        /*.Append("~>")
                        .Append(tstb_CurrentPath.Text);*/

                        PostMess(cls_Msg.hwndFrmMain, cls_Msg.LIST_DIRECTORIES_AND_FILES, sb.ToString());
                    });
                });
        }

        private void tv_FolderList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (initTaskFlag)
                return;

            string currentPath = tv_FolderList.SelectedNode.FullPath;
            lv_ShowFile.Clear();
            lv_ShowFile.AllowColumnReorder = true;

            tstb_CurrentPath.Text = currentPath;

            lv_ShowFile.Columns.Add("name", "name", 250);
            lv_ShowFile.Columns.Add("size", "size", 100);
            lv_ShowFile.Columns.Add("permission", "permission", 100);
            lv_ShowFile.Columns.Add("owner", "owner", 100);
            lv_ShowFile.Columns.Add("group", "group", 100);
            lv_ShowFile.Columns.Add("replication", "replication", lv_ShowFile.Width - 650);

            HDFS.client.GetDirectoryStatus(tstb_CurrentPath.Text)
                .ContinueWith(ds =>
                {
                    ds.Result.Entries.ToList()
                    .ForEach(f =>
                    {
                        string permission = f.Permission;
                        string owner = f.Owner;
                        string group = f.Group;
                        string size = f.Length.ToString();
                        string replication = f.Replication.ToString();
                        string name = f.PathSuffix;
                        string type = f.Type;
                        StringBuilder sb = new StringBuilder();
                        sb.Append(name)
                        .Append("~>")
                        .Append(size)
                        .Append("~>")
                        .Append(permission)
                        .Append("~>")
                        .Append(owner)
                        .Append("~>")
                        .Append(group)
                        .Append("~>")
                        .Append(replication)
                        .Append("~>")
                        .Append(type);
                        /*.Append("~>")
                        .Append(tstb_CurrentPath.Text);*/

                        PostMess(cls_Msg.hwndFrmMain, cls_Msg.LIST_DIRECTORIES_AND_FILES, sb.ToString());
                    });
                });
        }

        private void tsmi_BigIcon_Click(object sender, EventArgs e)
        {
            lv_ShowFile.View = View.LargeIcon;
        }

        private void tsmi_SmallIcon_Click(object sender, EventArgs e)
        {
            lv_ShowFile.View = View.SmallIcon;
        }

        private void tsmi_TailIcon_Click(object sender, EventArgs e)
        {
            lv_ShowFile.View = View.Tile;
        }

        private void tsmi_ListIcon_Click(object sender, EventArgs e)
        {
            lv_ShowFile.View = View.List;
        }

        private void tsmi_DetailIcon_Click(object sender, EventArgs e)
        {
            lv_ShowFile.View = View.Details;
        }

        private void tsmi_ConnectConfig_Click(object sender, EventArgs e)
        {
            frm_Config fc = new frm_Config();
            fc.ShowDialog();
        }

        private void tsmi_Connect_Click(object sender, EventArgs e)
        {
            if (linkStatus || initTaskFlag)
                return;

            InitHDFS();
        }

        private void tsb_Forward_Click(object sender, EventArgs e)
        {

        }

        private void tsb_Backward_Click(object sender, EventArgs e)
        {

        }
        #endregion

        #region Function
        /// <summary>
        /// 递归获取Tree父节点
        /// </summary>
        /// <param name="tn">节点</param>
        /// <returns></returns>
        private TreeNode GetParentNode(TreeNode tn)
        {
            if (tn.Parent == null)
                return tn;
            else
                return GetParentNode(tn.Parent);
        }

        /// <summary>
        /// 获取一级树节点
        /// </summary>
        /// <param name="client"></param>
        /// <param name="tn"></param>
        /// <param name="directory"></param>
        private void GetFirstDirectory(WebHDFSClient client, TreeNode tn, string directory)
        {
            client.GetDirectoryStatus(directory)
                .ContinueWith(ds =>
                {
                    ds.Result.Directories.ToList()
                    .ForEach(f =>
                    {
                        PostMess(cls_Msg.hwndFrmMain, cls_Msg.LIST_DIRECTORIES, f.PathSuffix);
                    });
                });
        }

        /// <summary>
        /// 获取二级树节点
        /// </summary>
        /// <param name="client"></param>
        /// <param name="directory"></param>
        private void GetSecondDirectory(WebHDFSClient client, string directory)
        {

        }

        /// <summary>
        /// 初始化HDFS
        /// </summary>
        private void InitHDFS()
        {
            tv_FolderList.Nodes.Clear();

            Uri myUri = new Uri("http://" + cls_Config.host + ":" + cls_Config.port + "/");
            HDFS.client = new WebHDFSClient(myUri, cls_Config.userName);
            tv_FolderList.Nodes.Add("/");
            HDFS.client.GetDirectoryStatus("/")
                .ContinueWith(ds =>
                {
                    try
                    {
                        linkStatus = true;
                        ds.Result.Info.ToString();
                        PostMess(cls_Msg.hwndFrmMain, cls_Msg.MAIN_UI_ENABLE);
                        ds.Result.Directories.ToList()
                        .ForEach(f =>
                        {
                            PostMess(cls_Msg.hwndFrmMain, cls_Msg.LIST_DIRECTORIES, f.PathSuffix);
                        });
                    }
                    catch
                    {
                        linkStatus = false;
                        PostMess(cls_Msg.hwndFrmMain, cls_Msg.MAIN_UI_DISABLE);
                        MessageBox.Show("HDFS连接失败!", "Err", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        initTaskFlag = false;
                    }
                });
        }

        /// <summary>
        /// 初始化Treeview ImageList
        /// </summary>
        public void InitImageList()
        {
            smallImageList = new ImageList();
            smallImageList.ImageSize = new Size(20, 20);
            smallImageList.Images.Add("folder", Properties.Resources.FileFolder);
            smallImageList.Images.Add("file", Properties.Resources.newfile);

            largeImageList = new ImageList();
            largeImageList.ImageSize = new Size(32, 32);
            largeImageList.Images.Add("folder", Properties.Resources.FileFolder);
            largeImageList.Images.Add("file", Properties.Resources.newfile);

            tv_FolderList.ImageList = smallImageList;

            lv_ShowFile.LargeImageList = largeImageList;
            lv_ShowFile.SmallImageList = smallImageList;
        }

        /// <summary>
        /// 初始化配置参数
        /// </summary>
        private void InitConfig()
        {
            string path = Application.StartupPath + @"\parameter\config.ini";
            config = cls_Config.getInstance(path);

            cls_Config.port = config.IniReadValue("config", "port", "9870");
            cls_Config.host = config.IniReadValue("config", "host", "localhost");
            cls_Config.userName = config.IniReadValue("config", "userName", "user");
        }

        /// <summary>
        /// 主界面UI使能
        /// </summary>
        /// <param name="enable">使能</param>
        public void RefreshUIEnable(bool enable = false)
        {
            tsb_Backward.Enabled = enable;
            tsb_Forward.Enabled = enable;
            tsb_Refresh.Enabled = enable;
            tsb_Enter.Enabled = enable;
            tsb_Search.Enabled = enable;

            tssb_File.Enabled = enable;

            tstb_CurrentPath.Enabled = enable;
            tstb_Search.Enabled = enable;

            tv_FolderList.Enabled = enable;

            lv_ShowFile.Enabled = enable;
        }

        /*/// <summary>
        /// PostMessage方法发送消息，带穿节点的内容
        /// </summary>
        /// <param name="Handle">句柄</param>
        /// <param name="msg">发送的消息</param>
        /// <param name="tn">发送的节点</param>
        public void PostMess(IntPtr Handle,int msg,TreeNode tn)
        {
            if (tn == null || tn.Parent == null)
                return;

            GCHandle tnHandle = GCHandle.Alloc(tn);
            IntPtr p = GCHandle.ToIntPtr(tnHandle);

            if (Handle != IntPtr.Zero)
            {
                PostMessage(Handle, msg, p, 0);
            }
            //tnHandle.Free(); //这种方法在释放GCHandle的时候会出现问题
        }*/

        /// <summary>
        /// PostMessage方法发送消息，带穿字符串内容
        /// </summary>
        /// <param name="Handle">目标句柄</param>
        /// <param name="msg">发送的消息</param>
        /// <param name="wParam">发送的内容</param>
        public void PostMess(IntPtr Handle, int msg, string wParam)
        {
            IntPtr p = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(wParam);

            if (Handle != IntPtr.Zero)
            {
                PostMessage(Handle, msg, p, 0);
            }
        }

        /// <summary>
        /// PostMessage方法发送消息
        /// </summary>
        /// <param name="handle">目标句柄</param>
        /// <param name="msg">发送的消息</param>
        public void PostMess(IntPtr handle, int msg)
        {
            if (handle != IntPtr.Zero)
            {
                PostMessage(handle, msg, 0, 0);
            }
        }
        #endregion

        #region Message
        /// <summary>
        /// 处理句柄消息
        /// </summary>
        /// <param name="m">消息</param>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case cls_Msg.MAIN_UI_ENABLE:
                    RefreshUIEnable(true);
                    break;

                case cls_Msg.MAIN_UI_DISABLE:
                    RefreshUIEnable();
                    break;

                case cls_Msg.LIST_DIRECTORIES:
                    lock (lockObject)
                    {
                        string text = Marshal.PtrToStringAnsi(m.WParam);
                        TreeNode tn = new TreeNode(text);
                        tn.ImageIndex = smallImageList.Images.IndexOfKey("folder");
                        tn.SelectedImageIndex = smallImageList.Images.IndexOfKey("folder");
                        tv_FolderList.Nodes[0].Nodes.Add(tn);
                        Marshal.FreeHGlobal(m.WParam);
                    }
                    break;

                case cls_Msg.LIST_DIRECTORIES_AND_FILES:
                    lock (lockObject)
                    {
                        string info = Marshal.PtrToStringAnsi(m.WParam);
                        string[] infos = Regex.Split(info, "~>");
                        ListViewItem item = new ListViewItem();
                        item.Text = infos[0];
                        item.Tag = info;
                        if ("DIRECTORY".Equals(infos[6]))
                            item.ImageIndex = smallImageList.Images.IndexOfKey("folder");
                        else
                            item.ImageIndex = smallImageList.Images.IndexOfKey("file");
                        item.SubItems.Add(infos[1] + "B");
                        item.SubItems.Add(infos[2]);
                        item.SubItems.Add(infos[3]);
                        item.SubItems.Add(infos[4]);
                        item.SubItems.Add(infos[5]);
                        lv_ShowFile.Items.Add(item);
                        Marshal.FreeHGlobal(m.WParam);
                    }
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }

            /*GCHandle tnHandle = GCHandle.FromIntPtr(m.WParam);
                    TreeNode tn = (TreeNode)tnHandle.Target;
                    MessageBox.Show(tn.Text);
                    tnHandle.Free();*/
            /*string s = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(m.WParam);
            MessageBox.Show(s);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(m.WParam);*/
        }
        #endregion

    }
}
