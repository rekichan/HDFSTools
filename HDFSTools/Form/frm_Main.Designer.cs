
namespace HDFSTools
{
    partial class frm_Main
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_Main));
            this.ts_Main = new System.Windows.Forms.ToolStrip();
            this.tsb_Backward = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tsb_Forward = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tssb_File = new System.Windows.Forms.ToolStripSplitButton();
            this.tsmi_UploadFile = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_DownloadFile = new System.Windows.Forms.ToolStripMenuItem();
            this.tssb_View = new System.Windows.Forms.ToolStripSplitButton();
            this.tsmi_BigIcon = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_SmallIcon = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_TailIcon = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_ListIcon = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_DetailIcon = new System.Windows.Forms.ToolStripMenuItem();
            this.tssb_SysConfig = new System.Windows.Forms.ToolStripSplitButton();
            this.tsmi_ConnectConfig = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_Connect = new System.Windows.Forms.ToolStripMenuItem();
            this.tv_FolderList = new System.Windows.Forms.TreeView();
            this.ts_Path = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.tstb_CurrentPath = new System.Windows.Forms.ToolStripTextBox();
            this.tsb_Enter = new System.Windows.Forms.ToolStripButton();
            this.tsb_Refresh = new System.Windows.Forms.ToolStripButton();
            this.tsb_Search = new System.Windows.Forms.ToolStripButton();
            this.tstb_Search = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.lv_ShowFile = new System.Windows.Forms.ListView();
            this.sc_Main = new System.Windows.Forms.SplitContainer();
            this.ts_Main.SuspendLayout();
            this.ts_Path.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sc_Main)).BeginInit();
            this.sc_Main.Panel1.SuspendLayout();
            this.sc_Main.Panel2.SuspendLayout();
            this.sc_Main.SuspendLayout();
            this.SuspendLayout();
            // 
            // ts_Main
            // 
            this.ts_Main.AutoSize = false;
            this.ts_Main.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ts_Main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsb_Backward,
            this.toolStripLabel1,
            this.tsb_Forward,
            this.toolStripSeparator1,
            this.tssb_File,
            this.tssb_View,
            this.tssb_SysConfig});
            this.ts_Main.Location = new System.Drawing.Point(0, 0);
            this.ts_Main.Name = "ts_Main";
            this.ts_Main.Size = new System.Drawing.Size(1006, 40);
            this.ts_Main.TabIndex = 0;
            this.ts_Main.Text = "toolStrip1";
            // 
            // tsb_Backward
            // 
            this.tsb_Backward.AutoSize = false;
            this.tsb_Backward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_Backward.Enabled = false;
            this.tsb_Backward.Image = global::HDFSTools.Properties.Resources.left;
            this.tsb_Backward.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsb_Backward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_Backward.Name = "tsb_Backward";
            this.tsb_Backward.Size = new System.Drawing.Size(29, 37);
            this.tsb_Backward.Text = "返回";
            this.tsb_Backward.Click += new System.EventHandler(this.tsb_Backward_Click);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(17, 37);
            this.toolStripLabel1.Text = "  ";
            // 
            // tsb_Forward
            // 
            this.tsb_Forward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_Forward.Enabled = false;
            this.tsb_Forward.Image = global::HDFSTools.Properties.Resources.right;
            this.tsb_Forward.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsb_Forward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_Forward.Name = "tsb_Forward";
            this.tsb_Forward.Size = new System.Drawing.Size(30, 37);
            this.tsb_Forward.Text = "前进";
            this.tsb_Forward.Click += new System.EventHandler(this.tsb_Forward_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 40);
            // 
            // tssb_File
            // 
            this.tssb_File.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmi_UploadFile,
            this.tsmi_DownloadFile});
            this.tssb_File.Enabled = false;
            this.tssb_File.Image = global::HDFSTools.Properties.Resources.document;
            this.tssb_File.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tssb_File.Name = "tssb_File";
            this.tssb_File.Size = new System.Drawing.Size(78, 37);
            this.tssb_File.Text = "文件";
            // 
            // tsmi_UploadFile
            // 
            this.tsmi_UploadFile.Image = global::HDFSTools.Properties.Resources.upload;
            this.tsmi_UploadFile.Name = "tsmi_UploadFile";
            this.tsmi_UploadFile.Size = new System.Drawing.Size(224, 26);
            this.tsmi_UploadFile.Text = "上传文件";
            this.tsmi_UploadFile.Click += new System.EventHandler(this.tsmi_UploadFile_Click);
            // 
            // tsmi_DownloadFile
            // 
            this.tsmi_DownloadFile.Image = global::HDFSTools.Properties.Resources.download;
            this.tsmi_DownloadFile.Name = "tsmi_DownloadFile";
            this.tsmi_DownloadFile.Size = new System.Drawing.Size(224, 26);
            this.tsmi_DownloadFile.Text = "下载文件";
            this.tsmi_DownloadFile.Click += new System.EventHandler(this.tsmi_DownloadFile_Click);
            // 
            // tssb_View
            // 
            this.tssb_View.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmi_BigIcon,
            this.tsmi_SmallIcon,
            this.tsmi_TailIcon,
            this.tsmi_ListIcon,
            this.tsmi_DetailIcon});
            this.tssb_View.Image = global::HDFSTools.Properties.Resources.taillImage;
            this.tssb_View.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tssb_View.Name = "tssb_View";
            this.tssb_View.Size = new System.Drawing.Size(78, 37);
            this.tssb_View.Text = "视图";
            // 
            // tsmi_BigIcon
            // 
            this.tsmi_BigIcon.Image = global::HDFSTools.Properties.Resources.bigImage;
            this.tsmi_BigIcon.Name = "tsmi_BigIcon";
            this.tsmi_BigIcon.Size = new System.Drawing.Size(224, 26);
            this.tsmi_BigIcon.Text = "大图标";
            this.tsmi_BigIcon.Click += new System.EventHandler(this.tsmi_BigIcon_Click);
            // 
            // tsmi_SmallIcon
            // 
            this.tsmi_SmallIcon.Image = global::HDFSTools.Properties.Resources.smallImage;
            this.tsmi_SmallIcon.Name = "tsmi_SmallIcon";
            this.tsmi_SmallIcon.Size = new System.Drawing.Size(224, 26);
            this.tsmi_SmallIcon.Text = "小图标";
            this.tsmi_SmallIcon.Click += new System.EventHandler(this.tsmi_SmallIcon_Click);
            // 
            // tsmi_TailIcon
            // 
            this.tsmi_TailIcon.Image = global::HDFSTools.Properties.Resources.taillImage;
            this.tsmi_TailIcon.Name = "tsmi_TailIcon";
            this.tsmi_TailIcon.Size = new System.Drawing.Size(224, 26);
            this.tsmi_TailIcon.Text = "平铺";
            this.tsmi_TailIcon.Click += new System.EventHandler(this.tsmi_TailIcon_Click);
            // 
            // tsmi_ListIcon
            // 
            this.tsmi_ListIcon.Image = global::HDFSTools.Properties.Resources.ListImage;
            this.tsmi_ListIcon.Name = "tsmi_ListIcon";
            this.tsmi_ListIcon.Size = new System.Drawing.Size(224, 26);
            this.tsmi_ListIcon.Text = "列表";
            this.tsmi_ListIcon.Click += new System.EventHandler(this.tsmi_ListIcon_Click);
            // 
            // tsmi_DetailIcon
            // 
            this.tsmi_DetailIcon.Image = global::HDFSTools.Properties.Resources.DetailImage;
            this.tsmi_DetailIcon.Name = "tsmi_DetailIcon";
            this.tsmi_DetailIcon.Size = new System.Drawing.Size(224, 26);
            this.tsmi_DetailIcon.Text = "详细信息";
            this.tsmi_DetailIcon.Click += new System.EventHandler(this.tsmi_DetailIcon_Click);
            // 
            // tssb_SysConfig
            // 
            this.tssb_SysConfig.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmi_ConnectConfig,
            this.tsmi_Connect});
            this.tssb_SysConfig.Image = global::HDFSTools.Properties.Resources.setting;
            this.tssb_SysConfig.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tssb_SysConfig.Name = "tssb_SysConfig";
            this.tssb_SysConfig.Size = new System.Drawing.Size(108, 37);
            this.tssb_SysConfig.Text = "系统配置";
            // 
            // tsmi_ConnectConfig
            // 
            this.tsmi_ConnectConfig.Image = global::HDFSTools.Properties.Resources.syssetting;
            this.tsmi_ConnectConfig.Name = "tsmi_ConnectConfig";
            this.tsmi_ConnectConfig.Size = new System.Drawing.Size(224, 26);
            this.tsmi_ConnectConfig.Text = "连接属性";
            this.tsmi_ConnectConfig.Click += new System.EventHandler(this.tsmi_ConnectConfig_Click);
            // 
            // tsmi_Connect
            // 
            this.tsmi_Connect.Image = global::HDFSTools.Properties.Resources.connect;
            this.tsmi_Connect.Name = "tsmi_Connect";
            this.tsmi_Connect.Size = new System.Drawing.Size(224, 26);
            this.tsmi_Connect.Text = "连接";
            this.tsmi_Connect.Click += new System.EventHandler(this.tsmi_Connect_Click);
            // 
            // tv_FolderList
            // 
            this.tv_FolderList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tv_FolderList.Location = new System.Drawing.Point(0, 0);
            this.tv_FolderList.Name = "tv_FolderList";
            this.tv_FolderList.PathSeparator = "/";
            this.tv_FolderList.ShowLines = false;
            this.tv_FolderList.Size = new System.Drawing.Size(250, 653);
            this.tv_FolderList.TabIndex = 1;
            this.tv_FolderList.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.tv_FolderList_AfterCollapse);
            this.tv_FolderList.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.tv_FolderList_AfterExpand);
            this.tv_FolderList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tv_FolderList_AfterSelect);
            // 
            // ts_Path
            // 
            this.ts_Path.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ts_Path.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel2,
            this.tstb_CurrentPath,
            this.tsb_Enter,
            this.tsb_Refresh,
            this.tsb_Search,
            this.tstb_Search,
            this.toolStripLabel3});
            this.ts_Path.Location = new System.Drawing.Point(0, 40);
            this.ts_Path.Name = "ts_Path";
            this.ts_Path.Size = new System.Drawing.Size(1006, 27);
            this.ts_Path.TabIndex = 2;
            this.ts_Path.Text = "toolStrip2";
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(54, 24);
            this.toolStripLabel2.Text = "地址：";
            // 
            // tstb_CurrentPath
            // 
            this.tstb_CurrentPath.Enabled = false;
            this.tstb_CurrentPath.Name = "tstb_CurrentPath";
            this.tstb_CurrentPath.Size = new System.Drawing.Size(880, 27);
            // 
            // tsb_Enter
            // 
            this.tsb_Enter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_Enter.Enabled = false;
            this.tsb_Enter.Image = global::HDFSTools.Properties.Resources.enter;
            this.tsb_Enter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_Enter.Name = "tsb_Enter";
            this.tsb_Enter.Size = new System.Drawing.Size(29, 24);
            this.tsb_Enter.Text = "跳转";
            this.tsb_Enter.Click += new System.EventHandler(this.tsb_Enter_Click);
            // 
            // tsb_Refresh
            // 
            this.tsb_Refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_Refresh.Enabled = false;
            this.tsb_Refresh.Image = global::HDFSTools.Properties.Resources.refresh;
            this.tsb_Refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_Refresh.Name = "tsb_Refresh";
            this.tsb_Refresh.Size = new System.Drawing.Size(29, 24);
            this.tsb_Refresh.Text = "刷新";
            this.tsb_Refresh.Click += new System.EventHandler(this.tsb_Refresh_Click);
            // 
            // tsb_Search
            // 
            this.tsb_Search.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsb_Search.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_Search.Enabled = false;
            this.tsb_Search.Image = global::HDFSTools.Properties.Resources.search;
            this.tsb_Search.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_Search.Name = "tsb_Search";
            this.tsb_Search.Size = new System.Drawing.Size(29, 24);
            this.tsb_Search.Text = "搜索";
            this.tsb_Search.Visible = false;
            // 
            // tstb_Search
            // 
            this.tstb_Search.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tstb_Search.Enabled = false;
            this.tstb_Search.Name = "tstb_Search";
            this.tstb_Search.Size = new System.Drawing.Size(200, 27);
            this.tstb_Search.Visible = false;
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(54, 20);
            this.toolStripLabel3.Text = "搜索：";
            this.toolStripLabel3.Visible = false;
            // 
            // lv_ShowFile
            // 
            this.lv_ShowFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lv_ShowFile.HideSelection = false;
            this.lv_ShowFile.Location = new System.Drawing.Point(0, 0);
            this.lv_ShowFile.Name = "lv_ShowFile";
            this.lv_ShowFile.Size = new System.Drawing.Size(752, 653);
            this.lv_ShowFile.TabIndex = 3;
            this.lv_ShowFile.UseCompatibleStateImageBehavior = false;
            this.lv_ShowFile.View = System.Windows.Forms.View.Details;
            this.lv_ShowFile.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lv_ShowFile_MouseDoubleClick);
            // 
            // sc_Main
            // 
            this.sc_Main.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.sc_Main.Location = new System.Drawing.Point(0, 70);
            this.sc_Main.Name = "sc_Main";
            // 
            // sc_Main.Panel1
            // 
            this.sc_Main.Panel1.Controls.Add(this.tv_FolderList);
            // 
            // sc_Main.Panel2
            // 
            this.sc_Main.Panel2.Controls.Add(this.lv_ShowFile);
            this.sc_Main.Size = new System.Drawing.Size(1006, 653);
            this.sc_Main.SplitterDistance = 250;
            this.sc_Main.TabIndex = 4;
            // 
            // frm_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1006, 723);
            this.Controls.Add(this.sc_Main);
            this.Controls.Add(this.ts_Path);
            this.Controls.Add(this.ts_Main);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1024, 768);
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "frm_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "HDFS Tools";
            this.Load += new System.EventHandler(this.frm_Main_Load);
            this.ts_Main.ResumeLayout(false);
            this.ts_Main.PerformLayout();
            this.ts_Path.ResumeLayout(false);
            this.ts_Path.PerformLayout();
            this.sc_Main.Panel1.ResumeLayout(false);
            this.sc_Main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sc_Main)).EndInit();
            this.sc_Main.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip ts_Main;
        private System.Windows.Forms.ToolStripSplitButton tssb_SysConfig;
        private System.Windows.Forms.ToolStripMenuItem tsmi_ConnectConfig;
        private System.Windows.Forms.ToolStripMenuItem tsmi_Connect;
        private System.Windows.Forms.ToolStripButton tsb_Backward;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsb_Forward;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripSplitButton tssb_View;
        private System.Windows.Forms.ToolStripMenuItem tsmi_BigIcon;
        private System.Windows.Forms.ToolStripMenuItem tsmi_SmallIcon;
        private System.Windows.Forms.ToolStripMenuItem tsmi_TailIcon;
        private System.Windows.Forms.ToolStripMenuItem tsmi_ListIcon;
        private System.Windows.Forms.ToolStripMenuItem tsmi_DetailIcon;
        private System.Windows.Forms.TreeView tv_FolderList;
        private System.Windows.Forms.ToolStrip ts_Path;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripTextBox tstb_CurrentPath;
        private System.Windows.Forms.ToolStripButton tsb_Refresh;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripTextBox tstb_Search;
        private System.Windows.Forms.ToolStripButton tsb_Search;
        private System.Windows.Forms.ListView lv_ShowFile;
        private System.Windows.Forms.ToolStripSplitButton tssb_File;
        private System.Windows.Forms.ToolStripMenuItem tsmi_UploadFile;
        private System.Windows.Forms.ToolStripMenuItem tsmi_DownloadFile;
        private System.Windows.Forms.ToolStripButton tsb_Enter;
        private System.Windows.Forms.SplitContainer sc_Main;
    }
}

