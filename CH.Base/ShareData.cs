using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH.Base
{
    public class ShareData
    {
        private string _type = "";
        private byte[] _imgData = new byte[0];
        private string _textData = "";

        public string Type { get => _type; set => _type = value; }
        public byte[] ImgData { get => _imgData; set => _imgData = value; }
        public string TextData { get => _textData; set => _textData = value; }
    }
}
