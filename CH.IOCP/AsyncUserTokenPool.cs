using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH.IOCP
{
    internal class AsyncUserTokenPool
    {
        /// <summary>
        /// 使用用户池-(高并发处理,减轻松服务器压力)
        /// 每一次客户端连接的时候,都是从用户池子里面取一个空用户(稳定),而不是重新去开辟一个空间
        /// 避免AsyncUserToken 多次重复在内存中创建,删除
        /// 需要服务启动的时候初始化
        /// </summary>
        private Stack<AsyncUserToken> m_pool;

        public AsyncUserTokenPool(int capacity)
        {
            m_pool = new Stack<AsyncUserToken>(capacity);
        }

        /// <summary>
        /// 把  SocketUserToken =null  的重新取出来加入列队
        /// </summary>
        /// <param name="item">SocketUserToken</param>
        public void Push(AsyncUserToken item)
        {
            if (item == null)
            {
                throw new ArgumentException("项目的用户不能为空！");
            }
            lock (m_pool)
            {
                m_pool.Push(item);
            }
        }
        /// <summary>
        /// 分配SocketUserToken一个地址给你
        /// </summary>
        /// <returns>SocketUserToken</returns>
        public AsyncUserToken Pop()
        {
            lock (m_pool)
            {
                return m_pool.Pop();
            }
        }
        /// <summary>
        /// 用户池大小
        /// </summary>
        public int Count
        {
            get { return m_pool.Count; }
        }
    }
}
