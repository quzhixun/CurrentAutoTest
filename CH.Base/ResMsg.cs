using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH.Base
{
    public class ResMsg
    {
        private bool _result = false;
        private string _message = "";
        private object _refObj = null;

        public bool Result
        {
            get { return _result; }
            set { _result = value; }
        }
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        public object RefObj
        {
            get { return _refObj; }
            set { _refObj = value; }
        }

        public ResMsg(bool result = false, string message = "", object refObj = null)
        {
            _result = result;
            _message = message;
            _refObj = refObj;
        }
    }
}
