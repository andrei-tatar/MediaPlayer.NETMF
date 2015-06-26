using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.Security;

namespace Mp.Email
{
    public delegate void EmailSentEventHandler(bool succesfull);

    internal static class EmailUtils
    {
        private static SslProtocols[] _usedProtocols;

        public const int TimeoutMs = 10000;
        public static SslProtocols[] UsedProtocols { get { return _usedProtocols ?? (_usedProtocols = new SslProtocols[] { SslProtocols.SSLv3, SslProtocols.TLSv1 }); } }
    }
}
