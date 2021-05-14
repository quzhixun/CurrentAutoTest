using CH.Base;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CH.IOCP
{
    public delegate void AsyncDataEventHandler(AsyncUserToken userToken, byte[] data);
    /// <summary>
    /// 服务器-同步监听端口、异步发送数据
    /// </summary>
    public class AsyncTCPServer
    {
        /// <summary>
        /// 是否初始化
        /// </summary>
        private bool _isInited = false;
        /// <summary>
        /// 用于监听传入连接请求的套接字
        /// </summary>
        private Socket listenSocket;

        private bool _isListening = false;
        /// <summary>
        /// 是否已启动监听
        /// </summary>
        public bool IsListening
        {
            get { return _isListening; }
        }

        /// <summary>
        /// 用户池定义
        /// </summary>
        private AsyncUserTokenPool m_asyncUserTokenPool;

        /// <summary>
        /// 已连接客户端列表
        /// </summary>
        private AsyncUserTokenList m_asyncUserTokenList;
        public AsyncUserTokenList AsyncUserTokenList
        {
            get { return m_asyncUserTokenList; }
        }

        private int _maxConnections = 50000;
        /// <summary>
        /// 最大连接数量
        /// </summary>
        public int MaxConnections
        {
            get { return _maxConnections; }
        }

        /// <summary>
        /// 限制访问接收连接的线程数，用来控制最大并发数-如果有numConnections 线程全部阻塞,等待一个用户退出,才能继续
        /// </summary>
        private Semaphore m_maxNumberAcceptedClients;

        private int _recvBufferSize = 2048;
        /// <summary>
        /// 接收缓存大小
        /// </summary>
        public int RecvBuffSize
        {
            get { return _recvBufferSize; }
        }

        /// <summary>
        /// 监听IP-端口
        /// </summary>
        private string _listenIp = "127.0.0.1";
        public string ListenIp
        {
            get { return _listenIp; }
        }
        private int _listenPort = 1021;
        public int ListenPort
        {
            get { return _listenPort; }
        }
        private IPEndPoint _localEndPoint;
        public IPEndPoint LocalEndPoint
        {
            get { return _localEndPoint; }
        }

        public event EventHandler<AsyncUserToken> ClientConnected;
        public event EventHandler<AsyncUserToken> ClientDisConnected;
        public event AsyncDataEventHandler ClientReceiveData;

        /// <summary>
        /// 创建一个未初始化的服务器实例。要启动监听连接请求的服务器，请调用start方法
        /// </summary>
        /// <param name="listenIP">监听IP</param>
        /// <param name="listenPort">监听端口</param>
        /// <param name="maxConnections">最大连接数</param>
        /// <param name="recvBufferSize">接收缓存区大小</param>
        public AsyncTCPServer(string listenIP, int listenPort, int maxConnections = 50000, int recvBufferSize = 2048)
        {
            _listenIp = listenIP;
            _listenPort = listenPort;
            _localEndPoint = new IPEndPoint(IPAddress.Parse(listenIP), listenPort);

            _maxConnections = maxConnections;
            _recvBufferSize = recvBufferSize;
            m_asyncUserTokenList = new AsyncUserTokenList();
            m_asyncUserTokenPool = new AsyncUserTokenPool(_maxConnections);
            m_maxNumberAcceptedClients = new Semaphore(_maxConnections, _maxConnections);
        }

        /// <summary>
        /// 通过预先分配可重用缓冲区和上下文对象来初始化服务器。
        /// 这些对象不需要预先分配或重用，但是这样做是为了说明如何轻松地使用API创建可重用对象来提高服务器性能。
        /// </summary>
        private void Init()
        {
            if (_isInited)
            {
                return;
            }
            AsyncUserToken userToken;
            for (int i = 0; i < _maxConnections; i++) //按照连接数建立读写对象
            {
                userToken = new AsyncUserToken(_recvBufferSize);
                userToken.AsyncServer = this;
                //异步回调函数初始化
                userToken.RecvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                userToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                //开辟固定空间
                m_asyncUserTokenPool.Push(userToken);
            }
            _isInited = true;
        }

        /// <summary>
        /// 启动服务器，以便它侦听传入的连接请求。
        /// </summary>
        public ResMsg Start()
        {
            ResMsg rm = new ResMsg();
            if (_isListening)
            {
                rm.Result = true;
                return rm;
            }
            Init();
            listenSocket = new Socket(_localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listenSocket.Bind(_localEndPoint);
                listenSocket.Listen(_maxConnections);
                m_maxNumberAcceptedClients = new Semaphore(_maxConnections, _maxConnections);
                StartAccept(null);
                _isListening = true;
                rm.Result = true;
            }
            catch (Exception ex)
            {
                rm.Message = ex.Message;
            }
            return rm;
        }

        /// <summary>
        /// 开始接受来自客户端的连接请求的操作
        /// </summary>
        /// <param name="acceptEventArgs">在服务器的监听套接字上发出accept操作时使用的上下文对象</param>
        private void StartAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs == null)
            {
                acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                //必须清除套接字，因为上下文对象正在被重用
                acceptEventArgs.AcceptSocket = null;
            }

            m_maxNumberAcceptedClients.WaitOne();
            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArgs);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArgs);
            }
        }
        /// <summary>
        /// 此方法是与套接字关联的回调方法。AcceptAsync操作，并在accept操作完成时调用
        /// </summary>
        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                ProcessAccept(e);
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 添加新客户
        /// </summary>
        /// <param name="e"></param>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.AcceptSocket.RemoteEndPoint == null)
                throw new Exception("服务器停止.");

            //获取接受的客户端连接的套接字，并将其放入用户令牌中
            AsyncUserToken userToken = m_asyncUserTokenPool.Pop();
            m_asyncUserTokenList.Add(userToken);
            userToken.Socket = e.AcceptSocket;
            userToken.Identifier = userToken.Socket.RemoteEndPoint.ToString();
            userToken.ConnectDateTime = DateTime.Now;
            try
            {
                //触发客户端连接事件
                ClientConnected?.Invoke(this, userToken);
                //一旦连接了客户机，就向连接发送一个接收
                bool willRaiseEvent = userToken.Socket.ReceiveAsync(userToken.RecvEventArgs);
                if (!willRaiseEvent)
                {
                    ProcessReceive(userToken.RecvEventArgs);
                }
            }
            catch (Exception ex)
            {

            }
            //接受下一个连接请求
            StartAccept(e);
        }

        /// <summary>
        /// 只要在套接字上完成了接收或发送操作，就会调用此方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">与完成的接收操作相关联的SocketAsyncEventArg</param>
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            AsyncUserToken userToken = e.UserToken as AsyncUserToken;
            try
            {
                //确定刚刚完成的操作类型并调用关联的处理程序
                if (e.LastOperation == SocketAsyncOperation.Receive)
                {
                    ProcessReceive(e);
                }
                else if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    ProcessSend(e);
                }
                else
                {
                    throw new ArgumentException("在套接字上完成的最后一个操作不是接收或发送");
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 此方法在异步接收操作完成时调用。
        /// </summary>
        /// <param name="e"></param>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            AsyncUserToken userToken = e.UserToken as AsyncUserToken;
            if (userToken.Socket == null)
                return;

            userToken.ActiveDateTime = DateTime.Now;
            //检查远程主机是否关闭了连接
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                int offset = e.Offset;
                int count = e.BytesTransferred;
                byte[] data = new byte[count];
                for (int i = offset; i < count; i++)
                {
                    data[i] = e.Buffer[i];
                }

                //触发接收数据事件
                ClientReceiveData?.Invoke(userToken, data);
                //继续异步接收
                bool willRaiseEvent = userToken.Socket.ReceiveAsync(userToken.RecvEventArgs);
                if (!willRaiseEvent)
                    ProcessReceive(userToken.RecvEventArgs);
            }
            else
            {
                userToken.AsyncClose();
            }
        }

        /// <summary>
        /// 清除客户端
        /// </summary>
        /// <param name="userToken"></param>
        public void CloseClientSocket(AsyncUserToken userToken)
        {
            lock (userToken.lockObj)
            {
                userToken.m_sendAsync = false;
                if (userToken.Socket == null)
                {
                    return;
                }
                try
                {
                    //关闭
                    userToken.Socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    //释放引用，并清理缓存
                    userToken.Socket.Close();
                    userToken.Socket = null;
                }
            }
            m_maxNumberAcceptedClients.Release();//增加个一信号量
            m_asyncUserTokenPool.Push(userToken);
            m_asyncUserTokenList.Remove(userToken);
            //触发客户端连接关闭事件
            ClientDisConnected?.Invoke(this, userToken);
        }

        public ResMsg SendData(AsyncUserToken userToken, byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return new ResMsg(false, "要发送的数据不能为空");
            }
            if (userToken.Socket == null || !userToken.Socket.Connected)
            {
                return new ResMsg(false, "连接已断开");
            }
            userToken.SendBuffer.Enqueue(data);
            try
            {
                if (!userToken.m_sendAsync)
                {
                    userToken.m_sendAsync = true;
                    byte[] sendData;
                    if (userToken.SendBuffer.TryDequeue(out sendData) && sendData != null)
                    {
                        return SendAsyncEvent(userToken, sendData, 0, sendData.Length);
                    }
                    userToken.m_sendAsync = false;
                }
                return new ResMsg(true, "");
            }
            catch (Exception ex)
            {
                return new ResMsg { Result = false, Message = ex.Message, RefObj = ex };
            }
        }

        public ResMsg SendData(AsyncUserToken userToken, byte[] buffer, int offset, int count)
        {
            if (buffer == null || buffer.Length == 0 || count <= 1)
            {
                return new ResMsg(false, "要发送的数据不能为空");
            }
            if ((offset + count) > buffer.Length)
            {
                return new ResMsg(false, "要发送的数据长度不足");
            }
            if (userToken.Socket == null || !userToken.Socket.Connected)
            {
                return new ResMsg(false, "连接已断开");
            }
            byte[] data = new byte[count];
            Buffer.BlockCopy(buffer, 0, data, 0, count);
            userToken.SendBuffer.Enqueue(data);
            try
            {
                if (!userToken.m_sendAsync)
                {
                    byte[] sendData;
                    if (userToken.SendBuffer.TryDequeue(out sendData) && sendData != null)
                    {
                        userToken.m_sendAsync = true;
                        return SendAsyncEvent(userToken, sendData, 0, sendData.Length);
                    }
                }
                return new ResMsg(true, "");
            }
            catch (Exception ex)
            {
                return new ResMsg { Result = false, Message = ex.Message, RefObj = ex };
            }
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="userToken">AsyncUserToken</param>
        /// <param name="buffer">byte[]</param>
        /// <param name="offset">int</param>
        /// <param name="count">int</param>
        /// <returns>bool</returns>
        private ResMsg SendAsyncEvent(AsyncUserToken userToken, byte[] buffer, int offset, int count)
        {
            if (userToken.Socket == null || !userToken.Socket.Connected)
            {
                userToken.m_sendAsync = false;
                return new ResMsg(false, "连接已关闭");
            }
            userToken.SendEventArgs.SetBuffer(buffer, offset, count);//设置发送缓冲区域
            bool willRaiseEvent = userToken.Socket.SendAsync(userToken.SendEventArgs);//异步发送
            if (!willRaiseEvent)
            {
                return ProcessSend(userToken.SendEventArgs);
            }
            //userToken.m_sendAsync = false;
            return new ResMsg(true, "");
        }

        /// <summary>
        /// 异步发送回调函数
        /// </summary>
        /// <param name="asyncEventArgs"></param>
        private ResMsg ProcessSend(SocketAsyncEventArgs sendEventArgs)
        {
            AsyncUserToken userToken = sendEventArgs.UserToken as AsyncUserToken;
            userToken.ActiveDateTime = DateTime.Now;
            if (sendEventArgs.SocketError == SocketError.Success)
            {
                //发送成功，检查缓存中是否还有数据没有发送,有就继续发送
                byte[] data;
                if (userToken.SendBuffer.TryDequeue(out data) && data != null)
                {
                    userToken.m_sendAsync = true;
                    return SendAsyncEvent(userToken, data, 0, data.Length);
                }
                userToken.m_sendAsync = false;
                return new ResMsg(true, "");
            }
            else
            {
                userToken.AsyncClose();
                return new ResMsg(false, "连接已关闭");
            }
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            AsyncUserToken[] asyncUserTokens;
            m_asyncUserTokenList.CopyList(out asyncUserTokens);
            if (asyncUserTokens != null)
            {
                for (int i = 0; i < asyncUserTokens.Length; i++)
                {
                    asyncUserTokens[i].Close();
                }
            }
            _isListening = false;
            listenSocket.Close();
            GC.Collect();
        }
    }
}
