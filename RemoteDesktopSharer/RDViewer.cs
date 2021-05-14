using CH.IOCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDesktopSharer
{
    public class RDViewer
    {
        private string _userName = "";
        private AsyncUserToken _userToken = null;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }
        public AsyncUserToken UserToken
        {
            get { return _userToken; }
            set { _userToken = value; }
        }
    }
}
