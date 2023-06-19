using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenShare.Network.P2P.Payload;

public class Decoder
{
    public byte[]? Payload;
    public bool IsFinal = false;
    public bool IsMasked = false;
    public StringBuilder PayloadData = new StringBuilder();
    public int PayloadLength = 0;
    public PayloadType PayloadType;

    private int _currentByte = 0;
    private byte[] _maskingKey = new byte[4];

    public void Decode(byte[] bytes)
    {
        Payload = bytes;
        PayloadLength = 0;
        _currentByte = 0;
        _maskingKey = new byte[4];

        // 1. First part (FIN, RSV, Opcode)
        string first = Base10toBase2(Payload[_currentByte]);
        CheckIsFinal(first);
        CheckPayloadType(first);
        Next();

        // 2. Second part (Mask, payload length)
        string second = Base10toBase2(Payload[_currentByte]);
        CheckIsMasked(second);
        CheckPayloadLength(bytes[1]);
        Next();

        // 3. Third part (Masking-key)
        if (IsMasked)
        {
            GetMaskingKey();
        }

        // 4. Decode
        Decode();
    }

    private string Base10toBase2(int base10)
    {
        return Convert.ToString(base10, 2);
    }

    private string Base2toBase10(string base2)
    {
        int base2Int = Convert.ToInt32(base2, 2);
        return Convert.ToString(base2Int, 10);
    }

    private void CheckIsFinal(string s)
    {
        IsFinal = s[0] == '1' ? true : false;
    }

    private void CheckPayloadType(string s)
    {
        if (s.Last() == '0')
        {
            return;
        }
        if (s.Last() == '1')
        {
            PayloadType = PayloadType.Text;
            return;
        }
        if (s.Last() == '2')
        {
            PayloadType = PayloadType.Binary;
            return;
        }

        throw new Exception("Invalid Opcode %x" + s.Last());
    }

    private void CheckIsMasked(string s)
    {
        IsMasked = s[0] == '1' ? true : false;
    }

    private void CheckPayloadLength(int b)
    {
        if (IsMasked)
        {
            b -= 128;
        }

        if (b < 125)
        {
            PayloadLength = b;
            return;
        }
        if (b == 126)
        {
            GetPayloadLength(2);
            return;
        }
        if (b == 127)
        {
            GetPayloadLength(4);
            return;
        }

        throw new Exception("Invalid Payload Length " + b);
    }

    private void GetPayloadLength(int iterate)
    {
        string b = "";

        for (int i = 0; i < iterate; i++)
        {
            Next();
            b += Base10toBase2(Payload![_currentByte]);
        }

        if (iterate == 4 && b[0] != '0')
        {
            throw new Exception("The most significant bit must be 0");
        }

        PayloadLength = Convert.ToInt32(Base2toBase10(b));
    }

    private void GetMaskingKey()
    {
        for (int i = 0; i < 4; i++)
        {
            _maskingKey[i] = Payload![_currentByte];
            Next();
        }
    }

    private void Decode()
    {
        byte[] decoded = new byte[PayloadLength];

        for (int i = 0; i < decoded.Length; i++)
        {
            decoded[i] = (byte)(Payload![_currentByte + i] ^ _maskingKey[i % 4]);
        }

        PayloadData.Append(Encoding.UTF8.GetString(decoded));
    }

    private void Next()
    {
        _currentByte += 1;
    }
}
