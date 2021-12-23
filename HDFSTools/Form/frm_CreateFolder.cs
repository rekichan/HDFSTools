using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace HDFSTools
{
    public partial class frm_CreateFolder : Form
    {

        #region Properties
        string currentPath;
        #endregion

        #region API
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void PostMessage(IntPtr hWnd, int msg, IntPtr wParam, int lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        #endregion

        #region Constructor
        public frm_CreateFolder(string currentPath)
        {
            this.currentPath = currentPath;
            InitializeComponent();
        }
        #endregion

        #region FormEvent
        private void frm_CreateFolder_Load(object sender, EventArgs e)
        {
            txt_CurrentPath.Text = currentPath;
            txt_TargetPath.Focus();
            txt_TargetPath.SelectAll();
        }
        #endregion

        #region Event
        private void btn_CommitCreate_Click(object sender, EventArgs e)
        {
            string target = txt_TargetPath.Text;
            if (target.StartsWith("/"))
                target = target.Substring(1);

            bool regex = Regex.IsMatch(target, "^(\\w+/?)+$");
            if (!regex)
            {
                MessageBox.Show("输入的目标路径不符合命名规范!", "Warn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (target.EndsWith("/"))
                target = target.Substring(0, target.Length - 1);

            target = currentPath.EndsWith("/") ? currentPath + target : currentPath + "/" + target;
            if (target.StartsWith("/"))
                target = target.Substring(1);

            PostMess(cls_Msg.hwndFrmMain, cls_Msg.CREATE_FOLDER, target);
            this.Close();
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Function
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

    }
}
