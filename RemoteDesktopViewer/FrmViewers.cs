using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using WeifenLuo.WinFormsUI.Docking;

namespace RemoteDesktopViewer
{

    public partial class FrmViewers : DockContent
    {
        public FrmViewers()
        {
            InitializeComponent();

            DockContent dcLeft = new DockContent();
            dcLeft.CloseButtonVisible = false;
            dcLeft.Text = "连接设置";
            dcLeft.Show(dockPanel);
            dcLeft.DockTo(dockPanel, DockStyle.Left);
            dcLeft.DockState = DockState.DockLeftAutoHide;
            dcLeft.Controls.Add(tlp_Settings);
            tlp_Settings.Dock = DockStyle.Fill;

            DockContent dcFill = new DockContent();
            dcFill.CloseButtonVisible = false;
            dcFill.Show(dockPanel);
            dcFill.DockTo(dockPanel, DockStyle.Fill);
            dcFill.Text = "集中控制";
            dcFill.Controls.Add(tlp_RemoteDesktops);
            tlp_RemoteDesktops.Dock = DockStyle.Fill;
        }

        private void FrmViewer_Load(object sender, EventArgs e)
        {
            XmlDocument xmldoc = new XmlDocument();
            try
            {
                string filePath = AppDomain.CurrentDomain.BaseDirectory + "ConnParams.xml";
                if (!File.Exists(filePath))
                {
                    // 若 Data 目录不存在则创建
                    xmldoc.LoadXml(
                        @"<vms>
	                        <vm>
		                        <ip>192.168.3.103</ip>
		                        <port>5002</port>
		                        <vmName>1#</vmName>
		                        <password>123456</password>
	                        </vm>
                          </vms>");
                    xmldoc.Save(filePath);
                }
                xmldoc.Load(filePath);
                XmlNode node = xmldoc.SelectSingleNode("vms");
                if (node != null)
                {
                    foreach (XmlNode xnode in node.ChildNodes)
                    {
                        DevParam para = new DevParam();
                        para.IP = xnode["ip"].InnerText;
                        para.Port = xnode["port"].InnerText;
                        para.VmName = xnode["vmName"].InnerText;
                        para.Password = xnode["password"].InnerText;
                        flowLayoutPanel1.Controls.Add(para);

                        viewerControl viewer = new viewerControl();
                        viewer.IP = para.IP;
                        viewer.Port = para.Port;
                        viewer.VmName = para.VmName;
                        viewer.Password = para.Password;
                        viewer.Dock = DockStyle.Fill;

                        tlp_RemoteDesktops.Controls.Add(viewer);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_allConnect_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < tlp_RemoteDesktops.Controls.Count; i++)
            {
                viewerControl contrl = (viewerControl)tlp_RemoteDesktops.Controls[i];
                contrl.Connect();
            }
        }

        private void btn_allStart_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < tlp_RemoteDesktops.Controls.Count; i++)
            {
                viewerControl contrl = (viewerControl)tlp_RemoteDesktops.Controls[i];
                contrl.Start();
            }
        }
    }


}
