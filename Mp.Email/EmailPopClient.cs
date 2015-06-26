using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Net.Security;

namespace Mp.Email
{
    public sealed class EmailPopClient
    {
        private Socket _socket;
        private string _user, _password, _serverAddress;
        private ushort _serverPort;
        private bool _useEncryption;

        NetworkStream _dataStream;

        private string WriteLine(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text + "\r\n");
            _dataStream.Write(buffer, 0, buffer.Length);
            return ReadLine();
        }

        private string ReadLine()
        {
            string res = string.Empty;
            byte lastByte = 0;
            while (true)
            {
                int v = _dataStream.ReadByte();
                if (v == -1) Thread.Sleep(100);
                else
                {
                    byte b = (byte)v;
                    if (b == 0x0A && lastByte == 0x0D)
                        break;
                    else
                    {
                        if (b != 0x0D) res += Convert.ToChar(b);
                        lastByte = b;
                    }
                }
            }
            return res;
        }

        private bool IsValidResponse(string response)
        {
            if (response.Length >= 3 && response[0] == '+' && response[1] == 'O' && response[2] == 'K') return true;
            return false;
        }

        private void DownloadEmail(int index)
        {
            string szTemp = WriteLine("RETR " + index.ToString());
            if (szTemp[0] != '-')
            {
                while (szTemp != ".")
                {
                    Debug.Print(szTemp);
                    szTemp = ReadLine();
                }
            }
            else
            {
                Debug.Print(szTemp);
            }
        }

        public void Connect()
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(_serverAddress);
                if (hostEntry.AddressList.Length == 0) throw new Exception("Could not resolve server address");

                using (Timer _timeoutTimer = new Timer((o) =>
                    {
                        _socket.Close();
                    }, null, EmailUtils.TimeoutMs, Timeout.Infinite))
                {
                    _socket.Connect(new IPEndPoint(hostEntry.AddressList[0], _serverPort));
                }

                if (_useEncryption)
                {
                    _dataStream = new SslStream(_socket);
                    ((SslStream)_dataStream).AuthenticateAsClient(_serverAddress, EmailUtils.UsedProtocols);
                }
                else
                {
                    _dataStream = new NetworkStream(_socket);
                }
                _dataStream.WriteTimeout = EmailUtils.TimeoutMs;
                _dataStream.ReadTimeout = EmailUtils.TimeoutMs;

                if (!IsValidResponse(ReadLine())) throw new Exception("Invalid server");

                if (!IsValidResponse(WriteLine("USER " + _user)) ||
                    !IsValidResponse(WriteLine("PASS " + _password))) throw new Exception("Invalid user name or password");

                Debug.Print(WriteLine("STAT"));

                string szTemp;
                Debug.Print(szTemp = WriteLine("TOP 1 5"));
                if (szTemp[0] != '-')
                {
                    while (szTemp != ".")
                    {
                        Debug.Print(szTemp);
                        szTemp = ReadLine();
                    }
                }
                else
                {
                    Debug.Print(szTemp);
                }
            }
            catch (Exception)
            {
                Disconnect(false);
                throw;
            }
        }

        public void Disconnect()
        {
            Disconnect(true);
        }

        private void Disconnect(bool wasSuccesfullyConnected)
        {
            try
            {
                if (_dataStream != null)
                {
                    if (wasSuccesfullyConnected)
                    {
                        WriteLine("QUIT");
                    }

                    _dataStream.Close();
                    _dataStream.Dispose();
                    _dataStream = null;
                }
                if (_socket != null)
                {
                    _socket.Close();
                    _socket = null;
                }
            }
            catch (Exception) { }
        }

        public EmailPopClient(string user, string password, string serverAddress, ushort port, bool useEncryption)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.ReceiveTimeout = EmailUtils.TimeoutMs;
            _socket.SendTimeout = EmailUtils.TimeoutMs;
            _user = user;
            _password = password;
            _serverAddress = serverAddress;
            _serverPort = port;
            _useEncryption = useEncryption;
        }
    }
}
