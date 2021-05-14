using CH.Base;
using CH.IOCP;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteDesktopSharer
{
    public partial class FrmSharer : Form
    {
        private AsyncTCPServer tcpServer = null;
        private Dictionary<string, RDViewer> rdViewers = new Dictionary<string, RDViewer>();
        private bool shareDesktop = false;
        private List<byte> recvBuffer = new List<byte>();

        private ImageCodecInfo ici = null;

        private IntPtr formHandle = new IntPtr(0);
        private IntPtr dialogHandle = new IntPtr(0);
        private IntPtr startHandle = new IntPtr(0);
        private string ctrlTitle = "";

        public FrmSharer()
        {
            InitializeComponent();
        }

        private void FrmSharer_Load(object sender, EventArgs e)
        {
            ici = getImageCoderInfo("image/jpeg");
            this.ActiveControl = btn_Share;
            btn_Share.Focus();
        }

        private void FrmSharer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (shareDesktop)
            {
                tcpServer.Stop();
            }
        }

        private delegate void ShowLogHandler(string msg);

        private void ShowLog(string msg)
        {
            if (rtb_Log.InvokeRequired)
            {
                this.Invoke(new ShowLogHandler(ShowLog), msg);
            }
            else
            {
                if (rtb_Log.Lines.Length > 2000)
                {
                    rtb_Log.Clear();
                }
                rtb_Log.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + msg + "\r\n");
                rtb_Log.ScrollToCaret();
            }
        }

        private void btn_Share_Click(object sender, EventArgs e)
        {
            if (!shareDesktop)
            {
                if (tcpServer != null)
                {
                    tcpServer.ClientDisConnected -= TcpServer_ClientDisConnected;
                    tcpServer.ClientReceiveData -= TcpServer_ClientReceiveData;
                }
                tcpServer = new AsyncTCPServer(IPAddress.Any.ToString(), Convert.ToInt32(txt_Port.Text), 50);
                tcpServer.ClientDisConnected += TcpServer_ClientDisConnected;
                tcpServer.ClientReceiveData += TcpServer_ClientReceiveData;
                ResMsg rm = tcpServer.Start();
                if (!rm.Result)
                {
                    ShowLog("开始分享失败，原因：" + rm.Message);
                    return;
                }
                shareDesktop = true;
                Thread thShare = new Thread(new ThreadStart(ShareDesktop));
                thShare.IsBackground = true;
                thShare.Start();

                Thread thParseCmd = new Thread(new ThreadStart(ParseCmds));
                thParseCmd.IsBackground = true;
                thParseCmd.Start();
                btn_Share.Text = "停止分享";
                ShowLog("开始分享");
            }
            else
            {
                shareDesktop = false;
                tcpServer.Stop();
                btn_Share.Text = "开始分享";
                ShowLog("停止分享");
            }
        }

        private void TcpServer_ClientDisConnected(object sender, AsyncUserToken e)
        {
            RDViewer viewer = null;
            lock (rdViewers)
            {
                if (rdViewers.ContainsKey(e.Identifier))
                {
                    viewer = rdViewers[e.Identifier];
                    rdViewers.Remove(e.Identifier);
                }
            }
            if (viewer != null)
            {
                viewer.UserToken = null;
                ShowLog(viewer.UserName + "停止观看");
            }
        }

        private void TcpServer_ClientReceiveData(AsyncUserToken userToken, byte[] data)
        {
            if (!shareDesktop)
            {
                return;
            }
            bool isExist = false;
            lock (rdViewers)
            {
                if (rdViewers.ContainsKey(userToken.Identifier))
                {
                    isExist = true;
                }
            }
            if (!isExist)
            {
                ResMsg rm = Authentication(data);
                if (rm.Result)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(state =>
                    {
                        AsyncUserToken asyncUserToken = (AsyncUserToken)state;
                        asyncUserToken.SendData(Encoding.ASCII.GetBytes("login ok!"));
                    }), userToken);
                    RDViewer viewer = new RDViewer()
                    {
                        UserName = rm.Message,
                        UserToken = userToken
                    };
                    lock (rdViewers)
                    {
                        rdViewers.Add(userToken.Identifier, viewer);
                    }
                    ShowLog(viewer.UserName + "开始观看，来源：" + userToken.Identifier);
                }
            }
            else
            {
                lock (recvBuffer)
                {
                    recvBuffer.AddRange(data);
                }
            }
        }

        /// <summary>
        /// 观看者身份验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private ResMsg Authentication(byte[] data)
        {
            ResMsg rm = new ResMsg();
            if (data == null || data.Length == 0)
            {
                return rm;
            }
            List<byte[]> cmds = new List<byte[]>();
            int removeCount = 0;
            Package7E.UnPack(data, ref cmds, ref removeCount);

            for (int i = 0; i < cmds.Count; i++)
            {
                JObject msg = JsonConvert.DeserializeObject<JObject>(Encoding.ASCII.GetString(cmds[i]));
                if (msg != null && msg.ContainsKey("Password") && msg["Password"].ToString() == txt_Password.Text)
                {
                    rm.Result = true;
                    rm.Message = msg["UserName"].ToString();
                    break;
                }
            }
            return rm;
        }

        /// <summary>
        /// 解析命令线程
        /// </summary>
        private void ParseCmds()
        {
            while (shareDesktop)
            {
                if (recvBuffer.Count == 0)
                {
                    Thread.Sleep(5);
                    continue;
                }
                try
                {
                    List<byte[]> cmds = new List<byte[]>();
                    int removeCount = 0;
                    byte[] buffers;
                    lock (recvBuffer)
                    {
                        buffers = recvBuffer.ToArray();
                    }
                    Package7E.UnPack(buffers, ref cmds, ref removeCount);
                    if (removeCount > 0)
                    {
                        lock (recvBuffer)
                        {
                            recvBuffer.RemoveRange(0, removeCount);
                        }
                    }

                    for (int i = 0; i < cmds.Count; i++)
                    {
                        try
                        {
                            ExecuteCmd(Encoding.ASCII.GetString(cmds[i]));
                        }
                        catch (Exception ex)
                        {

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

        /// <summary>
        /// 执行键盘和鼠标命令
        /// </summary>
        /// <param name="cmd"></param>
        private void ExecuteCmd(string cmd)
        {
            JObject msg = JsonConvert.DeserializeObject<JObject>(cmd);
            if (msg == null || !msg.ContainsKey("Msg"))
            {
                return;
            }
            switch (Convert.ToInt32(msg["Msg"].ToString()))
            {
                case Win32API.WM_KEYDOWN:        //键盘按下事件　
                case Win32API.WM_SYSKEYDOWN:
                    Win32API.keybd_event(Convert.ToByte(msg["WParam"].ToString()), 0, 0, 0);
                    break;
                case Win32API.WM_KEYUP:          //键盘弹起事件
                case Win32API.WM_SYSKEYUP:
                    Win32API.keybd_event(Convert.ToByte(msg["WParam"].ToString()), 0, 2, 0);
                    break;
                case Win32API.WM_MOUSEMOVE:      //鼠标移动
                    Win32API.SetCursorPos(Convert.ToInt32(msg["X"].ToString()), Convert.ToInt32(msg["Y"].ToString()));
                    break;
                case Win32API.WM_LBUTTONDOWN:    //鼠标左键按下
                case Win32API.WM_LBUTTONDBLCLK:
                    Win32API.mouse_event(MouseEventFlags.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    break;
                case Win32API.WM_RBUTTONDOWN:    //鼠标右键按下
                case Win32API.WM_RBUTTONDBLCLK:
                    Win32API.mouse_event(MouseEventFlags.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                    break;
                case Win32API.WM_MBUTTONDOWN:    //鼠标中键按下
                case Win32API.WM_MBUTTONDBLCLK:
                    Win32API.mouse_event(MouseEventFlags.MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                    break;
                case Win32API.WM_LBUTTONUP:      //鼠标左键弹起
                    Win32API.mouse_event(MouseEventFlags.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;
                case Win32API.WM_RBUTTONUP:      //鼠标右键弹起
                    Win32API.mouse_event(MouseEventFlags.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;
                case Win32API.WM_MBUTTONUP:      //鼠标中键弹起
                    Win32API.mouse_event(MouseEventFlags.MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                    break;
                case Win32API.WM_MOUSEWHEEL:     //鼠标滚轮
                    Win32API.mouse_event(MouseEventFlags.MOUSEEVENTF_WHEEL, 0, 0, Convert.ToInt32(msg["WParam"].ToString()), 0);
                    break;
                case Win32API.WM_Start://启动
                    ShowLog("收到启动命令");
                    formHandle = Win32API.FindWindow(null, ConfigurationManager.AppSettings["FormTitle"]);
                    IntPtr intPtr = Win32API.FindWindowEx(formHandle, new IntPtr(0), "AfxFrameOrView100u", null);
                    dialogHandle = Win32API.FindWindowEx(intPtr, IntPtr.Zero, "#32770", null);
                    startHandle = Win32API.FindWindowEx(dialogHandle, IntPtr.Zero, "Button", ConfigurationManager.AppSettings["StartBtnText"]);
                    if (IntPtr.Zero != startHandle)
                    {
                        Win32API.SetForegroundWindow(formHandle);
                        Win32API.PostMessage(startHandle, Win32API.BM_CLICK, 0, 0);
                        ShowLog("找到启动按钮并点击");
                        Thread ResultThread = new Thread(ReturnResult);
                        ResultThread.IsBackground = true;
                        ResultThread.Start();
                    }
                    break;
                default:
                    break;
            }
        }

        //监视启动结果并返回
        private void ReturnResult()
        {
            string passStr = ConfigurationManager.AppSettings["PassBtnText"];
            string failStr = ConfigurationManager.AppSettings["FailBtnText"];
            int timeout = int.Parse(ConfigurationManager.AppSettings["ResultTimeout"]);

            DateTime start = DateTime.Now;
            while (true)
            {
                if ((DateTime.Now - start).TotalMilliseconds > timeout)
                {
                    ShowLog("等待超时");
                    SendResult(failStr);
                    break;
                }

                IntPtr errdialogHandle = Win32API.FindWindow(null, "Longcheer_Solution_Centre");
                if (errdialogHandle != IntPtr.Zero)
                {
                    IntPtr errHandle = Win32API.FindWindowEx(errdialogHandle, IntPtr.Zero, "Button", "确定");
                    Win32API.PostMessage(errHandle, Win32API.BM_CLICK, 0, 0);
                }

                if (Win32API.FindWindowEx(dialogHandle, IntPtr.Zero, "Static", passStr) != IntPtr.Zero)
                {
                    ShowLog("启动成功");
                    SendResult(passStr);
                    break;
                }
                if (IntPtr.Zero != Win32API.FindWindowEx(dialogHandle, IntPtr.Zero, "Static", failStr))
                {
                    ShowLog("启动失败");
                    SendResult(failStr);
                    break;
                }
                Thread.Sleep(20);

            }
        }


        private void SendResult(string result)
        {
            //Bson打包
            byte[] bs = PackBosn(result);
            //7E打包
            bs = Pack7E(bs);
            RDViewer[] viewers = null;
            lock (rdViewers)
            {
                viewers = new RDViewer[rdViewers.Count];
                rdViewers.Values.CopyTo(viewers, 0);
            }
            for (int i = 0; i < viewers.Length; i++)
            {
                if (viewers[i].UserToken != null)
                {
                    viewers[i].UserToken.SendData(bs);
                }
            }
        }

        /// <summary>
        /// 分享桌面线程
        /// </summary>
        private void ShareDesktop()
        {
            RDViewer[] viewers = null;
            while (shareDesktop)
            {
                if (rdViewers.Count == 0)
                {
                    Thread.Sleep(20);
                    continue;
                }

                try
                {
                    MemoryStream ms = new MemoryStream();
                    //获取屏幕截图并按质量压缩图片，然后将图片保存到内存流
                    CompressImage(GetScreen(), (long)nud_ImageQuality.Value, ms);
                    //gzip压缩字节流
                    byte[] bs = Compress(ms.ToArray());
                    //Bson打包
                    bs = PackBosn(bs);
                    //7E打包
                    bs = Pack7E(bs);

                    lock (rdViewers)
                    {
                        viewers = new RDViewer[rdViewers.Count];
                        rdViewers.Values.CopyTo(viewers, 0);
                    }
                    for (int i = 0; i < viewers.Length; i++)
                    {
                        if (viewers[i].UserToken != null)
                        {
                            viewers[i].UserToken.SendData(bs);
                        }
                    }
                }
                catch { }
                Thread.Sleep(50);
            }
        }

        private byte[] PackBosn(byte[] bs)
        {
            ShareData shareData = new ShareData
            {
                Type = "Image",
                ImgData = bs
            };
            MemoryStream ms = new MemoryStream();
            BsonDataWriter bsonDataWriter = new BsonDataWriter(ms);
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(bsonDataWriter, shareData);
            byte[] res = ms.ToArray();
            bsonDataWriter.Close();
            return res;
        }

        private byte[] PackBosn(string result)
        {
            ShareData shareData = new ShareData
            {
                Type = "Text",
                TextData = result
            };
            MemoryStream ms = new MemoryStream();
            BsonDataWriter bsonDataWriter = new BsonDataWriter(ms);
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(bsonDataWriter, shareData);
            byte[] res = ms.ToArray();
            ms.Close();
            return res;
        }

        private byte[] Pack7E(byte[] bs)
        {
            List<byte> cmd = new List<byte>();
            cmd.Add(0x7e);
            foreach (byte b in bs)
            {
                if (b == 0x7e)
                {
                    cmd.Add(0x7d);
                    cmd.Add(0x5e);
                }
                else if (b == 0x7d)
                {
                    cmd.Add(0x7d);
                    cmd.Add(0x5d);
                }
                else
                {
                    cmd.Add(b);
                }
            }
            cmd.Add(0x7e);
            return cmd.ToArray();
        }

        /// <summary>
        /// 获取屏幕截图
        /// </summary>
        /// <returns></returns>
        private Bitmap GetScreen()
        {
            Bitmap bit = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(bit);
            g.CopyFromScreen(0, 0, 0, 0, bit.Size);
            //先截屏后，然后找到鼠标的位置，后将鼠标画上去
            Win32API.CURSORINFO pci;
            pci.cbSize = Marshal.SizeOf(typeof(Win32API.CURSORINFO));
            Win32API.GetCursorInfo(out pci);
            if (pci.hCursor != IntPtr.Zero)
            {
                Cursor cur = new Cursor(pci.hCursor);
                cur.Draw(g, new Rectangle(pci.ptScreenPos.x, pci.ptScreenPos.y, cur.Size.Width, cur.Size.Height));
            }
            g.Dispose();
            return bit;
        }

        /// <summary>
        /// 按质量压缩图片
        /// </summary>
        /// <param name="bitmap">原图片</param>
        /// <param name="ms">内存流</param>
        public void CompressImage(Bitmap bitmap, long quality, MemoryStream ms)
        {
            EncoderParameter ep = null;
            EncoderParameters eps = null;
            try
            {
                if (ici != null && quality < 100)
                {
                    eps = new EncoderParameters(1);
                    ep = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                    eps.Param[0] = ep;
                    bitmap.Save(ms, ici, eps);
                }
                else
                {
                    bitmap.Save(ms, ImageFormat.Jpeg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (ep != null)
                {
                    ep.Dispose();
                    eps.Dispose();
                }
                bitmap.Dispose();
            }
        }

        /// <summary>  
        /// 获取图片编码信息  
        /// </summary>  
        /// <param name="coderType">编码类型</param>  
        /// <returns>ImageCodecInfo</returns>  
        private ImageCodecInfo getImageCoderInfo(string coderType)
        {
            ImageCodecInfo[] iciS = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo retIci = null;
            foreach (ImageCodecInfo ici in iciS)
            {
                if (ici.MimeType.Equals(coderType))
                    retIci = ici;
            }
            return retIci;
        }

        /// <summary>
        /// 压缩流
        /// </summary>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                GZipStream Compress = new GZipStream(ms, CompressionMode.Compress);
                Compress.Write(bytes, 0, bytes.Length);
                Compress.Close();
                return ms.ToArray();
            }
        }
    }
}
