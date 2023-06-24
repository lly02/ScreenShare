using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using ScreenShare.Network.P2P.Interface;
using System.Xml.Serialization;
using Accessibility;

namespace ScreenShare.Network.P2P.Payload;

public class Decoder : IDecoder
{
    public byte[]? Payload { get; private set; }
    public bool IsPayloadFullyReceived { get; private set; } = true;
    public bool IsFinal { get; private set; } = false;
    public bool IsMasked { get; private set; } = false;
    public List<byte[]> PayloadData { get; private set; } = new List<byte[]> ();
    public int PayloadLength { get; private set; } = 0;
    public PayloadType PayloadType { get; private set; }

    private int _currentByte = 0;
    private byte[] _maskingKey = new byte[4];
    private int _step = 1;

    public void Decode(byte[] bytes)
    {
        AppendPayload(bytes);
        //PayloadLength = 0;
        _currentByte = 0;
        //_maskingKey = new byte[4];

        try
        {
            DecodePayload();
        } 
        catch (IncompleteDataException)
        {
            return;
        }
    }

    public byte[]? ReadData()
    {
        if (PayloadData.Count == 0)
        {
            return null;
        }

        byte[] ReturnData = PayloadData[0];
        PayloadData.RemoveAt(0);

        return ReturnData;
    }

    private void DecodePayload()
    {
        switch (_step)
        {
            // 1. First part (FIN, RSV, Opcode)
            case 1:
                string first = Base10toBase2(Payload![_currentByte]);
                CheckIsFinal(first);
                CheckPayloadType(first);
                NextByte();
                NextStep();
                goto case 2;

            // 2. Second part (Mask, payload length)
            case 2:
                if (!CheckByteExists())
                {
                    throw new IncompleteDataException();
                }

                string second = Base10toBase2(Payload![_currentByte]);
                CheckIsMasked(second);
                CheckPayloadLength(Payload[_currentByte]);
                NextByte();
                NextStep();
                goto case 3;

            // 3. Third part (Masking-key)
            case 3:
                if (IsMasked)
                {
                    if (!CheckIsMaskingKeyFullyReceived())
                    {
                        throw new IncompleteDataException();
                    }

                    GetMaskingKey();
                }
                NextStep();
                goto case 4;

            // 4. Decode
            case 4:
                if (!CheckIsPayloadFullyReceived())
                {
                    throw new IncompleteDataException();
                }

                Decode();
                NextStep();
                goto case 5;

            // 5. If have next payload
            case 5:
                if (CheckLeftoverBytes())
                {
                    RemoveUnnecessaryBytes();
                    ResetDecoder();
                    DecodePayload();
                }
                else
                {
                    goto case 6;
                }
                break;

            // 6. No more payload left to decode, reset the decoder.
            case 6:
                ResetDecoder();
                Payload = null;
                break;

        }
    }

    private string Base10toBase2(int base10)
    {
        string base2 = Convert.ToString(base10, 2);
        if (base2.Length != 8)
        {
            string add0 = "";
            for (int i = 0; i < 8 - base2.Length; i++)
            {
                add0 += "0";
            }
            base2 = add0 + base2;
        }
        return base2;
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

        if (b <= 125)
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
            GetPayloadLength(8);
            return;
        }

        throw new Exception("Invalid Payload Length " + b);
    }

    private void GetPayloadLength(int iterate)
    {
        string b = "";

        for (int i = 0; i < iterate; i++)
        {
            NextByte();
            b += Base10toBase2(Payload![_currentByte]);
        }

        if (iterate == 4 && b[0] != '0')
        {
            throw new Exception("The most significant bit must be 0");
        }

        PayloadLength = Convert.ToInt32(Base2toBase10(b));
    }

    private bool CheckIsPayloadFullyReceived()
    {
        IsPayloadFullyReceived = PayloadLength - 1 <= Payload!.Length - _currentByte;
        return IsPayloadFullyReceived;
    }

    private bool CheckIsMaskingKeyFullyReceived()
    {
        return Payload!.Length >= _currentByte + 3;
    }

    private void GetMaskingKey()
    {
        for (int i = 0; i < 4; i++)
        {
            _maskingKey[i] = Payload![_currentByte];
            NextByte();
        }
    }

    private void Decode()
    {
        byte[] decoded = new byte[PayloadLength];

        for (int i = 0; i < PayloadLength; i++)
        {
            decoded[i] = (byte)(Payload![_currentByte + i] ^ _maskingKey[i % 4]);
        }

        PayloadData.Add(decoded);
    }

    private void NextByte()
    {
        _currentByte += 1;
    }

    private void NextStep()
    {
        _step += 1;
    }

    private void ResetDecoder()
    {
        //Payload = null;
        IsFinal = false;
        IsMasked = false;
        //PayloadData = new List<StringBuilder>();
        PayloadLength = 0;
        _currentByte = 0;
        _maskingKey = new byte[4];
        _step = 1;
    }
    
    private void RemoveUnnecessaryBytes()
    {
        int NewPayloadLength = Payload!.Length - (PayloadLength + _currentByte);
        byte[] NewPayload = new byte[NewPayloadLength];

        Array.Copy(Payload, PayloadLength + _currentByte, NewPayload, 0, NewPayloadLength);
        Payload = NewPayload;
    }

    private bool CheckLeftoverBytes()
    {
        return Payload!.Length - _currentByte > PayloadLength;
    }

    private bool CheckByteExists()
    {
        return Payload!.Length > _currentByte;
    }

    private void AppendPayload(byte[] bytes)
    {
        if (Payload == null)
        {
            Payload = bytes;
            return;
        }

        int NewPayloadLength = bytes.Length + Payload!.Length;
        byte[] NewPayload = new byte[NewPayloadLength];

        Payload.CopyTo(NewPayload, 0);
        bytes.CopyTo(NewPayload, Payload.Length);
        Payload = NewPayload;
    }
}
