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
using System.Net;
using System.IO;

namespace HDFSTools
{
    public partial class frm_Main : Form
    {

        #region Properties
        private cls_HDFS HDFS;
        private cls_Config config;
        private cls_Logger logger;
        private bool linkStatus;
        private bool initTaskFlag = true;
        private bool loopMonitor = true;
        private List<ListViewItem> itemSource;
        private List<ListViewItem> cacheSource;
        private string currentPath;
        private object lockObject;
        private System.Diagnostics.Process process;
        private System.Diagnostics.PerformanceCounter pf;
        private ImageList smallImageList;
        private ImageList largeImageList;
        private Stack<string> forwardStack;
        private Stack<string> backwardStack;
        private delegate void showDirectory(TreeNode subTN, string path);
        private delegate void showDirectoryAndFile(string path);
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

            //实例化前进后退栈对象
            forwardStack = new Stack<string>();
            backwardStack = new Stack<string>();

            //实例化logger
            logger = cls_Logger.Instance;

            //获取窗口句柄
            cls_Msg.hwndFrmMain = this.Handle;

            //实例化config
            InitConfig();

            //实例化HDFS
            HDFS = cls_HDFS.getInstance();
            InitHDFS();

            //实例化ImageList并插入图片
            InitImageList();

            //开启监控软件内存及CPU线程
            process = System.Diagnostics.Process.GetCurrentProcess();
            pf = new System.Diagnostics.PerformanceCounter("Process", "Working Set - Private", process.ProcessName);
            System.Threading.Thread threadMonitor = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadStatusMonitor));
            threadMonitor.IsBackground = true;
            threadMonitor.Start();
        }

        private void frm_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            loopMonitor = false;
            pf.Dispose();
            process.Dispose();
        }
        #endregion

        #region Event
        private void lv_ShowFile_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            lock (lockObject)
            {
                if (cacheSource == null || cacheSource.Count == 0)
                    return;

                e.Item = cacheSource[e.ItemIndex];
            }
        }

        private void tsb_ActiveGC_Click(object sender, EventArgs e)
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            double memoryMB = process.WorkingSet64 / 1024.0 / 1024.0;
            if (memoryMB > 120.0)
            {
                GC.Collect();
            }
        }

        private void tsmi_UploadFile_Click(object sender, EventArgs e)
        {
            try
            {
                /*string localPath = System.Web.HttpContext.Current.Server.MapPath("D:\\python image source.txt");
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(localPath);
                webRequest.Method = "POST";
                webRequest.AllowAutoRedirect = false;
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                string result = webResponse.Headers["Location"];*/

                string remote = "http://wh0:9870/webhdfs/v1/2.flow?user.name=gazeon&op=CREATE";
                string local = "D:\\1.flow";
                WebClient client = new WebClient();
                client.UploadFile(remote, "PUT", local);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Err", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logger.WriteExceptionLog(ex);
            }
        }

        private void tsmi_DownloadFile_Click(object sender, EventArgs e)
        {
            string remote = "http://wh0:9870/webhdfs/v1/1.flow?op=OPEN";

            string local = "D:\\1.flow";
            //Stream input = null;
            //Stream output = null;
            WebClient client = null;
            FileStream fs = null;
            try
            {
                client = new WebClient();
                byte[] bs = client.DownloadData(remote);
                fs = File.Open(local, FileMode.Create);
                fs.Write(bs, 0, bs.Length);
                bs = null;

                /*HttpWebRequest request = (HttpWebRequest)WebRequest.Create(remote);
                //request.Method = "POST";
                //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                //long totalBytes = request.ContentLength;
                input = request.GetRequestStream();
                output = new FileStream(local, FileMode.Create);
                byte[] bytes = new byte[1024];
                int outputSize = input.Read(bytes, 0, bytes.Length);
                while (outputSize > 0)
                {
                    output.Write(bytes, 0, outputSize);
                    outputSize = input.Read(bytes, 0, bytes.Length);
                }*/
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Err", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logger.WriteExceptionLog(ex);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Flush();
                    fs.Close();
                }

                if (client != null)
                    client.Dispose();

                /*wc.Dispose();
                if (output != null)
                {
                    output.Flush();
                    output.Close();
                }
                if (input != null)
                    input.Close();*/
            }
        }

        private void tv_FolderList_AfterExpand(object sender, TreeViewEventArgs e)
        {
            tv_FolderList.SelectedNode = e.Node;
            TreeNode super = e.Node;
            string path = super.FullPath;

            for (int i = 0; i < super.Nodes.Count; i++)
            {
                TreeNode sub = super.Nodes[i];
                string directory = path + "/" + sub.Text;
                HDFS.client.GetDirectoryStatus(directory)
               .ContinueWith(ds =>
               {
                   ds.Result.Directories.ToList()
                   .ForEach(f =>
                   {
                       showDirectory sd = new showDirectory(GetFirstDirectory);
                       sd.BeginInvoke(sub, f.PathSuffix, null, null);
                       //PostMess(cls_Msg.hwndFrmMain, cls_Msg.LIST_DIRECTORIES, f.PathSuffix);
                   });
               });
                System.Threading.Thread.Sleep(5);
            }
        }

        private void tv_FolderList_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            e.Node.Collapse(false);
            int cnt = e.Node.Nodes.Count;
            for (int i = 0; i < cnt; i++)
            {
                e.Node.Nodes[i].Nodes.Clear();
            }
        }

        private void tsb_Refresh_Click(object sender, EventArgs e)
        {
            CommonEnterTargetPath(this.currentPath);
        }

        private void tsb_Enter_Click(object sender, EventArgs e)
        {
            TCEnterTargetPath(tstb_CurrentPath.Text);
        }

        private void lv_ShowFile_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lv_ShowFile.SelectedIndices.Count <= 0)
                return;

            string currentPath = tstb_CurrentPath.Text;
            if (!currentPath.EndsWith("/"))
                currentPath += "/";
            string info = itemSource[lv_ShowFile.SelectedIndices[0]].Tag.ToString();
            string[] infos = Regex.Split(info, "~>");
            if ("FILE".Equals(infos[6]))
                return;
            lv_ShowFile.SelectedIndices.Clear();
            string newPath = currentPath + infos[0] + "/";
            tstb_CurrentPath.Text = currentPath + infos[0];
            this.currentPath = tstb_CurrentPath.Text;
            backwardStack.Push(this.currentPath);
            forwardStack.Clear();

            showDirectoryAndFile showFile = new showDirectoryAndFile(InvokeEnterTargetPath);
            showFile.BeginInvoke(newPath, null, null);

            /*lv_ShowFile.Clear();
            lv_ShowFile.AllowColumnReorder = true;
            lv_ShowFile.Columns.Add("name", "name", 250);
            lv_ShowFile.Columns.Add("size", "size", 100);
            lv_ShowFile.Columns.Add("permission", "permission", 100);
            lv_ShowFile.Columns.Add("owner", "owner", 100);
            lv_ShowFile.Columns.Add("group", "group", 100);
            lv_ShowFile.Columns.Add("replication", "replication", lv_ShowFile.Width - 650);*/

            //CommonEnterTargetPath(newPath);
        }

        private void tv_FolderList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (initTaskFlag)
                return;
            string currentPath = tv_FolderList.SelectedNode.FullPath;
            tstb_CurrentPath.Text = currentPath;
            this.currentPath = tstb_CurrentPath.Text;
            backwardStack.Push(this.currentPath);
            forwardStack.Clear();

            CommonEnterTargetPath(tstb_CurrentPath.Text);
        }

        private void tsmi_BigIcon_Click(object sender, EventArgs e)
        {
            lv_ShowFile.VirtualMode = false;
            lv_ShowFile.View = View.LargeIcon;
        }

        private void tsmi_SmallIcon_Click(object sender, EventArgs e)
        {
            lv_ShowFile.VirtualMode = false;
            lv_ShowFile.View = View.SmallIcon;
        }

        private void tsmi_TailIcon_Click(object sender, EventArgs e)
        {
            lv_ShowFile.VirtualMode = false;
            lv_ShowFile.View = View.Tile;
        }

        private void tsmi_ListIcon_Click(object sender, EventArgs e)
        {
            lv_ShowFile.VirtualMode = false;
            lv_ShowFile.View = View.List;
        }

        private void tsmi_DetailIcon_Click(object sender, EventArgs e)
        {
            lv_ShowFile.VirtualMode = false;
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
            if (forwardStack.Count > 0)
            {
                string target = forwardStack.Pop();
                backwardStack.Push(target);
                TCFEnterTargetPath(target);
                tstb_CurrentPath.Text = target;
            }
        }

        private void tsb_Backward_Click(object sender, EventArgs e)
        {
            if (backwardStack.Count > 1)
            {
                forwardStack.Push(backwardStack.Pop());
                string target = backwardStack.Peek();
                TCFEnterTargetPath(target);
                tstb_CurrentPath.Text = target;
            }
        }
        #endregion

        #region Function
        /// <summary>
        /// 获取系统启动经过的毫秒数
        /// </summary>
        /// <returns></returns>
        private int GetTickCount()
        {
            return System.Environment.TickCount;
        }

        /// <summary>
        /// 虚拟模式进入HDFS目录
        /// </summary>
        /// <param name="directory">路径</param>
        private void InvokeEnterTargetPath(string directory)
        {
            this.lv_ShowFile.Invoke(new Action(() =>
            {
                itemSource.Clear();
                try
                {
                    lv_ShowFile.VirtualMode = false;
                }
                catch { }
                lv_ShowFile.Clear();
                lv_ShowFile.AllowColumnReorder = true;
                lv_ShowFile.Columns.Add("name", "name", 250);
                lv_ShowFile.Columns.Add("size", "size", 100);
                lv_ShowFile.Columns.Add("permission", "permission", 50);
                lv_ShowFile.Columns.Add("owner", "owner", 50);
                lv_ShowFile.Columns.Add("group", "group", 50);
                lv_ShowFile.Columns.Add("modifiedtime", "modifiedtime", lv_ShowFile.Width - 650);
            }));

            HDFS.client.GetDirectoryStatus(directory)
                .ContinueWith(ds =>
                {
                    ds.Result.Entries.ToList()
                    .ForEach(f =>
                    {
                        string lastModifiedTime = "";
                        if (f.Info != null)
                        {
                            long lastModified = Convert.ToInt64(f.Info.GetValue("modificationTime").ToString());
                            lastModifiedTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)).AddMilliseconds(lastModified).ToLongDateString();
                        }
                        string permission = f.Permission;
                        string owner = f.Owner;
                        string group = f.Group;
                        string size = f.Length.ToString();
                        //string replication = f.Replication.ToString();
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
                        .Append(lastModifiedTime)
                        .Append("~>")
                        .Append(type);
                        /*.Append("~>")
                        .Append(tstb_CurrentPath.Text);*/

                        PostMess(cls_Msg.hwndFrmMain, cls_Msg.LIST_DIRECTORIES_AND_FILES, sb.ToString());
                    });
                    System.Threading.Thread.Sleep(50);
                    PostMess(cls_Msg.hwndFrmMain, cls_Msg.SHOW_LIST_VIEW_ITEMS);
                });
        }

        /// <summary>
        /// no-try进入HDFS目标目录
        /// </summary>
        /// <param name="directory"></param>
        private void CommonEnterTargetPath(string directory)
        {
            itemSource.Clear();
            lv_ShowFile.VirtualMode = false;
            lv_ShowFile.Clear();
            lv_ShowFile.AllowColumnReorder = true;
            lv_ShowFile.Columns.Add("name", "name", 250);
            lv_ShowFile.Columns.Add("size", "size", 100);
            lv_ShowFile.Columns.Add("permission", "permission", 50);
            lv_ShowFile.Columns.Add("owner", "owner", 50);
            lv_ShowFile.Columns.Add("group", "group", 50);
            lv_ShowFile.Columns.Add("modifiedtime", "modifiedtime", lv_ShowFile.Width - 650);

            HDFS.client.GetDirectoryStatus(directory)
                .ContinueWith(ds =>
                {
                    ds.Result.Entries.ToList()
                    .ForEach(f =>
                    {
                        string lastModifiedTime = "";
                        if (f.Info != null)
                        {
                            long lastModified = Convert.ToInt64(f.Info.GetValue("modificationTime").ToString());
                            lastModifiedTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)).AddMilliseconds(lastModified).ToLongDateString();
                        }
                        string permission = f.Permission;
                        string owner = f.Owner;
                        string group = f.Group;
                        string size = f.Length.ToString();
                        //string replication = f.Replication.ToString();
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
                        .Append(lastModifiedTime)
                        .Append("~>")
                        .Append(type);
                        /*.Append("~>")
                        .Append(tstb_CurrentPath.Text);*/

                        PostMess(cls_Msg.hwndFrmMain, cls_Msg.LIST_DIRECTORIES_AND_FILES, sb.ToString());
                    });
                    System.Threading.Thread.Sleep(50);
                    PostMess(cls_Msg.hwndFrmMain, cls_Msg.SHOW_LIST_VIEW_ITEMS);
                });
        }

        /// <summary>
        /// try-catch进入HDFS目标目录
        /// </summary>
        /// <param name="directory"></param>
        private void TCEnterTargetPath(string directory)
        {
            itemSource.Clear();
            lv_ShowFile.VirtualMode = false;
            lv_ShowFile.Clear();
            lv_ShowFile.AllowColumnReorder = true;
            lv_ShowFile.Columns.Add("name", "name", 250);
            lv_ShowFile.Columns.Add("size", "size", 100);
            lv_ShowFile.Columns.Add("permission", "permission", 50);
            lv_ShowFile.Columns.Add("owner", "owner", 50);
            lv_ShowFile.Columns.Add("group", "group", 50);
            lv_ShowFile.Columns.Add("modifiedtime", "modifiedtime", lv_ShowFile.Width - 650);

            HDFS.client.GetDirectoryStatus(directory)
            .ContinueWith(ds =>
            {
                try
                {
                    ds.Result.Entries.ToList()
                        .ForEach(f =>
                        {
                            string lastModifiedTime = "";
                            if (f.Info != null)
                            {
                                long lastModified = Convert.ToInt64(f.Info.GetValue("modificationTime").ToString());
                                lastModifiedTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)).AddMilliseconds(lastModified).ToLongDateString();
                            }
                            string permission = f.Permission;
                            string owner = f.Owner;
                            string group = f.Group;
                            string size = f.Length.ToString();
                            //string replication = f.Replication.ToString();
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
                            .Append(lastModifiedTime)
                            .Append("~>")
                            .Append(type);
                            /*.Append("~>")
                            .Append(tstb_CurrentPath.Text);*/

                            PostMess(cls_Msg.hwndFrmMain, cls_Msg.LIST_DIRECTORIES_AND_FILES, sb.ToString());
                        });
                    System.Threading.Thread.Sleep(50);
                    PostMess(cls_Msg.hwndFrmMain, cls_Msg.SHOW_LIST_VIEW_ITEMS);
                    PostMess(cls_Msg.hwndFrmMain, cls_Msg.ASSIGN_PATH, directory);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("跳转的路径有误!", "Err", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    logger.WriteExceptionLog(ex);
                }
            });
        }

        /// <summary>
        /// try-catch-finally进入HDFS目标目录
        /// </summary>
        /// <param name="directory"></param>
        private void TCFEnterTargetPath(string directory)
        {
            itemSource.Clear();
            lv_ShowFile.VirtualMode = false;
            lv_ShowFile.Clear();
            lv_ShowFile.AllowColumnReorder = true;
            lv_ShowFile.Columns.Add("name", "name", 250);
            lv_ShowFile.Columns.Add("size", "size", 100);
            lv_ShowFile.Columns.Add("permission", "permission", 50);
            lv_ShowFile.Columns.Add("owner", "owner", 50);
            lv_ShowFile.Columns.Add("group", "group", 50);
            lv_ShowFile.Columns.Add("modifiedtime", "modifiedtime", lv_ShowFile.Width - 650);

            HDFS.client.GetDirectoryStatus(directory)
            .ContinueWith(ds =>
            {
                try
                {
                    ds.Result.Entries.ToList()
                        .ForEach(f =>
                        {
                            string lastModifiedTime = "";
                            if (f.Info != null)
                            {
                                long lastModified = Convert.ToInt64(f.Info.GetValue("modificationTime").ToString());
                                lastModifiedTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)).AddMilliseconds(lastModified).ToLongDateString();
                            }
                            string permission = f.Permission;
                            string owner = f.Owner;
                            string group = f.Group;
                            string size = f.Length.ToString();
                            //string replication = f.Replication.ToString();
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
                            .Append(lastModifiedTime)
                            .Append("~>")
                            .Append(type);
                            /*.Append("~>")
                            .Append(tstb_CurrentPath.Text);*/

                            PostMess(cls_Msg.hwndFrmMain, cls_Msg.LIST_DIRECTORIES_AND_FILES, sb.ToString());
                        });
                    System.Threading.Thread.Sleep(50);
                    PostMess(cls_Msg.hwndFrmMain, cls_Msg.SHOW_LIST_VIEW_ITEMS);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("跳转的路径有误!", "Err", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    logger.WriteExceptionLog(ex);
                }
                finally
                {
                    PostMess(cls_Msg.hwndFrmMain, cls_Msg.NAVIGATE_PATH_CHANGE, directory);
                    //tstb_CurrentPath.Text = target;
                }
            });
        }

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
        private void GetFirstDirectory(TreeNode tn, string directory)
        {
            this.tv_FolderList.Invoke(new Action(() =>
            {
                tn.Nodes.Add(directory);
            }));
        }

        /// <summary>
        /// 初始化HDFS
        /// </summary>
        private void InitHDFS()
        {
            itemSource = new List<ListViewItem>();
            cacheSource = new List<ListViewItem>();
            lv_ShowFile.VirtualMode = true;
            tv_FolderList.Nodes.Clear();

            Uri myUri = new Uri("http://" + cls_Config.host + ":" + cls_Config.port + "/");
            HDFS.client = new WebHDFSClient(myUri, cls_Config.userName);
            tv_FolderList.Nodes.Add("/");
            tv_FolderList.BeginUpdate();
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
                    catch (Exception ex)
                    {
                        linkStatus = false;
                        PostMess(cls_Msg.hwndFrmMain, cls_Msg.MAIN_UI_DISABLE);
                        MessageBox.Show("HDFS连接失败!", "Err", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        logger.WriteExceptionLog(ex);
                    }
                    finally
                    {
                        initTaskFlag = false;
                        PostMess(cls_Msg.hwndFrmMain, cls_Msg.TREEVIEW_ENDUPDATE);
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

        #region Thread
        private void ThreadStatusMonitor()
        {
            int lastTick = GetTickCount();
            while (loopMonitor)
            {
                double memUsed = Math.Round(pf.NextValue() / 1024.0 / 1024.0, 2);
                int currentTick = GetTickCount();
                if (currentTick - lastTick > 20000 && memUsed > 120)
                {
                    lastTick = GetTickCount();
                    GC.Collect();
                }
                PostMess(cls_Msg.hwndFrmMain, cls_Msg.SHOW_MEMORY_USED, memUsed.ToString());
                System.Threading.Thread.Sleep(1000);
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
                case cls_Msg.TREEVIEW_ENDUPDATE:
                    tv_FolderList.EndUpdate();
                    break;

                case cls_Msg.SHOW_LIST_VIEW_ITEMS:
                    lock (lockObject)
                    {
                        cacheSource = itemSource;
                        try
                        {
                            if (itemSource.Count != 0)
                            {
                                lv_ShowFile.VirtualMode = true;
                                lv_ShowFile.VirtualListSize = cacheSource.Count;
                                lv_ShowFile.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(lv_ShowFile_RetrieveVirtualItem);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.WriteExceptionLog(ex);
                        }
                    }
                    break;

                case cls_Msg.SHOW_MEMORY_USED:
                    string memory = Marshal.PtrToStringAnsi(m.WParam);
                    tssl_Mem.Text = memory;
                    Marshal.FreeHGlobal(m.WParam);
                    break;

                case cls_Msg.MAIN_UI_ENABLE:
                    RefreshUIEnable(true);
                    tstb_CurrentPath.Text = "/";
                    this.currentPath = tstb_CurrentPath.Text;
                    backwardStack.Push(this.currentPath);
                    forwardStack.Clear();
                    CommonEnterTargetPath(tstb_CurrentPath.Text);
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
                        itemSource.Add(item);
                        //lv_ShowFile.Items.Add(item);
                        Marshal.FreeHGlobal(m.WParam);
                    }
                    break;

                case cls_Msg.NAVIGATE_PATH_CHANGE:
                    string path = Marshal.PtrToStringAnsi(m.WParam);
                    tstb_CurrentPath.Text = path;
                    break;

                case cls_Msg.ASSIGN_PATH:
                    this.currentPath = tstb_CurrentPath.Text;
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
