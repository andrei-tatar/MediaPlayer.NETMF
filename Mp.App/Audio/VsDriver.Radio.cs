using Microsoft.SPOT;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Mp.App.Audio
{
    delegate void VsRadioMetaDataReceivedHandler(string metaData);

    static partial class VsDriver
    {
        private static Socket socket = null;

        public static event VsRadioMetaDataReceivedHandler VsRadioMetaDataReceived;

        private static bool connectToStation(string server, string path, int port)
        {
            if (status != VsStatus.Stopped) return false;
            status = VsStatus.Connecting;
            if (VsStatusChanged != null) VsStatusChanged(status);

            vsThread = new Thread(() =>
            {
                bool noMoreDataInvoked = true;
                const int timeoutMs = 5000;

                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Linger, new byte[] { 0, 0, 0, 0 });
                    //socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 10 * 1024);
                    socket.ReceiveTimeout = timeoutMs;
                    socket.SendTimeout = timeoutMs;

                    IPHostEntry hostEntry = Dns.GetHostEntry(server);
                    socket.Connect(new IPEndPoint(hostEntry.AddressList[0], port));

                    #region send the request
                    byte[] header = Encoding.UTF8.GetBytes(
                        "GET " + path + " HTTP/1.0\r\n" +
                        "Icy-MetaData: 1\r\n" +
                        "\r\n");

                    socket.Send(header);
                    #endregion

                    int metaInt = -1;

                    #region read the response
                    string aux = string.Empty;
                    bool stayInLoop = true;
                    byte[] oneByteBuffer = new byte[1];
                    do
                    {
                        if (!socket.Poll(timeoutMs * 1000, SelectMode.SelectRead)) throw new Exception("Socket timeout");
                        if (socket.Available == 0) throw new Exception("Connection closed");
                        socket.Receive(oneByteBuffer);
                        int b = oneByteBuffer[0];

                        switch (b)
                        {
                            case 13: break;
                            case 10:
                                if (aux.Length == 0)
                                {
                                    stayInLoop = false;
                                    break;
                                }

                                Debug.Print(aux);
                                aux = aux.ToLower();
                                if (aux.IndexOf("icy-metaint:") != -1)
                                    metaInt = Convert.ToInt32(aux.Split(new char[] { ':' })[1]);
                                aux = string.Empty;
                                break;

                            default:
                                aux += (char)b;
                                break;
                        }
                    } while (stayInLoop);
                    #endregion

                    if (metaInt == -1)
                    {
                        RaiseException("Invalid response");
                        return;
                    }

                    int inUse = 0;
                    string metaInfoString;
                    int toMeta = metaInt, metaSize, metaInfoRead, toRead = toMeta < BUFFER_SIZE ? toMeta : BUFFER_SIZE;

                    byte[] metaInfo = new byte[512]; //max meta info size?
                    noMoreDataInvoked = false;

                    status = VsStatus.PlayingRadio;
                    if (VsStatusChanged != null) VsStatusChanged(status);

                    Thread.Sleep(500);

                    if (!socket.Poll(timeoutMs * 1000, SelectMode.SelectRead)) throw new Exception("Socket timeout");
                    if (socket.Available == 0) throw new Exception("Connection closed");

                    int read = socket.Receive(buffer[inUse], toRead, SocketFlags.None);
                    toMeta = metaInt - read;

                    while (true)
                    {
                        if (read != 0)
                        {
                            vsStreamData.InvokeEx(buffer[inUse], read);
                            inUse ^= 1;
                        }

                        #region check and read the meta info
                        if (toMeta == 0)
                        {
                            if (!socket.Poll(timeoutMs * 1000, SelectMode.SelectRead)) throw new Exception("Socket timeout");
                            if (socket.Available == 0) throw new Exception("Connection closed");
                            socket.Receive(oneByteBuffer);
                            metaSize = oneByteBuffer[0] << 4;

                            if (metaSize != 0)
                            {
                                metaInfoRead = 0;
                                do
                                {
                                    if (!socket.Poll(timeoutMs * 1000, SelectMode.SelectRead)) throw new Exception("Socket timeout");
                                    if (socket.Available == 0) throw new Exception("Connection closed");
                                    metaInfoRead += socket.Receive(metaInfo, metaInfoRead, metaSize - metaInfoRead, SocketFlags.None);
                                }
                                while (metaInfoRead != metaSize);
                                metaInfo[metaSize] = 0;
                                metaInfoString = new string(UTF8Encoding.UTF8.GetChars(metaInfo));
                                if (VsRadioMetaDataReceived != null) VsRadioMetaDataReceived(metaInfoString);
                            }

                            toMeta = metaInt;
                        }
                        #endregion

                        toRead = toMeta < BUFFER_SIZE ? toMeta : BUFFER_SIZE;
                        if (toRead != 0)
                        {
                            if (!socket.Poll(timeoutMs * 1000, SelectMode.SelectRead)) throw new Exception("Socket timeout");
                            if (socket.Available == 0) throw new Exception("Connection closed");
                            if ((read = socket.Receive(buffer[inUse], toRead, SocketFlags.None)) == -1)
                            {
                                vsNoMoreData.Invoke();
                                noMoreDataInvoked = true;
                                break;
                            }
                            toMeta -= read;
                            wait.WaitOne();
                        }
                        else read = 0;
                    }
                }
                catch (ThreadAbortException)
                { }
                catch (Exception ex)
                { RaiseException(ex.Message); }
                finally
                {
                    if (!noMoreDataInvoked)
                        vsNoMoreData.Invoke();

                    if (socket != null)
                    {
                        socket.Close();
                        socket = null;
                    }

                    status = VsStatus.Stopped;
                    if (VsStatusChanged != null) VsStatusChanged(status);
                }
            }) { Priority = ThreadPriority.Highest };
            vsThread.Start();
            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool StopRadio()
        {
            if (status != VsStatus.PlayingRadio && status != VsStatus.Connecting) return false;
            vsThread.Abort();
            vsThread.Join();
            if (status == VsStatus.Connecting)
            {
                if (socket != null)
                {
                    socket.Close();
                    socket = null;
                }
            }
            return true;
        }

        public static bool ConnectToStation(string url)
        {
            string server, path;
            int port;
            ParseRadioUrl(url, out server, out path, out port);
            return ConnectToStation(server, path, port);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool ConnectToStation(string server, string path, int port)
        {
            if (!initialized)
            {
                RaiseException("VS Driver not initialized");
                return false;
            }
            return connectToStation(server, path, port);
        }

        private static void ParseRadioUrl(string inUrl, out string server, out string path, out int port)
        {
            const int DEFAULT_PORT = 8000;
            const string DEFAULT_PATH = "/";

            inUrl = inUrl.ToLower();
            int index;

            //if it starts with http://, remove it
            if ((index = inUrl.IndexOf("http://")) == 0)
                inUrl = inUrl.Substring(7, inUrl.Length - 7);

            //get index of server port separator
            if ((index = inUrl.IndexOf(':')) != -1)
            {
                server = inUrl.Substring(0, index); //get the server part
                inUrl = inUrl.Substring(index, inUrl.Length - index); //remaining (with ':')

                //next numeric chars represent the port number
                string portStr = string.Empty;
                int i = 1;
                while (i < inUrl.Length && inUrl[i] >= '0' && inUrl[i] <= '9')
                    portStr += inUrl[i++];

                //convert port to int
                try { port = int.Parse(portStr); }
                catch (Exception) { port = DEFAULT_PORT; }
            }
            else
            {
                //no port specified
                port = DEFAULT_PORT; //use defaul port

                //get the server address (all chars until slash)
                int i = 0;
                server = string.Empty;
                while (i < inUrl.Length && i != '/')
                    server += inUrl[i++];
            }

            //get the first index of the slash, all chars after slash (including) are the path
            if ((index = inUrl.IndexOf('/')) != -1)
                path = inUrl.Substring(index, inUrl.Length - index);
            else
                path = DEFAULT_PATH; //use default path

            //path should start with slash
            if (path[0] != '/') path = '/' + path;
        }
    }
}
