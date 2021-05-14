using CH.Base;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace CH.IOCP
{
    /// <summary>
    /// 用户集合类
    /// </summary>
    public class AsyncUserToken
    {
        private AutoResetEvent recvAutoResetEvent = new AutoResetEvent(true);
        private AutoResetEvent autoresetEvent = new AutoResetEvent(true);
        public AutoResetEvent AutoResetEvent { get { return autoresetEvent; } }

        protected SocketAsyncEventArgs m_recvEventArgs;
        public SocketAsyncEventArgs RecvEventArgs { get { return m_recvEventArgs; } set { m_recvEventArgs = value; } }

        protected SocketAsyncEventArgs m_sendEventArgs;
        public SocketAsyncEventArgs SendEventArgs { get { return m_sendEventArgs; } set { m_sendEventArgs = value; } }

        /// <summary>
        /// 接收数据时初始定义
        /// </summary>
        protected byte[] m_asyncReceiveBuffer;

        private ConcurrentQueue<byte[]> m_sendBuffer = new ConcurrentQueue<byte[]>();
        /// <summary>
        /// 发送缓存
        /// </summary>
        public ConcurrentQueue<byte[]> SendBuffer { get { return m_sendBuffer; } set { m_sendBuffer = value; } }
        /// <summary>
        /// 接收缓存
        /// </summary>
        private ConcurrentQueue<byte[]> m_recvBuffer = new ConcurrentQueue<byte[]>();

        /// <summary>
        /// 待处理队列
        /// </summary>
        private ConcurrentQueue<DataPacket> _pendingDataPackets = new ConcurrentQueue<DataPacket>();
        internal ConcurrentQueue<DataPacket> PendingDataPackets { get { return _pendingDataPackets; } }

        /// <summary>
        /// 是否处于异步发送中
        /// </summary>
        public bool m_sendAsync = false;
        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime ConnectDateTime;
        /// <summary>
        /// 消息时间 心跳时判断
        /// </summary>
        public DateTime ActiveDateTime;

        /// <summary>
        /// 匹配的RTU数量
        /// </summary>
        public int MatchRTUCount = 0;
        /// <summary>
        /// 唯一标识
        /// </summary>
        public string Identifier = string.Empty;

        private Socket m_socket;
        /// <summary>
        /// 连接客户端
        /// </summary>
        public Socket Socket
        {
            get
            {
                return m_socket;
            }
            set
            {
                m_socket = value;
                if (m_socket == null)
                {
                    ClearBuffer();
                }
                //异步监听赋予回调函数
                m_recvEventArgs.AcceptSocket = m_socket;
                m_sendEventArgs.AcceptSocket = m_socket;
            }
        }

        private AsyncTCPServer m_asyncServer = null;
        internal AsyncTCPServer AsyncServer { get { return m_asyncServer; } set { m_asyncServer = value; } }

        public object lockObj = new object();
        public event AsyncDataEventHandler DataReceived;
        public event EventHandler DisConnected;

        /// <summary>
        /// 初始化客户端
        /// </summary>
        /// <param name="asyncReceiveBufferSize">接收缓存大小</param>
        internal AsyncUserToken(int asyncReceiveBufferSize)
        {
            m_socket = null;
            m_recvEventArgs = new SocketAsyncEventArgs();

            //异步回调函数时候的值(this)
            m_recvEventArgs.UserToken = this;
            m_asyncReceiveBuffer = new byte[asyncReceiveBufferSize];
            m_recvEventArgs.SetBuffer(m_asyncReceiveBuffer, 0, m_asyncReceiveBuffer.Length);

            m_sendEventArgs = new SocketAsyncEventArgs();
            m_sendEventArgs.UserToken = this;
        }

        public ResMsg SendData(byte[] data)
        {
            if (m_asyncServer == null)
            {
                return new ResMsg(false, "异步Server不能为空");
            }
            return AsyncServer.SendData(this, data);
        }

        public ResMsg SendData(byte[] data, int offset, int count)
        {
            if (m_asyncServer == null)
            {
                return new ResMsg(false, "异步Server不能为空");
            }
            return AsyncServer.SendData(this, data, offset, count);
        }

        public void ReceiveData(byte[] datas)
        {
            m_recvBuffer.Enqueue(datas);
            ThreadPool.QueueUserWorkItem(new WaitCallback(RecvDataWaitCallBack), this);
        }

        public void RecvDataWaitCallBack(object state)
        {
            AsyncUserToken userToken = (AsyncUserToken)state;
            byte[] data;
            recvAutoResetEvent.WaitOne();
            if (userToken.m_recvBuffer.TryDequeue(out data) && data != null)
            {
                DataReceived?.Invoke(userToken, data);
            }
            recvAutoResetEvent.Set();
        }

        private void ClearBuffer()
        {
            byte[] data;
            while (!m_sendBuffer.IsEmpty)
            {
                m_sendBuffer.TryDequeue(out data);
            }
            while (!m_recvBuffer.IsEmpty)
            {
                m_recvBuffer.TryDequeue(out data);
            }
            DataPacket dataPacket;
            while (!_pendingDataPackets.IsEmpty)
            {
                _pendingDataPackets.TryDequeue(out dataPacket);
            }
        }

        public void Close()
        {
            if (m_asyncServer != null)
            {
                AsyncServer.CloseClientSocket(this);
            }
            MatchRTUCount = 0;
            ClearBuffer();
            DisConnected?.Invoke(this, new EventArgs());
        }

        public void AsyncClose()
        {
            Thread th = new Thread(new ParameterizedThreadStart(state =>
            {
                AsyncUserToken userToken = state as AsyncUserToken;
                if (userToken != null)
                {
                    try
                    {
                        userToken.Close();
                    }
                    catch (Exception ex)
                    {
                        //LogHelper.LogError(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 关闭连接发生异常", ex);
                    }
                }
            }));
            th.IsBackground = true;
            th.Start(this);
        }
    }
}
