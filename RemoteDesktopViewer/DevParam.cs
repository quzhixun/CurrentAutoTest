using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteDesktopViewer
{
    public partial class DevParam : UserControl
    {
        public string IP
        {
            get { return txt_ip.Text; }
            set
            {
                txt_ip.Text = value;
            }
        }

        public string Port
        {
            get { return txt_port.Text; }
            set
            {
                txt_port.Text = value;
            }
        }

        public string VmName
        {
            get { return txt_vmName.Text; }
            set
            {
                txt_vmName.Text = value;
            }
        }
        public string Password
        {
            get { return txt_password.Text; }
            set
            {
                txt_password.Text = value;
            }
        }

        public DevParam()
        {
            InitializeComponent();
        }
    }
}
