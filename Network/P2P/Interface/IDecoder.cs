using ScreenShare.Network.P2P.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenShare.Network.P2P.Interface
{
    public interface IDecoder
    {
        public void Decode(byte[] bytes);
        public byte[]? ReadData();
    }
}
