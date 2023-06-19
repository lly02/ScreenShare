using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace ScreenShare.Network.P2P.Payload;

public class Payload
{
    private Decoder _decoder;

    public Payload()
    {
        _decoder = new Decoder();
    }

    public Payload AddPayload(byte[] bytes)
    {
        if (_decoder.IsFinal == true)
        {
            var payload = new Payload();
            payload.AddPayload(bytes);
            return payload;
        }

        _decoder.Decode(bytes);

        return this;
    }
}
