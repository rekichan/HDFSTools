using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HDFSTools
{
    public partial class frm_Config : Form
    {

        #region Properties
        private cls_Config config;
        private string path = Application.StartupPath + @"\parameter\config.ini";
        #endregion

        #region Constructor
        public frm_Config()
        {
            InitializeComponent();
        }
        #endregion

        #region FormEvent
        private void frm_Config_Load(object sender, EventArgs e)
        {
            InitConfig();
        }
        #endregion

        #region Event
        private void btn_Save_Click(object sender, EventArgs e)
        {
            string p = txt_Port.Text;
            if (!RegexPort(p))
            {
                MessageBox.Show("端口号有误!", "Err", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult dr = MessageBox.Show("确定要保存参数?", "Q", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(dr == DialogResult.Yes)
            {
                SaveConfig();
                MessageBox.Show("保存成功!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Function
        /// <summary>
        /// 初始化配置参数
        /// </summary>
        private void InitConfig()
        {
            config = cls_Config.getInstance(path);

            cls_Config.port = config.IniReadValue("config", "port", "9870");
            cls_Config.host = config.IniReadValue("config", "host", "localhost");
            cls_Config.userName = config.IniReadValue("config", "userName", "user");

            txt_Host.Text = cls_Config.host;
            txt_Port.Text = cls_Config.port;
            txt_UserName.Text = cls_Config.userName;
        }

        /// <summary>
        /// 保存配置参数
        /// </summary>
        private void SaveConfig()
        {
            cls_Config.host = txt_Host.Text;
            cls_Config.port = txt_Port.Text;
            cls_Config.userName = txt_UserName.Text;

            config.IniWriteValue("config", "port", cls_Config.port);
            config.IniWriteValue("config", "host", cls_Config.host);
            config.IniWriteValue("config", "userName", cls_Config.userName);
        }

        /// <summary>
        /// 正则表达式判断端口号
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool RegexPort(string text)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(text, "^((6553[0-5])|(655[0-2][0-9])|(65[0-4][0-9]{2})|(6[0-4][0-9]{3})|([1-5][0-9]{4})|([0-5]{0,5})|([0-9]{1,4}))$");
        }
        #endregion

    }
}
