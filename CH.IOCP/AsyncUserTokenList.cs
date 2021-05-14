using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH.IOCP
{
    /// <summary>
    /// 实时用户集合
    /// </summary>
    public class AsyncUserTokenList : Object
    {
        private List<AsyncUserToken> m_clientlist;

        public AsyncUserTokenList()
        {
            m_clientlist = new List<AsyncUserToken>();
        }

        /// <summary>
        /// 用户集合
        /// </summary>
        public List<AsyncUserToken> ClientList
        {
            get { return m_clientlist; }
            set { m_clientlist = value; }
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="userToken">用户信息</param>
        public void Add(AsyncUserToken userToken)
        {
            lock (m_clientlist)
            {
                m_clientlist.Add(userToken);
            }
        }
        /// <summary>
        /// 删除一个用户SocketUserToken
        /// </summary>
        /// <param name="userToken">SocketUserToken</param>
        public void Remove(AsyncUserToken userToken)
        {
            lock (m_clientlist)
            {
                m_clientlist.Remove(userToken);
            }
        }
        /// <summary>
        /// 复制给SocketUserToken[]
        /// </summary>
        /// <param name="array">ref SocketUserToken[]</param>
        public void CopyList(out AsyncUserToken[] array)
        {
            lock (m_clientlist)
            {
                array = new AsyncUserToken[m_clientlist.Count];
                m_clientlist.CopyTo(array);
            }
        }
    }
}
