
namespace RemoteDesktopSharer
{
    partial class FrmSharer
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
            this.txt_Port = new System.Windows.Forms.TextBox();
            this.btn_Share = new System.Windows.Forms.Button();
            this.rtb_Log = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_Password = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.nud_ImageQuality = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nud_ImageQuality)).BeginInit();
            this.SuspendLayout();
            // 
            // txt_Port
            // 
            this.txt_Port.Location = new System.Drawing.Point(67, 10);
            this.txt_Port.Name = "txt_Port";
            this.txt_Port.Size = new System.Drawing.Size(45, 21);
            this.txt_Port.TabIndex = 0;
            this.txt_Port.Text = "5001";
            // 
            // btn_Share
            // 
            this.btn_Share.Location = new System.Drawing.Point(349, 9);
            this.btn_Share.Name = "btn_Share";
            this.btn_Share.Size = new System.Drawing.Size(64, 23);
            this.btn_Share.TabIndex = 2;
            this.btn_Share.Text = "开始分享";
            this.btn_Share.UseVisualStyleBackColor = true;
            this.btn_Share.Click += new System.EventHandler(this.btn_Share_Click);
            // 
            // rtb_Log
            // 
            this.rtb_Log.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtb_Log.Location = new System.Drawing.Point(12, 40);
            this.rtb_Log.Name = "rtb_Log";
            this.rtb_Log.Size = new System.Drawing.Size(555, 398);
            this.rtb_Log.TabIndex = 3;
            this.rtb_Log.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "监听端口";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(118, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "访问密码";
            // 
            // txt_Password
            // 
            this.txt_Password.Location = new System.Drawing.Point(173, 10);
            this.txt_Password.Name = "txt_Password";
            this.txt_Password.Size = new System.Drawing.Size(59, 21);
            this.txt_Password.TabIndex = 5;
            this.txt_Password.Text = "123456";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(238, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "图像质量";
            // 
            // nud_ImageQuality
            // 
            this.nud_ImageQuality.Location = new System.Drawing.Point(297, 10);
            this.nud_ImageQuality.Name = "nud_ImageQuality";
            this.nud_ImageQuality.Size = new System.Drawing.Size(40, 21);
            this.nud_ImageQuality.TabIndex = 9;
            this.nud_ImageQuality.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // FrmSharer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(579, 450);
            this.Controls.Add(this.nud_ImageQuality);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_Password);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rtb_Log);
            this.Controls.Add(this.btn_Share);
            this.Controls.Add(this.txt_Port);
            this.DoubleBuffered = true;
            this.Name = "FrmSharer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "分享桌面服务端";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmSharer_FormClosing);
            this.Load += new System.EventHandler(this.FrmSharer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nud_ImageQuality)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_Port;
        private System.Windows.Forms.Button btn_Share;
        private System.Windows.Forms.RichTextBox rtb_Log;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_Password;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nud_ImageQuality;
    }
}

