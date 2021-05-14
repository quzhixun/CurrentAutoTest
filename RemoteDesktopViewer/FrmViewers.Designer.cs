
namespace RemoteDesktopViewer
{
    partial class FrmViewers
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
            this.tlp_Settings = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btn_allStart = new System.Windows.Forms.Button();
            this.btn_allConnect = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tlp_RemoteDesktops = new System.Windows.Forms.TableLayoutPanel();
            this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.tlp_Settings.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlp_Settings
            // 
            this.tlp_Settings.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tlp_Settings.ColumnCount = 1;
            this.tlp_Settings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp_Settings.Controls.Add(this.panel2, 0, 0);
            this.tlp_Settings.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tlp_Settings.Location = new System.Drawing.Point(21, 54);
            this.tlp_Settings.Name = "tlp_Settings";
            this.tlp_Settings.RowCount = 2;
            this.tlp_Settings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.tlp_Settings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlp_Settings.Size = new System.Drawing.Size(279, 705);
            this.tlp_Settings.TabIndex = 44;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btn_allStart);
            this.panel2.Controls.Add(this.btn_allConnect);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(4, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(271, 41);
            this.panel2.TabIndex = 7;
            // 
            // btn_allStart
            // 
            this.btn_allStart.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_allStart.Location = new System.Drawing.Point(185, 10);
            this.btn_allStart.Name = "btn_allStart";
            this.btn_allStart.Size = new System.Drawing.Size(75, 23);
            this.btn_allStart.TabIndex = 1;
            this.btn_allStart.Text = "启动";
            this.btn_allStart.UseVisualStyleBackColor = true;
            // 
            // btn_allConnect
            // 
            this.btn_allConnect.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_allConnect.Location = new System.Drawing.Point(12, 10);
            this.btn_allConnect.Name = "btn_allConnect";
            this.btn_allConnect.Size = new System.Drawing.Size(75, 23);
            this.btn_allConnect.TabIndex = 0;
            this.btn_allConnect.Text = "连接";
            this.btn_allConnect.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 52);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(271, 650);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // tlp_RemoteDesktops
            // 
            this.tlp_RemoteDesktops.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tlp_RemoteDesktops.ColumnCount = 5;
            this.tlp_RemoteDesktops.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlp_RemoteDesktops.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlp_RemoteDesktops.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlp_RemoteDesktops.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlp_RemoteDesktops.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlp_RemoteDesktops.Location = new System.Drawing.Point(340, 54);
            this.tlp_RemoteDesktops.Name = "tlp_RemoteDesktops";
            this.tlp_RemoteDesktops.RowCount = 2;
            this.tlp_RemoteDesktops.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlp_RemoteDesktops.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlp_RemoteDesktops.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlp_RemoteDesktops.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlp_RemoteDesktops.Size = new System.Drawing.Size(1105, 705);
            this.tlp_RemoteDesktops.TabIndex = 45;
            // 
            // dockPanel
            // 
            this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockPanel.DockLeftPortion = 280D;
            this.dockPanel.Location = new System.Drawing.Point(0, 0);
            this.dockPanel.Name = "dockPanel";
            this.dockPanel.Size = new System.Drawing.Size(1487, 807);
            this.dockPanel.TabIndex = 49;
            // 
            // FrmViewers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1487, 807);
            this.Controls.Add(this.tlp_RemoteDesktops);
            this.Controls.Add(this.tlp_Settings);
            this.Controls.Add(this.dockPanel);
            this.DoubleBuffered = true;
            this.IsMdiContainer = true;
            this.Name = "FrmViewers";
            this.Text = "远程桌面";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmViewer_Load);
            this.tlp_Settings.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlp_Settings;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btn_allStart;
        private System.Windows.Forms.Button btn_allConnect;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tlp_RemoteDesktops;
        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
    }
}

