using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH.IOCP
{
    internal class DataPacket
    {
        private AsyncUserToken _userToken = null;
        private byte[] _data = new byte[0];
        private DataPacketType packetType = DataPacketType.Data;

        public AsyncUserToken UserToken
        {
            get { return _userToken; }
            set { _userToken = value; }
        }
        public byte[] Data
        {
            get { return _data; }
            set { _data = value; }
        }
        public DataPacketType PacketType
        {
            get { return packetType; }
            set { packetType = value; }
        }

        public DataPacket() { }
        public DataPacket(AsyncUserToken userToken, byte[] data)
        {
            _userToken = userToken;
            _data = data;
        }
    }

    internal enum DataPacketType
    {
        Data,
        Connect,
        DisConnect
    }
}
