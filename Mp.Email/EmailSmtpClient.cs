using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Net.Security;

namespace Mp.Email
{
    public sealed class EmailSmtpClient
    {
        private Socket _socket;
        NetworkStream _dataStream;
        private string _user, _password, _serverAddress;
        private ushort _serverPort;
        private bool _useEncryption;
        private bool _requireAuth;

        public EmailSmtpClient(string serverAddress, ushort port, bool useEncryption)
            : this(null, null, serverAddress, port, useEncryption)
        {
            _requireAuth = false;
        }

        public EmailSmtpClient(string user, string password, string serverAddress, ushort port, bool useEncryption)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.ReceiveTimeout = EmailUtils.TimeoutMs;
            _socket.SendTimeout = EmailUtils.TimeoutMs;
            _user = user;
            _password = password;
            _serverAddress = serverAddress;
            _serverPort = port;
            _useEncryption = useEncryption;
            _requireAuth = true;
        }

        private void Connect()
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
            }
            catch (Exception)
            {
                Disconnect();
                throw;
            }
        }

        private void Disconnect()
        {
            try
            {
                if (_dataStream != null)
                {
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

        private string WriteReadLine(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text + "\r\n");
            _dataStream.Write(buffer, 0, buffer.Length);
            return ReadLine();
        }

        private void WriteLine(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text + "\r\n");
            _dataStream.Write(buffer, 0, buffer.Length);
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

        public void SendMail(EmailAddress from, EmailAddressCollection to, EmailAddressCollection cc, string subject, string body)
        {
            Connect();
            Debug.Print(ReadLine());
            if (_requireAuth)
            {
                string str = WriteReadLine("EHLO test.domain.com");
                Debug.Print(str);
                while (str[3] == '-') Debug.Print(str = ReadLine());

                byte[] buf = new byte[2 + _user.Length + _password.Length];
                buf[0] = 0;
                byte[] userData = Encoding.UTF8.GetBytes(_user);
                Array.Copy(userData, 0, buf, 1, userData.Length);
                buf[1 + userData.Length] = 0;
                byte[] passData = Encoding.UTF8.GetBytes(_password);
                Array.Copy(passData, 0, buf, 2 + userData.Length, passData.Length);
                Debug.Print(WriteReadLine("AUTH PLAIN " + ConvertBase64.ToBase64String(buf)));
            }
            else
            {
                Debug.Print(WriteReadLine("HELO test.domain.com"));
            }
            Debug.Print(WriteReadLine("MAIL FROM: <" + from.Address + ">"));
            if (to != null)
            {
                foreach (EmailAddress toAddress in to)
                {
                    Debug.Print(WriteReadLine("RCPT TO: <" + toAddress.Address + ">"));
                }
            }
            if (cc != null)
            {
                foreach (EmailAddress ccAddress in cc)
                {
                    Debug.Print(WriteReadLine("RCPT TO: <" + ccAddress.Address + ">"));
                }
            }
            Debug.Print(WriteReadLine("DATA\r\n"));

            #region send data
            WriteLine("From: \"" + from.Name + "\" <" + from.Address + ">");
            if (to != null)
            {
                foreach (EmailAddress toAddress in to)
                {
                    WriteLine("To: \"" + toAddress.Name + "\" <" + toAddress.Address + ">");
                }
            }
            if (cc != null)
            {
                foreach (EmailAddress ccAddress in cc)
                {
                    WriteLine("Cc: \"" + ccAddress.Name + "\" <" + ccAddress.Address + ">");
                }
            }
            WriteLine("Subject: " + subject + "\r\n");
            WriteLine(body);
            Debug.Print(WriteReadLine("."));
            #endregion

            Debug.Print(WriteReadLine("QUIT"));
            Disconnect();
        }

        public void SendAsync(MailMessage message, EmailSentEventHandler doneCallback)
        {
        }
    }
}
