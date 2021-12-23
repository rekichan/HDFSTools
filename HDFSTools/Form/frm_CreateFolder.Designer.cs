
namespace HDFSTools
{
    partial class frm_CreateFolder
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_CreateFolder));
            this.txt_CurrentPath = new System.Windows.Forms.TextBox();
            this.txt_TargetPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_CommitCreate = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txt_CurrentPath
            // 
            this.txt_CurrentPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.txt_CurrentPath.Location = new System.Drawing.Point(112, 38);
            this.txt_CurrentPath.Name = "txt_CurrentPath";
            this.txt_CurrentPath.ReadOnly = true;
            this.txt_CurrentPath.Size = new System.Drawing.Size(339, 27);
            this.txt_CurrentPath.TabIndex = 1;
            // 
            // txt_TargetPath
            // 
            this.txt_TargetPath.Location = new System.Drawing.Point(112, 91);
            this.txt_TargetPath.Name = "txt_TargetPath";
            this.txt_TargetPath.Size = new System.Drawing.Size(339, 27);
            this.txt_TargetPath.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "当前目录：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "目标路径：";
            // 
            // btn_CommitCreate
            // 
            this.btn_CommitCreate.Location = new System.Drawing.Point(82, 143);
            this.btn_CommitCreate.Name = "btn_CommitCreate";
            this.btn_CommitCreate.Size = new System.Drawing.Size(100, 35);
            this.btn_CommitCreate.TabIndex = 4;
            this.btn_CommitCreate.Text = "确认创建";
            this.btn_CommitCreate.UseVisualStyleBackColor = true;
            this.btn_CommitCreate.Click += new System.EventHandler(this.btn_CommitCreate_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(301, 143);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(100, 35);
            this.btn_Cancel.TabIndex = 5;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // frm_CreateFolder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 205);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_CommitCreate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_TargetPath);
            this.Controls.Add(this.txt_CurrentPath);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(500, 250);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 250);
            this.Name = "frm_CreateFolder";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "创建文件夹";
            this.Load += new System.EventHandler(this.frm_CreateFolder_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_CurrentPath;
        private System.Windows.Forms.TextBox txt_TargetPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_CommitCreate;
        private System.Windows.Forms.Button btn_Cancel;
    }
}