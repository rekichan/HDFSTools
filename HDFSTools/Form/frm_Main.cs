using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Hadoop.WebHDFS;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using RestSharp;

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
        private bool[] lvBit;
        private List<ListViewItem> itemSource;
        private List<ListViewItem> cacheSource;
        private List<ListViewItem> searchSource;
        private string currentPath;
        private string lastSearch = "";
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

        private void frm_Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                tstb_Search.Focus();
                tstb_Search.SelectAll();
            }

            if (e.KeyCode == Keys.Back && !tstb_Search.Focused && !tstb_CurrentPath.Focused)
            {
                if (backwardStack.Count > 1)
                {
                    forwardStack.Push(backwardStack.Pop());
                    string target = backwardStack.Peek();
                    TCFEnterTargetPath(target, false);
                    tstb_CurrentPath.Text = target;
                    this.currentPath = target;
                }
            }

            if (e.KeyCode == Keys.F5)
                TCEnterTargetPath(this.currentPath);
        }
        #endregion

        #region Event
        private void tsb_Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tsmi_DeleteFile_Click(object sender, EventArgs e)
        {
            DeleteFileOrFolder();
        }

        private void csmi_Delete_Click(object sender, EventArgs e)
        {
            DeleteFileOrFolder();
        }

        private void lv_ShowSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv_ShowSearch.SelectedIndices.Count != 0)
                lv_ShowFile.SelectedIndices.Clear();
        }

        private void lv_ShowFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv_ShowFile.SelectedIndices.Count != 0)
                lv_ShowSearch.SelectedIndices.Clear();
        }

        private void lv_ShowSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                OpenSearchFolder();
            }
        }

        private void lv_ShowFile_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                OpenFolder();
            }
        }

        private void tsmi_CreateFolder_Click(object sender, EventArgs e)
        {
            frm_CreateFolder cf = new frm_CreateFolder(this.currentPath);
            cf.ShowDialog();
        }

        private void cmsi_CreateFolder_Click(object sender, EventArgs e)
        {
            frm_CreateFolder cf = new frm_CreateFolder(this.currentPath);
            cf.ShowDialog();
        }

        private void tsb_ConnectConfig_Click(object sender, EventArgs e)
        {
            frm_Config fc = new frm_Config();
            fc.ShowDialog();
        }

        private void tsb_Connect_Click(object sender, EventArgs e)
        {
            if (linkStatus || initTaskFlag)
                return;

            InitHDFS();
        }

        private void cmsi_DownloadFile_Click(object sender, EventArgs e)
        {
            DownloadFile();
        }

        private void cmsi_OpenFolder_Click(object sender, EventArgs e)
        {
            OpenFolder();
        }

        private void cmsi_UploadFile_Click(object sender, EventArgs e)
        {
            UploadFile();
        }

        private void cmsi_Refresh_Click(object sender, EventArgs e)
        {
            TCEnterTargetPath(this.currentPath);
        }

        private void lv_ShowSearch_ColumnReordered(object sender, ColumnReorderedEventArgs e)
        {
            //防止拖动column header
            e.Cancel = true;
        }

        private void tsb_ReturnPrev_Click(object sender, EventArgs e)
        {
            if (currentPath == "/")
                return;

            string cur = currentPath;
            string[] curs = cur.Split('/');
            string target = "/";
            for (int i = 0; i < curs.Length - 1; i++)
            {
                if ("".Equals(curs[i]))
                    continue;
                target += curs[i];
                if (i != curs.Length - 2)
                    target += "/";
            }

            tstb_CurrentPath.Text = target;
            backwardStack.Push(target);
            forwardStack.Clear();

            if (!target.EndsWith("/"))
                target += "/";

            TCFEnterTargetPath(target, true);
        }

        private void lv_ShowSearch_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenSearchFolder();
        }

        private void tstb_Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SearchFile();
        }

        private void tstb_CurrentPath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                TCEnterTargetPath(tstb_CurrentPath.Text);
        }

        private void tsb_Search_Click(object sender, EventArgs e)
        {
            SearchFile();
        }

        private void lv_ShowFile_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //column header 数据排序
            int index = e.Column;
            lvBit[index] = !lvBit[index];
            cacheSource.Sort((x, y) =>
            {
                int cmp = x.SubItems[index].Text.CompareTo(y.SubItems[index].Text);
                if (cmp == 0)
                    cmp = x.SubItems[5].Text.CompareTo(y.SubItems[5].Text);
                if (lvBit[index])
                    return cmp;
                else
                    return -cmp;
            });
            lv_ShowFile.Refresh();
        }

        private void lv_ShowFile_ColumnReordered(object sender, ColumnReorderedEventArgs e)
        {
            //防止拖动column header
            e.Cancel = true;
        }

        private void lv_ShowFile_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            lock (lockObject)
            {
                if (cacheSource == null || cacheSource.Count == 0)
                    return;

                e.Item = cacheSource[e.ItemIndex];
            }
        }

        private void lv_SearchFile_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            lock (lockObject)
            {
                if (searchSource == null || searchSource.Count == 0)
                    return;

                e.Item = searchSource[e.ItemIndex];
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
            UploadFile();
        }

        private void tsmi_DownloadFile_Click(object sender, EventArgs e)
        {
            DownloadFile();
        }

        /*private void tv_FolderList_AfterExpand(object sender, TreeViewEventArgs e)
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
        }*/

        /*private void tv_FolderList_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            e.Node.Collapse(false);
            int cnt = e.Node.Nodes.Count;
            for (int i = 0; i < cnt; i++)
            {
                e.Node.Nodes[i].Nodes.Clear();
            }
        }*/

        private void tsb_Refresh_Click(object sender, EventArgs e)
        {
            TCEnterTargetPath(this.currentPath);
        }

        private void tsb_Enter_Click(object sender, EventArgs e)
        {
            TCEnterTargetPath(tstb_CurrentPath.Text);
        }

        private void lv_ShowFile_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenFolder();
        }

        /*private void tv_FolderList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (initTaskFlag)
                return;
            string currentPath = tv_FolderList.SelectedNode.FullPath;
            tstb_CurrentPath.Text = currentPath;
            this.currentPath = tstb_CurrentPath.Text;
            backwardStack.Push(this.currentPath);
            forwardStack.Clear();

            CommonEnterTargetPath(tstb_CurrentPath.Text);
        }*/

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

        private void tsb_Forward_Click(object sender, EventArgs e)
        {
            if (forwardStack.Count > 0)
            {
                string target = forwardStack.Pop();
                backwardStack.Push(target);
                TCFEnterTargetPath(target, false);
                tstb_CurrentPath.Text = target;
                this.currentPath = target;
            }
        }

        private void tsb_Backward_Click(object sender, EventArgs e)
        {
            if (backwardStack.Count > 1)
            {
                forwardStack.Push(backwardStack.Pop());
                string target = backwardStack.Peek();
                TCFEnterTargetPath(target, false);
                tstb_CurrentPath.Text = target;
                this.currentPath = target;
            }
        }
        #endregion

        #region Function
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="target">文件或文件夹路径</param>
        private void DeleteFileOrFolder()
        {
            if (lv_ShowFile.SelectedIndices.Count <= 0)
                return;

            DialogResult dr = MessageBox.Show("确定要删除文件?", "Q", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr != DialogResult.Yes)
                return;

            try
            {
                string info = itemSource[lv_ShowFile.SelectedIndices[0]].Tag.ToString();
                string[] infos = Regex.Split(info, "~>");
                string target = (this.currentPath.EndsWith("/") ? this.currentPath : this.currentPath + "/") + infos[0];
                lv_ShowFile.FocusedItem = null;
                lv_ShowFile.SelectedIndices.Clear();
                string uri = "http://" + cls_Config.host + ":" + cls_Config.port;
                string req = "webhdfs/v1/" + target + "?op=DELETE&recursive=true";
                RestClient client = new RestClient(uri);
                RestRequest request = new RestRequest(req);
                client.Delete(request);
                TCEnterTargetPath(this.currentPath);
                if ("DIRECTORY".Equals(infos[6].ToUpper()))
                {
                    if (forwardStack.Count > 0)
                    {
                        string forward = forwardStack.Peek();
                        if (target.Equals(forward))
                            forwardStack.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Err", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logger.WriteExceptionLog(ex);
            }
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="folderName">文件夹名</param>
        private void CreateFolder(string folderName)
        {
            try
            {
                /*string localPath = System.Web.HttpContext.Current.Server.MapPath("D:\\python image source.txt");
               HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(localPath);
               webRequest.Method = "POST";
               webRequest.AllowAutoRedirect = false;
               HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
               string result = webResponse.Headers["Location"];*/

                //http://wh0:9870/webhdfs/v1/hehe?op=MKDIRS 单层创建
                //http://wh0:9870/webhdfs/v1/gaga/gege?op=MKDIRS 复合创建
                string uri = "http://" + cls_Config.host + ":" + cls_Config.port;
                string req = "webhdfs/v1/" + folderName + "?op=MKDIRS";
                RestClient client = new RestClient(uri);
                RestRequest request = new RestRequest(req);
                client.Put(request);
                TCEnterTargetPath(this.currentPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Err", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logger.WriteExceptionLog(ex);
            }
        }

        /// <summary>
        /// 返回特殊权限
        /// </summary>
        /// <param name="single">单个权限字符</param>
        /// <returns></returns>
        private string SpecialPermission(char single)
        {
            string buf;
            switch (single)
            {
                case '1':
                    buf = "o";
                    break;

                case '2':
                    buf = "g";
                    break;

                case '4':
                    buf = "u";
                    break;

                default:
                    buf = "-";
                    break;
            }
            return buf;
        }

        /// <summary>
        /// 返回单个普通权限
        /// </summary>
        /// <param name="single">单个权限字符</param>
        /// <returns></returns>
        private string CommonPermission(char single)
        {
            string buf;
            switch (single)
            {
                case '1':
                    buf = "--x";
                    break;

                case '2':
                    buf = "-w-";
                    break;

                case '3':
                    buf = "-wx";
                    break;

                case '4':
                    buf = "r--";
                    break;

                case '5':
                    buf = "r-x";
                    break;

                case '6':
                    buf = "rw-";
                    break;

                case '7':
                    buf = "rwx";
                    break;

                default:
                    buf = "---";
                    break;
            }
            return buf;
        }

        /// <summary>
        /// 返回最终权限
        /// </summary>
        /// <param name="origin">原始权限字符</param>
        /// <returns></returns>
        private string Permission(string origin)
        {
            string pmn = "";
            if (origin.Length == 3)
            {
                for (int i = 0; i < origin.Length; i++)
                {
                    pmn += CommonPermission(origin[i]);
                }
                origin = pmn;
            }
            else if (origin.Length == 4)
            {
                pmn = SpecialPermission(origin[0]);
                for (int i = 1; i < origin.Length; i++)
                {
                    pmn += CommonPermission(origin[i]);
                }
                origin = pmn;
            }

            return origin;
        }

        /// <summary>
        /// 打开搜索栏中被选中的文件夹
        /// </summary>
        private void OpenSearchFolder()
        {
            if (lv_ShowSearch.SelectedIndices.Count <= 0)
                return;

            string info = searchSource[lv_ShowSearch.SelectedIndices[0]].Tag.ToString();
            string[] infos = Regex.Split(info, "~>");
            if ("FILE".Equals(infos[6]))
                return;

            string currentDic = currentPath.EndsWith("/") ? infos[7] + infos[0] : infos[7] + "/" + infos[0];
            string target = currentDic;

            if (this.currentPath.Equals(currentDic))
                return;

            if (!target.EndsWith("/"))
                target += "/";

            lv_ShowSearch.SelectedIndices.Clear();
            tstb_CurrentPath.Text = currentDic;
            this.currentPath = tstb_CurrentPath.Text;
            backwardStack.Push(this.currentPath);
            forwardStack.Clear();

            showDirectoryAndFile showFile = new showDirectoryAndFile(InvokeEnterTargetPath);
            showFile.BeginInvoke(currentDic, null, null);
        }

        /// <summary>
        /// 打开被选中的文件夹
        /// </summary>
        private void OpenFolder()
        {
            if (lv_ShowFile.SelectedIndices.Count <= 0)
                return;

            string currentPath = tstb_CurrentPath.Text;
            if (!currentPath.EndsWith("/"))
                currentPath += "/";
            string info = itemSource[lv_ShowFile.SelectedIndices[0]].Tag.ToString();
            string[] infos = Regex.Split(info, "~>");
            if ("FILE".Equals(infos[6].ToUpper()))
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
            lv_ShowFile.Columns.Add("replication", "replication", 200);*/

            //CommonEnterTargetPath(newPath);
        }

        /// <summary>
        /// 下载选中的文件
        /// </summary>
        private void DownloadFile()
        {
            if (lv_ShowFile.SelectedIndices.Count <= 0)
                return;

            //Stream input = null;
            //Stream output = null;
            WebClient client = null;
            FileStream fs = null;
            try
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    DialogResult dr = fbd.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        //string remote = "http://wh0:9870/webhdfs/v1/1.flow?op=OPEN";
                        //string local = "D:\\1.flow";
                        string info = itemSource[lv_ShowFile.SelectedIndices[0]].Tag.ToString();
                        string[] infos = Regex.Split(info, "~>");
                        string target = fbd.SelectedPath + "\\" + infos[0];
                        if (File.Exists(target))
                        {
                            MessageBox.Show("目标路径已存在同名文件!", "Warn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if ("DIRECTORY".Equals(infos[6].ToUpper()))
                        {
                            MessageBox.Show("下载的目标不为文件!", "Warn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        string file = (this.currentPath.EndsWith("/") ? this.currentPath : this.currentPath + "/") + infos[0];
                        string remote = "http://" + cls_Config.host + ":" + cls_Config.port + "/webhdfs/v1" + file + "?op=OPEN";
                        client = new WebClient();
                        byte[] bs = client.DownloadData(remote);
                        fs = File.Open(target, FileMode.Create);
                        fs.Write(bs, 0, bs.Length);
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
                }
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

        /// <summary>
        /// 搜索文件
        /// </summary>
        private void SearchFile()
        {
            lv_ShowSearch.VirtualMode = false;
            string search = tstb_Search.Text.Trim();
            if (string.IsNullOrWhiteSpace(search) || lastSearch.Equals(search))
                return;
            List<ListViewItem> lvi;
            lvi = cacheSource.Where(s => s.Text.ToLower().Contains(search.ToLower())).ToList();
            int cnt = lvi.Count;
            if (cnt != 0)
            {
                searchSource = lvi;
                for (int i = 0; i < searchSource.Count; i++)
                {
                    searchSource[i].Tag = searchSource[i].Tag + "~>" + currentPath;
                }
                lv_ShowSearch.Clear();
                InitSearchListView();
                lv_ShowSearch.AllowColumnReorder = false;
                lv_ShowSearch.VirtualMode = true;
                lv_ShowSearch.VirtualListSize = cnt;
                lv_ShowSearch.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(lv_SearchFile_RetrieveVirtualItem);
                System.Threading.Thread.Sleep(50);
                lv_ShowSearch.Focus();
                lv_ShowSearch.Select();
            }
            lastSearch = search;
            tssl_SearchResult.Text = string.Format("共找到{0}个", cnt.ToString());
            tssl_SearchPath.Text = currentPath;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        private void UploadFile()
        {
            WebClient client = null;
            try
            {
                /*string localPath = System.Web.HttpContext.Current.Server.MapPath("D:\\python image source.txt");
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(localPath);
                webRequest.Method = "POST";
                webRequest.AllowAutoRedirect = false;
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                string result = webResponse.Headers["Location"];*/

                /*string remote = "http://wh0:9870/webhdfs/v1/2.flow?user.name=gazeon&op=CREATE";
                string local = "D:\\1.flow";
                WebClient client = new WebClient();
                client.UploadFile(remote, "PUT", local);*/

                //http://wh0:9870/webhdfs/v1/abc.txt?op=CREATE&noredirect=true 在根目录
                //http://wh0:9870/webhdfs/v1/haha/abc.txt?op=CREATE&noredirect=true 在指定目录
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    DialogResult dr = ofd.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        string path = this.currentPath;
                        if (!path.EndsWith("/"))
                            path += "/";
                        string local = ofd.FileName;
                        string fileName = Path.GetFileName(local);
                        string remote = "http://" + cls_Config.host + ":" + cls_Config.port + "/webhdfs/v1" + path + fileName + "?user.name=" + cls_Config.userName + "&op=CREATE";
                        client = new WebClient();
                        client.UploadFile(remote, "PUT", local);
                        TCEnterTargetPath(this.currentPath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Err", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logger.WriteExceptionLog(ex);
            }
            finally
            {
                if (client != null)
                    client.Dispose();
            }
        }

        /// <summary>
        /// 初始化搜索ListView
        /// </summary>
        private void InitSearchListView()
        {
            lv_ShowSearch.AllowColumnReorder = false;
            lv_ShowSearch.Columns.Add("name", "name", 350);
            lv_ShowSearch.Columns.Add("size", "size", 100);
            lv_ShowSearch.Columns.Add("permission", "permission", 125);
            lv_ShowSearch.Columns.Add("owner", "owner", 100);
            lv_ShowSearch.Columns.Add("group", "group", 100);
            lv_ShowSearch.Columns.Add("modifiedtime", "modifiedtime", 200);
        }

        /// <summary>
        /// 初始化文件ListView
        /// </summary>
        private void InitShowListView()
        {
            lv_ShowFile.Clear();
            lv_ShowFile.AllowColumnReorder = true;
            lv_ShowFile.Columns.Add("name", "name", 350);
            lv_ShowFile.Columns.Add("size", "size", 100);
            lv_ShowFile.Columns.Add("permission", "permission", 125);
            lv_ShowFile.Columns.Add("owner", "owner", 100);
            lv_ShowFile.Columns.Add("group", "group", 100);
            lv_ShowFile.Columns.Add("modifiedtime", "modifiedtime", 200);
        }

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
                lv_ShowFile.SelectedIndices.Clear();
                itemSource.Clear();
                try
                {
                    lv_ShowFile.VirtualMode = false;
                }
                catch { }
                InitShowListView();
            }));

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
                            lastModifiedTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)).AddMilliseconds(lastModified).ToLocalTime().ToString();
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
                        MessageBox.Show("跳转的路径已不存在!", "Err", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        logger.WriteExceptionLog(ex);
                    }
                });
        }

        /// <summary>
        /// no-try进入HDFS目标目录
        /// </summary>
        /// <param name="directory">目标路径</param>
        private void CommonEnterTargetPath(string directory)
        {
            try
            {
                lv_ShowFile.SelectedIndices.Clear();
                itemSource.Clear();
                lv_ShowFile.VirtualMode = false;
                InitShowListView();
            }
            catch (Exception ex)
            {
                logger.WriteExceptionLog(ex);
                return;
            }

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
                            lastModifiedTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)).AddMilliseconds(lastModified).ToLocalTime().ToString();
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
                });
        }

        /// <summary>
        /// try-catch进入HDFS目标目录
        /// </summary>
        /// <param name="directory">目标路径</param>
        private void TCEnterTargetPath(string directory)
        {
            try
            {
                lv_ShowFile.SelectedIndices.Clear();
                itemSource.Clear();
                lv_ShowFile.VirtualMode = false;
                InitShowListView();
            }
            catch (Exception ex)
            {
                logger.WriteExceptionLog(ex);
                return;
            }

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
                                lastModifiedTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)).AddMilliseconds(lastModified).ToLocalTime().ToString();
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
        /// <param name="directory">目标路径</param>
        /// <param name="search">是否为搜索</param>
        private void TCFEnterTargetPath(string directory, bool search)
        {
            try
            {
                lv_ShowFile.SelectedIndices.Clear();
                itemSource.Clear();
                lv_ShowFile.VirtualMode = false;
                InitShowListView();
            }
            catch (Exception ex)
            {
                logger.WriteExceptionLog(ex);
                return;
            }

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
                                lastModifiedTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)).AddMilliseconds(lastModified).ToLocalTime().ToString();
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
                finally
                {
                    if (!search)
                        PostMess(cls_Msg.hwndFrmMain, cls_Msg.NAVIGATE_PATH_CHANGE, directory);
                }
            });
        }

        /*/// <summary>
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
        }*/

        /*/// <summary>
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
        }*/

        /// <summary>
        /// 初始化HDFS
        /// </summary>
        private void InitHDFS()
        {
            itemSource = new List<ListViewItem>();
            cacheSource = new List<ListViewItem>();
            lv_ShowFile.VirtualMode = true;
            //tv_FolderList.Nodes.Clear();

            Uri myUri = new Uri("http://" + cls_Config.host + ":" + cls_Config.port + "/");
            HDFS.client = new WebHDFSClient(myUri, cls_Config.userName);
            //tv_FolderList.Nodes.Add("/");
            //tv_FolderList.BeginUpdate();
            HDFS.client.GetDirectoryStatus("/")
                .ContinueWith(ds =>
                {
                    try
                    {
                        linkStatus = true;
                        ds.Result.Info.ToString();
                        PostMess(cls_Msg.hwndFrmMain, cls_Msg.MAIN_UI_ENABLE);
                        /*ds.Result.Directories.ToList()
                        .ForEach(f =>
                        {
                            PostMess(cls_Msg.hwndFrmMain, cls_Msg.LIST_DIRECTORIES, f.PathSuffix);
                        });*/
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
                        //PostMess(cls_Msg.hwndFrmMain, cls_Msg.TREEVIEW_ENDUPDATE);
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

            //tv_FolderList.ImageList = smallImageList;

            lv_ShowFile.LargeImageList = largeImageList;
            lv_ShowFile.SmallImageList = smallImageList;
            lv_ShowSearch.LargeImageList = largeImageList;
            lv_ShowSearch.SmallImageList = smallImageList;
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
        /// 主界面UI激活
        /// </summary>
        /// <param name="enable">是否激活</param>
        public void RefreshUIEnable(bool enable = false)
        {
            sc_Main.Enabled = enable;

            tsb_Backward.Enabled = enable;
            tsb_Forward.Enabled = enable;
            tsb_Refresh.Enabled = enable;
            tsb_Enter.Enabled = enable;
            tsb_Search.Enabled = enable;
            tsb_ReturnPrev.Enabled = enable;
            tsb_Connect.Enabled = !enable;

            tssb_File.Enabled = enable;
            tstb_CurrentPath.Enabled = enable;
            tstb_Search.Enabled = enable;

            //tv_FolderList.Enabled = enable;

            lv_ShowFile.Enabled = enable;
            lv_ShowSearch.Enabled = enable;

            //
            tsmi_DeleteFile.Enabled = enable;
            tsmi_UploadFile.Enabled = enable;
            tsmi_DownloadFile.Enabled = enable;
            tsmi_CreateFolder.Enabled = enable;

            cmsi_Delete.Enabled = enable;
            cmsi_CreateFolder.Enabled = enable;
            cmsi_OpenFolder.Enabled = enable;
            cmsi_Refresh.Enabled = enable;
            cmsi_UploadFile.Enabled = enable;
            cmsi_DownloadFile.Enabled = enable;

            if (enable)
            {
                tssl_LinkStatus.Text = "已链接";
                tssl_LinkStatus.BackColor = Color.Lime;
            }
            else
            {
                tssl_LinkStatus.Text = "未连接";
                tssl_LinkStatus.BackColor = Color.Red;
            }
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
                //循环检测系统内存使用量
                //如果内存使用量>120M&&强制GC时间间隔>20s则强制GC一次，防止因为系统GC不及时导致内存使用量过大
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
                /*case cls_Msg.TREEVIEW_ENDUPDATE:
                    //tv_FolderList.EndUpdate();
                    break;*/

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
                    tssl_MemoryUsage.Text = memory;
                    Marshal.FreeHGlobal(m.WParam);
                    break;

                case cls_Msg.MAIN_UI_ENABLE:
                    RefreshUIEnable(true);
                    tstb_CurrentPath.Text = "/";
                    this.currentPath = tstb_CurrentPath.Text;
                    backwardStack.Push(this.currentPath);
                    forwardStack.Clear();
                    lvBit = new bool[6];
                    TCEnterTargetPath(tstb_CurrentPath.Text);
                    InitSearchListView();
                    break;

                case cls_Msg.MAIN_UI_DISABLE:
                    RefreshUIEnable();
                    break;

                /*case cls_Msg.LIST_DIRECTORIES:
                    lock (lockObject)
                    {
                        string text = Marshal.PtrToStringAnsi(m.WParam);
                        *//*TreeNode tn = new TreeNode(text);
                        tn.ImageIndex = smallImageList.Images.IndexOfKey("folder");
                        tn.SelectedImageIndex = smallImageList.Images.IndexOfKey("folder");
                        tv_FolderList.Nodes[0].Nodes.Add(tn);*//*
                        Marshal.FreeHGlobal(m.WParam);
                    }
                    break;*/

                case cls_Msg.LIST_DIRECTORIES_AND_FILES:
                    lock (lockObject)
                    {
                        string info = Marshal.PtrToStringAnsi(m.WParam);
                        string[] infos = Regex.Split(info, "~>");
                        ListViewItem item = new ListViewItem();
                        item.Text = infos[0];
                        string permission = infos[2];
                        permission = Permission(permission);
                        item.Tag = info;
                        if ("DIRECTORY".Equals(infos[6].ToUpper()))
                        {
                            item.ImageIndex = smallImageList.Images.IndexOfKey("folder");
                            permission = "d" + permission;
                        }
                        else
                        {
                            item.ImageIndex = smallImageList.Images.IndexOfKey("file");
                            permission = "-" + permission;
                        }
                        item.SubItems.Add(infos[1] + "B");
                        item.SubItems.Add(permission);
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
                    Marshal.FreeHGlobal(m.WParam);
                    break;

                case cls_Msg.ASSIGN_PATH:
                    this.currentPath = tstb_CurrentPath.Text;
                    break;

                case cls_Msg.CREATE_FOLDER:
                    string target = Marshal.PtrToStringAnsi(m.WParam);
                    CreateFolder(target);
                    Marshal.FreeHGlobal(m.WParam);
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
