using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ScreenShare.Network.P2P;

public class Server
{
    private TcpListener? _server;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private readonly ILogger<Server> _logger;
    private readonly Payload.Decoder _decoder;

    public Server(ILogger<Server> logger, Payload.Decoder decoder)
    {
        _logger = logger;
        _decoder = decoder;
    }

    public void StartServer()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 80;

        _server = new TcpListener(ipAddress, port);
        _server.Start();
        _logger.LogInformation(
            "Server started at {ipAddress}:{port}. Waiting for connection",
            ipAddress, port
        );

        _client = _server.AcceptTcpClient();
        _logger.LogInformation("A client connected.");

        _stream = _client.GetStream();

        while (true)
        {
            while (_client.Available < 3)
            {
            }

            byte[] bytes = new byte[_client!.Available];
            _stream!.Read(bytes, 0, bytes.Length);

            string data = Encoding.UTF8.GetString(bytes);

            if (IsHandshake(data))
            {
                string eol = "\r\n"; // HTTP/1.1 defines the sequence CR LF as the end-of-line marker

                byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
                    + "Connection: Upgrade" + eol
                    + "Upgrade: websocket" + eol
                    + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                        SHA1.Create().ComputeHash(
                            Encoding.UTF8.GetBytes(
                                new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                            )
                        )
                    ) + eol
                    + eol);

                _stream.Write(response, 0, response.Length);

                break;
            }
        }

        while(true)
        {
            List<byte[]?> bytes = ReceiveFullData();

            for (int i = 0; i < bytes.Count; i++)
            {
                string data = Encoding.UTF8.GetString(bytes[i]!);

                _logger.LogInformation(
                    "Payload received \n" +
                    "Data: {_decoder.PayloadData} \n",
                    data + "\n");
            }
        }
    }

    private bool IsHandshake(string data)
    {
        return new Regex("^GET").IsMatch(data);
    }

    private List<byte[]?> ReceiveFullData()
    {
        List<byte[]?> ReadData = new List<byte[]?>(); 

        while (ReadData.Count == 0)
        {
            while (!_stream!.DataAvailable) ;

            byte[] bytes = new byte[_client!.Available];
            _stream!.Read(bytes, 0, bytes.Length);

            _decoder.Decode(bytes);

            while (_decoder.PayloadData.Count > 0)
            {
                ReadData.Add(_decoder.ReadData()!);
            }
        }
        
        return ReadData;
    }
}
