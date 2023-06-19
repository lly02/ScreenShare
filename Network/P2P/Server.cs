using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
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
    private ILogger<Server> _logger;
    private Payload.Decoder _decoder;

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

        NetworkStream stream = _client.GetStream();

        while (true)
        {
            while (!stream.DataAvailable) ;

            while (_client.Available < 3)
            {
                // wait for enough bytes to be available
            }

            byte[] bytes = new byte[_client.Available];

            stream.Read(bytes, 0, bytes.Length);

            //translate bytes of request to string
            string data = Encoding.UTF8.GetString(bytes);

            if (isHandshake(data))
            {
                const string eol = "\r\n"; // HTTP/1.1 defines the sequence CR LF as the end-of-line marker

                byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
                    + "Connection: Upgrade" + eol
                    + "Upgrade: websocket" + eol
                    + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                        System.Security.Cryptography.SHA1.Create().ComputeHash(
                            Encoding.UTF8.GetBytes(
                                new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                            )
                        )
                    ) + eol
                    + eol);

                stream.Write(response, 0, response.Length);
            }
            else
            {
                _decoder.Decode(bytes);
                _logger.LogInformation(_decoder.PayloadData.ToString());
            }
        }
    }

    public bool isHandshake(string data)
    {
        return new Regex("^GET").IsMatch(data);
    }
}
