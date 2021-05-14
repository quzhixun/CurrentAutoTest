using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using CH.Base;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;

namespace RemoteDesktopViewer
{
    public partial class viewerControl : UserControl, IMessageFilter
    {
        private bool _isTcpConnected = false;
        private bool _loginSuccess = false;
        private TcpClient tcpClient = null;
        private NetworkStream ns = null;
        private List<byte> receiveDataBuffer = new List<byte>();
        private DateTime receiveDataStartTime = DateTime.Now;
        private int _frameTimeout = 50;
        private bool viewerIsActivated = false;
        private int remoteDesktopWidth = 0;
        private int remoteDesktopHeight = 0;
        private int oldMouseX = -1;
        private int oldMouseY = -1;
        private int oldIndex = 0;
        private Control oldParent = null;

        public string IP = "";
        public string Port = "";
        public string Password = "";
        public bool IsFullScreen = false;

        public string VmName
        {
            get { return label3.Text; }
            set
            {
                label3.Text = value;
            }
        }


        public viewerControl()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;//quzhixun
        }

        public viewerControl(string ip, string port, string user, string pass, bool isFullScreen = false)
        {
            InitializeComponent();
            IP = ip;
            Port = port;
            VmName = user;
            Password = pass;
            IsFullScreen = false;
            if (isFullScreen)
            {
                btn_FullScreen.Text = "退出";
            }
        }

        private void viewerControl_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                Application.AddMessageFilter(this);
                //cbb_SizeMode.SelectedIndex = 1;
                Thread thRecvFromServer = new Thread(new ThreadStart(RecvFromServer));
                thRecvFromServer.IsBackground = true;
                thRecvFromServer.Start();
            }
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Focus();
            viewerIsActivated = true;
            Win32API.ShowCursor(0);
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            viewerIsActivated = false;
            Win32API.ShowCursor(1);
        }

        public bool PreFilterMessage(ref System.Windows.Forms.Message m)
        {
            if (btn_FullScreen.Text == "退出")
            {
                switch (m.Msg)
                {
                    case Win32API.WM_KEYDOWN:        //键盘按下事件　
                    case Win32API.WM_SYSKEYDOWN:
                    case Win32API.WM_KEYUP:          //键盘弹起事件
                    case Win32API.WM_SYSKEYUP:
                        if (m.WParam.ToInt32() == (int)Keys.Escape)
                        {
                            btn_FullScreen.PerformClick();
                            return true;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (!viewerIsActivated || !_loginSuccess)
            {
                return false;
            }
            try
            {
                switch (m.Msg)
                {
                    case Win32API.WM_KEYDOWN:        //键盘按下事件　
                    case Win32API.WM_SYSKEYDOWN:
                        SendData(Encoding.ASCII.GetBytes("{'Msg':" + m.Msg + ",'WParam':" + m.WParam.ToInt32() + "}"));
                        return true;
                    case Win32API.WM_KEYUP:          //键盘弹起事件
                    case Win32API.WM_SYSKEYUP:
                        SendData(Encoding.ASCII.GetBytes("{'Msg':" + m.Msg + ",'WParam':" + m.WParam.ToInt32() + "}"));
                        return true;
                    case Win32API.WM_MOUSEMOVE:      //鼠标移动
                        int x = Win32API.GET_X_LPARAM(m.LParam);
                        int y = Win32API.GET_Y_LPARAM(m.LParam);
                        if (oldMouseX == x && oldMouseY == y)
                        {
                            return true;
                        }
                        oldMouseX = x;
                        oldMouseY = y;
                        //if (pictureBox1.Width != remoteDesktopWidth && pictureBox1.Height != remoteDesktopHeight)
                        if (pictureBox1.Width != remoteDesktopWidth || pictureBox1.Height != remoteDesktopHeight)
                        {
                            if (pictureBox1.SizeMode == PictureBoxSizeMode.Zoom)
                            {
                                PropertyInfo imageRectangleProperty = pictureBox1.GetType().GetProperty("ImageRectangle", BindingFlags.Instance | BindingFlags.NonPublic);
                                Rectangle r = (Rectangle)imageRectangleProperty.GetValue(pictureBox1, null);
                                int zoomPicWidth = r.Width;
                                int zoomPicHeight = r.Height;
                                if (pictureBox1.Width > zoomPicWidth)
                                {
                                    int halfWidth = (pictureBox1.Width - zoomPicWidth) / 2;
                                    if (x <= halfWidth)
                                    {
                                        x = 0;
                                    }
                                    else if (x >= (halfWidth + zoomPicWidth))
                                    {
                                        x = zoomPicWidth;
                                    }
                                    else
                                    {
                                        x -= halfWidth;
                                    }
                                }
                                if (pictureBox1.Height > zoomPicHeight)
                                {
                                    int halfHeight = (pictureBox1.Height - zoomPicHeight) / 2;
                                    if (y <= halfHeight)
                                    {
                                        y = 0;
                                    }
                                    else if (y >= (halfHeight + zoomPicHeight))
                                    {
                                        y = zoomPicHeight;
                                    }
                                    else
                                    {
                                        y -= halfHeight;
                                    }
                                }
                                x = (int)(x * remoteDesktopWidth / (float)zoomPicWidth);
                                y = (int)(y * remoteDesktopHeight / (float)zoomPicHeight);
                            }
                            else
                            {
                                x = (int)(x * remoteDesktopWidth / (float)pictureBox1.Width);
                                y = (int)(y * remoteDesktopHeight / (float)pictureBox1.Height);
                            }
                        }
                        SendData(Encoding.ASCII.GetBytes("{'Msg':" + m.Msg + ",'X':" + x + ",'Y':" + y + "}"));
                        return true;
                    case Win32API.WM_LBUTTONDOWN:    //鼠标左键按下
                    case Win32API.WM_RBUTTONDOWN:    //鼠标右键按下
                    case Win32API.WM_MBUTTONDOWN:    //鼠标中键按下
                    case Win32API.WM_LBUTTONUP:      //鼠标左键弹起
                    case Win32API.WM_RBUTTONUP:      //鼠标右键弹起
                    case Win32API.WM_MBUTTONUP:      //鼠标中键弹起
                    case Win32API.WM_LBUTTONDBLCLK:  //鼠标左键双击
                    case Win32API.WM_RBUTTONDBLCLK:  //鼠标左键双击
                    case Win32API.WM_MBUTTONDBLCLK:  //鼠标左键双击
                        SendData(Encoding.ASCII.GetBytes("{'Msg':" + m.Msg + "}"));
                        return true;
                    case Win32API.WM_MOUSEWHEEL:     //鼠标滚轮
                        SendData(Encoding.ASCII.GetBytes("{'Msg':" + m.Msg + ",'WParam':" + Win32API.GET_WHEEL_DELTA_WPARAM(m.WParam) + "}"));
                        return true;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

            }
            return false;    //返回false则消息未被裁取，系统会处理
        }
        private void btn_Connect_Click(object sender, EventArgs e)
        {
            if (tcpClient != null)
            {
                tcpClient.Close();
            }
            if (btn_Connect.Text == "连接")
            {
                tcpClient = new TcpClient();
                try
                {
                    tcpClient.BeginConnect(IP, Convert.ToInt32(Port), new AsyncCallback(ConnectCallback), tcpClient);
                }
                catch (Exception ex)
                {
                    //ConnectFailed?.Invoke(this, ex.Message);
                }
            }
            else
            {
                btn_Connect.Text = "连接";
                pictureBox1.Image = null;
                _loginSuccess = false;
            }
        }

        public void Connect()
        {
            btn_Connect.PerformClick();
        }

        void ConnectCallback(IAsyncResult iar)
        {
            TcpClient tcp = iar.AsyncState as TcpClient;
            try
            {
                if (tcp.Client == null)
                {
                    _isTcpConnected = false;
                    _loginSuccess = false;
                    this.Invoke(new Action(() =>
                    {
                        btn_Connect.Text = "连接";
                        pictureBox1.Image = null;
                    }));
                    return;
                }
                tcp.EndConnect(iar);
                if (tcp.Connected)
                {
                    _isTcpConnected = true;
                    ns = tcp.GetStream();
                    StateObject state = new StateObject();
                    state.client = tcp;
                    NetworkStream stream = state.client.GetStream();
                    if (stream.CanRead)
                    {
                        byte[] buffer = new byte[tcp.ReceiveBufferSize];
                        stream.BeginRead(state.buffer, 0, StateObject.BufferSize, new AsyncCallback(AsyncReadCallBack), state);
                    }
                    //发送用户名和密码
                    SendData(Encoding.ASCII.GetBytes("{'UserName':'" + VmName + "','Password':'" + Password + "'}"));
                }
            }
            catch (Exception ex)
            {
                _isTcpConnected = false;
                _loginSuccess = false;
                this.Invoke(new Action(() =>
                {
                    btn_Connect.Text = "连接";
                    pictureBox1.Image = null;
                }));
            }
        }

        void AsyncReadCallBack(IAsyncResult iar)
        {
            StateObject state = (StateObject)iar.AsyncState;
            if ((state.client == null) || (!state.client.Connected)) return;
            int NumOfBytesRead;
            try
            {
                NetworkStream ns = state.client.GetStream();
                NumOfBytesRead = ns.EndRead(iar);
                if (NumOfBytesRead > 0)
                {
                    receiveDataStartTime = DateTime.Now;
                    byte[] buffer = new byte[NumOfBytesRead];
                    Array.Copy(state.buffer, 0, buffer, 0, NumOfBytesRead);

                    if (!_loginSuccess)
                    {
                        string msg = Encoding.ASCII.GetString(buffer);
                        if (msg.StartsWith("login ok!"))
                        {
                            _loginSuccess = true;
                            this.Invoke(new Action(() =>
                            {
                                btn_Connect.Text = "断开";
                                pictureBox1.Image = null;
                            }));
                        }
                    }
                    else
                    {
                        lock (receiveDataBuffer)
                        {
                            receiveDataBuffer.AddRange(buffer);
                        }
                    }
                    ns.BeginRead(state.buffer, 0, StateObject.BufferSize, new AsyncCallback(AsyncReadCallBack), state);
                }
                else
                {
                    ns.Close();
                    state.client.Close();
                    ns = null;
                    state = null;
                    _isTcpConnected = false;
                    _loginSuccess = false;
                    this.Invoke(new Action(() =>
                    {
                        btn_Connect.Text = "连接";
                        pictureBox1.Image = null;
                    }));
                }
            }
            catch (Exception ex)
            {

            }
        }

        void RecvFromServer()
        {
            TimeSpan ts;
            while (true)
            {
                if (!_loginSuccess)
                {
                    Thread.Sleep(5);
                    continue;
                }
                ts = DateTime.Now - receiveDataStartTime;
                if (ts.TotalSeconds > 5)
                {
                    lock (receiveDataBuffer)
                    {
                        receiveDataBuffer.Clear();
                    }
                    tcpClient.Close();
                    _isTcpConnected = false;
                    _loginSuccess = false;
                    this.Invoke(new Action(() =>
                    {
                        btn_Connect.Text = "连接";
                        pictureBox1.Image = null;
                    }));
                    Thread.Sleep(5);
                    continue;
                }
                if (receiveDataBuffer.Count == 0)
                {
                    Thread.Sleep(5);
                    continue;
                }

                try
                {
                    List<byte[]> cmds = new List<byte[]>();
                    int removeCount = 0;
                    byte[] buffers;
                    lock (receiveDataBuffer)
                    {
                        buffers = receiveDataBuffer.ToArray();
                    }
                    Package7E.UnPack(buffers, ref cmds, ref removeCount);
                    if (removeCount > 0)
                    {
                        lock (receiveDataBuffer)
                        {
                            receiveDataBuffer.RemoveRange(0, removeCount);
                        }
                    }

                    for (int i = 0; i < cmds.Count; i++)
                    {
                        BsonDataReader bsonDataReader = null;
                        try
                        {
                            MemoryStream msShareData = new MemoryStream(cmds[i]);
                            bsonDataReader = new BsonDataReader(msShareData);
                            JsonSerializer jsonSerializer = new JsonSerializer();
                            ShareData shareData = jsonSerializer.Deserialize<ShareData>(bsonDataReader);
                            switch (shareData.Type)
                            {
                                case "Text":
                                    ShowResult(shareData.TextData);
                                    break;
                                case "Image":
                                    ShowImage(shareData.ImgData);
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        finally
                        {
                            if (bsonDataReader != null)
                            {
                                bsonDataReader.Close();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Log.Error("RecvFromSlave Error", ex);
                }
                Thread.Sleep(5);
            }
        }

        private void ShowResult(string result)
        {
            this.Invoke(new Action(() => { lb_result.Text = result; }));

        }

        private void ShowImage(byte[] cmd)
        {
            byte[] arr = Decompress(cmd, cmd.Length);
            if (arr != null)
            {
                MemoryStream ms = new MemoryStream(arr);
                Image img;
                if (getImage(ms, out img))
                {
                    remoteDesktopWidth = img.Width;
                    remoteDesktopHeight = img.Height;
                    //quzhixun del this 修改下面代码。
                    //pictureBox1.Image = img;
                    this.Invoke(new Action(() =>
                    {
                        Graphics g = Graphics.FromHwnd(pictureBox1.Handle);
                        g.DrawImage(img, pictureBox1.DisplayRectangle);
                    }));
                    img.Dispose();
                }
            }
        }

        private bool getImage(MemoryStream ms, out Image image)
        {
            try
            {
                image = Image.FromStream(ms);
                return true;
            }
            catch
            {
                image = null;
                return false;
            }
        }

        /// <summary>
        /// 解压缩流
        /// </summary>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        public byte[] Decompress(Byte[] bytes, int len)
        {
            try
            {
                using (MemoryStream tempMs = new MemoryStream())
                {
                    using (MemoryStream ms = new MemoryStream(bytes, 0, len))
                    {
                        GZipStream Decompress = new GZipStream(ms, CompressionMode.Decompress);
                        Decompress.CopyTo(tempMs);
                        Decompress.Close();
                        return tempMs.ToArray();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public void SendData(byte[] data)
        {
            if (tcpClient == null || tcpClient.Client == null || !tcpClient.Connected)
            {
                return;
            }
            if (ns != null)
            {
                try
                {
                    if (ns.CanWrite)
                    {
                        data = Package7E.Pack(data);
                        ns.BeginWrite(data, 0, data.Length, new AsyncCallback(AsyncWriteCallBack), ns);
                    }
                }
                catch (IOException ioe)
                {
                    if (!_isTcpConnected)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        void AsyncWriteCallBack(IAsyncResult iar)
        {
            try
            {
                NetworkStream ns = (NetworkStream)iar.AsyncState;
                ns.EndWrite(iar);
            }
            catch (Exception ex)
            {
            }
        }



        private void cbb_SizeMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (cbb_SizeMode.SelectedIndex == 0)
            //{
            //    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            //}
            //else
            //{
            //    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            //}
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            SendData(Encoding.ASCII.GetBytes("{'Msg':" + Win32API.WM_Start + "}"));
        }

        public void Start()
        {
            btn_start.PerformClick();
        }


        private void btn_FullScreen_Click(object sender, EventArgs e)
        {
            if (btn_FullScreen.Text == "退出")
            {
                Win32API.ShowCursor(1);
                (this.Parent as Form).Close();
                btn_FullScreen.Text = "全屏";
                oldParent.Controls.Add(this);
                oldParent.Controls.SetChildIndex(this, oldIndex);
            }
            else
            {
                oldParent = this.Parent;
                oldIndex = oldParent.Controls.GetChildIndex(this);
                btn_FullScreen.Text = "退出";
                Form frmFullScreen = new Form();
                frmFullScreen.FormBorderStyle = FormBorderStyle.None;
                frmFullScreen.WindowState = FormWindowState.Maximized;
                frmFullScreen.Controls.Add(this);
                frmFullScreen.ShowDialog();
            }
        }

        public void ExitFullScreen()
        {
            btn_FullScreen.PerformClick();
        }
    }
}
